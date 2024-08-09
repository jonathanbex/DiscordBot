using Microsoft.AspNetCore.Mvc;
using Domain.Models.Discord;
using Domain.Services.Interfaces;
namespace DiscordBot.Controllers
{
	public class HookController : ControllerBase
	{
		IDiscordService _discordService;
		private readonly ILogger<HookController> _logger;
		public HookController(IDiscordService discordService, ILogger<HookController> logger)
		{
			_discordService = discordService;
			_logger = logger;
		}
		[HttpPost]
		public async Task<IActionResult> Interactions([FromBody] Interaction model)
		{
			_logger.LogInformation("Received interaction: {@model}", model);
			//if (!await _discordService.IsValidRequest(Request)) return Unauthorized();
			// Handle the PING interaction to verify the endpoint

			_logger.LogInformation("Received interaction: {@model}", model);
			if (model.Type == InteractionType.Ping)
			{
				return new JsonResult(new { type = 1 }) { ContentType = "application/json" };
			}


			await _discordService.HandleInteraction(model);
			_logger.LogInformation("Interaction handled");
			return Ok();
		}
	}
}
