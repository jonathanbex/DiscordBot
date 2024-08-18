using System.Text.Json.Serialization;

namespace Domain.Models.Discord
{
	public class Interaction
	{
		[JsonPropertyName("id")]
		public string? Id { get; set; }
		[JsonPropertyName("application_id")]
		public string? ApplicationId { get; set; }

		[JsonPropertyName("guild_id")]
		public string? GuildId { get; set; }

		[JsonPropertyName("channel_id")]
		public string? ChannelId { get; set; }

		[JsonPropertyName("token")]
		public string? Token { get; set; }

		[JsonPropertyName("type")]
		public InteractionType Type { get; set; }

		[JsonPropertyName("message")]
		public InteractionMessage? Message { get; set; }
	}

	public class InteractionMessage
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("channel_id")]
		public string? ChannelId { get; set; }

		[JsonPropertyName("author")]
		public InteractionUser? Author { get; set; }

		[JsonPropertyName("content")]
		public string? Content { get; set; }

		[JsonPropertyName("timestamp")]
		public DateTime Timestamp { get; set; }
	}

	public class InteractionUser
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("username")]
		public string? Username { get; set; }

		[JsonPropertyName("global_name")]
		public string GlobalName { get; set; }

		[JsonPropertyName("verified")]
		public bool? Verified { get; set; }

	}
	public enum InteractionType
	{
		[JsonPropertyName("1")]
		Ping = 1,

		[JsonPropertyName("2")]
		ApplicationCommand = 2,

		[JsonPropertyName("3")]
		MessageComponent = 3,

		[JsonPropertyName("4")]
		ApplicationCommandAutocomplete = 4,

		[JsonPropertyName("5")]
		ModalSubmit = 5
	}
}
