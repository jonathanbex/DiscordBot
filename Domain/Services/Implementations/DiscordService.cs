using Domain.Models.Discord;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSec.Cryptography;
using System.Text;
using System.Text.Json;

namespace Domain.Services.Implementations
{
	public class DiscordService : IDiscordService
	{
		IHttpClientFactory _httpClientFactory;
		IConfiguration _configuration;
		private readonly string _publicKey;
		private readonly ILogger<DiscordService> _logger;
		public DiscordService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<DiscordService> logger)
		{
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
			_publicKey = configuration.GetValue<string>("Discord:PublicKey");
			_logger = logger;
		}

		public async Task HandleInteraction(Interaction interaction)
		{
			// Ensure the interaction contains a message and that it's not from a bot
			if (interaction.Message != null && interaction.Message.Author != null && interaction.Message.Author.Verified == false)
			{
				string content = interaction?.Message?.Content;
				if (content == null) return;
				// Check if the message starts with the '!' command prefix
				if (content.StartsWith("!"))
				{
					// Extract the command and parameters
					var command = content.Split(' ')[0].Substring(1).ToLower(); // e.g., 'clear'
					var parameters = content.Substring(command.Length + 2).Trim(); // The rest of the message

					// Handle the command
					await HandleCommand(interaction, command, parameters);
				}
			}
		}

		private async Task HandleCommand(Interaction interaction, string command, string parameters)
		{
			switch (command)
			{
				case "clear":
					await HandleClearCommand(interaction.ChannelId, parameters, interaction.Token);
					break;

				// Add other command handlers here

				default:
					// Optionally handle unknown commands
					await HandleUnknownCommand(interaction, command);
					break;
			}
		}

		private async Task HandleClearCommand(string? channelId, string parameters, string token)
		{
			if (string.IsNullOrEmpty(channelId))
			{
				// Handle the case where channelId is null or empty
				return;
			}

			// Default to clearing 1 message if no parameter is provided
			int amount = 1;
			if (!string.IsNullOrEmpty(parameters) && int.TryParse(parameters, out int parsedAmount))
			{
				amount = parsedAmount;
			}

			// Construct the payload for bulk delete (this is a simplified example)
			var payload = new
			{
				messages = await GetMessagesToDelete(channelId, amount, token) // Get the message IDs to delete
			};

			var deleteUrl = $"https://discord.com/api/v10/channels/{channelId}/messages/bulk-delete";
			var response = await PostToDiscordApiAsync(deleteUrl, payload, token);

			if (response.IsSuccessStatusCode)
			{
				// Notify success
				await SendMessageToChannel(channelId, $"{amount} message(s) cleared successfully.", token);
			}
			else
			{
				// Handle failure
				var errorContent = await response.Content.ReadAsStringAsync();
				await SendMessageToChannel(channelId, $"Failed to clear messages: {errorContent}", token);
			}
		}

		private async Task<string[]> GetMessagesToDelete(string channelId, int amount, string token)
		{
			var client = _httpClientFactory.CreateClient();

			// Set up the request URL to get messages
			string url = $"https://discord.com/api/v10/channels/{channelId}/messages?limit={amount}";

			// Add the Authorization header using the provided token
			client.DefaultRequestHeaders.Add("Authorization", $"Bot {token}");

			// Send the GET request to fetch messages
			var response = await client.GetAsync(url);

			if (response.IsSuccessStatusCode)
			{
				// Read the response content
				var responseContent = await response.Content.ReadAsStringAsync();

				// Deserialize the JSON into a list of messages
				var messages = JsonSerializer.Deserialize<List<DiscordMessage>>(responseContent);

				// Extract the IDs of the messages
				var messageIds = messages?.Select(m => m.Id).ToArray();

				// Return the message IDs
				return messageIds ?? Array.Empty<string>();
			}
			else
			{
				// Handle failure (e.g., log the error)
				var errorContent = await response.Content.ReadAsStringAsync();
				// Log or handle the error based on the response content
				return Array.Empty<string>();
			}
		}

		private async Task SendMessageToChannel(string? channelId, string messageContent, string token)
		{
			if (string.IsNullOrEmpty(channelId))
			{
				return;
			}

			var url = $"https://discord.com/api/v10/channels/{channelId}/messages";
			var payload = new
			{
				content = messageContent
			};

			await PostToDiscordApiAsync(url, payload, token);
		}

		private async Task<HttpResponseMessage> PostToDiscordApiAsync(string url, object payload, string token)
		{
			var client = _httpClientFactory.CreateClient();

			// Serialize the payload to JSON
			var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

			// Add the Authorization header using the provided token
			client.DefaultRequestHeaders.Add("Authorization", $"Bot {token}");

			// Send the POST request
			var response = await client.PostAsync(url, content);

			return response;
		}

		private async Task HandleUnknownCommand(Interaction interaction, string command)
		{
			// Handle what happens when the bot receives an unknown command
			await SendMessageToChannel(interaction.ChannelId, $"Unknown command: {command}", interaction.Token);
		}

		public async Task<bool> IsValidRequest(HttpRequest request)
		{
			try
			{
				// Retrieve headers
				var signature = request.Headers["X-Signature-Ed25519"].FirstOrDefault();
				var timestamp = request.Headers["X-Signature-Timestamp"].FirstOrDefault();

				if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(timestamp))
				{
					_logger.LogWarning("Missing signature or timestamp.");
					return false;
				}

				// Read the request body
				string body;
				// Read the request body into a memory stream
				using (var memoryStream = new MemoryStream())
				{

					await request.Body.CopyToAsync(memoryStream);

					// Reset memory stream position to the beginning
					memoryStream.Position = 0;

					using (var reader = new StreamReader(memoryStream, Encoding.UTF8, leaveOpen: true))
					{
						body = await reader.ReadToEndAsync();
					}
				}
				_logger.LogInformation("Request body: {0}", body);

				// Combine timestamp and body for validation
				var combined = Encoding.UTF8.GetBytes(timestamp + body);
				_logger.LogInformation("Combined string for verification: {0}", Encoding.UTF8.GetString(combined));

				// Convert the public key and signature from hex to byte arrays
				var publicKeyBytes = ConvertHexStringToByteArray(_publicKey);
				var signatureBytes = ConvertHexStringToByteArray(signature);

				// Create the public key object
				var publicKey = PublicKey.Import(SignatureAlgorithm.Ed25519, publicKeyBytes, KeyBlobFormat.RawPublicKey);

				// Verify the signature
				var isValid = SignatureAlgorithm.Ed25519.Verify(publicKey, combined, signatureBytes);

				_logger.LogInformation("Signature valid: {0}", isValid);

				return isValid;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error verifying the request signature.");
				return false;
			}

		}

		public static byte[] ConvertHexStringToByteArray(string hexString)
		{
			if (hexString.Length % 2 != 0)
			{
				throw new ArgumentException($"Invalid length for a hex string: {hexString.Length}");
			}

			var byteArray = new byte[hexString.Length / 2];
			for (int i = 0; i < byteArray.Length; i++)
			{
				byteArray[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
			}
			return byteArray;
		}

	}
}
