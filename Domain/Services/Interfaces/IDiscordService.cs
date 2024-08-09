using Domain.Models.Discord;
using Microsoft.AspNetCore.Http;

namespace Domain.Services.Interfaces
{
	public interface IDiscordService
	{

		Task HandleInteraction(Interaction interaction);
		Task<bool> IsValidRequest(HttpRequest request);
	}
}
