using Discord.Commands;
using Domain.Models.BusinessLayer;
using Domain.Services.Interfaces;
using Domain.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotApplication.Methods
{
  public class CommandHelper
  {
    IServerCommandService _commandService;
    public CommandHelper(IServerCommandService commandService)
    {
      _commandService = commandService;
    }

    public async Task<ServerCommand> AddOrUpdateCommand(SocketCommandContext context, string command)
    {
      var guildId = context.Guild.Id.ToString();
      var parts = StringUtility.SmartSplit(command);
      var key = parts[1];
      var value = parts[2];
      var model = await _commandService.AddOrUpdateCommand(guildId, key, value);

      var confirmationMessage = await context.Channel.SendMessageAsync($"added command !{key} with message {value}");
      await Task.Delay(TimeSpan.FromSeconds(5));
      await confirmationMessage.DeleteAsync();
      return model;
    }
  }
}
