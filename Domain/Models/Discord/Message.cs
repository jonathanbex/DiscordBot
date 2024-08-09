using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Models.Discord
{
	public class DiscordMessage
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		// Include other properties as needed (e.g., content, author, etc.)
	}
}
