using NSec.Cryptography;
using System.Text;

namespace DiscordBot.Middleware
{
	public class SignatureValidationMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly string _publicKey;

		public SignatureValidationMiddleware(RequestDelegate next, IConfiguration configuration)
		{
			_next = next;
			_publicKey = configuration.GetValue<string>("Discord:PublicKey");
		}

		public async Task InvokeAsync(HttpContext context)
		{

			// Read the request body into a memory stream
			var memoryStream = new MemoryStream();

			// Copy the request body to the memory stream
			await context.Request.Body.CopyToAsync(memoryStream);

			// Reset the memory stream position to the beginning
			memoryStream.Position = 0;

			// Read the body from the memory stream
			using (var reader = new StreamReader(memoryStream, Encoding.UTF8, leaveOpen: true))
			{
				var body = await reader.ReadToEndAsync();
				memoryStream.Position = 0; // Reset position for downstream use

				// Replace the original request body stream with the memory stream
				context.Request.Body = memoryStream;

				// Retrieve headers
				var signature = context.Request.Headers["X-Signature-Ed25519"].FirstOrDefault();
				var timestamp = context.Request.Headers["X-Signature-Timestamp"].FirstOrDefault();

				if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(timestamp))
				{
					context.Response.StatusCode = StatusCodes.Status400BadRequest;
					await context.Response.WriteAsync("Missing signature or timestamp.");
					return;
				}

				// Combine timestamp and body for validation
				var combined = Encoding.UTF8.GetBytes(timestamp + body);

				// Convert the public key and signature from hex to byte arrays
				var publicKeyBytes = ConvertHexStringToByteArray(_publicKey);
				var signatureBytes = ConvertHexStringToByteArray(signature);

				// Create the public key object
				var publicKey = PublicKey.Import(SignatureAlgorithm.Ed25519, publicKeyBytes, KeyBlobFormat.RawPublicKey);

				// Verify the signature
				var isValid = SignatureAlgorithm.Ed25519.Verify(publicKey, combined, signatureBytes);

				if (!isValid)
				{
					context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					await context.Response.WriteAsync("Invalid request signature.");
					return;
				}
			}



			await _next(context);
		}

		private byte[] ConvertHexStringToByteArray(string hexString)
		{
			return Enumerable.Range(0, hexString.Length)
											 .Where(x => x % 2 == 0)
											 .Select(x => Convert.ToByte(hexString.Substring(x, 2), 16))
											 .ToArray();
		}
	}
}
