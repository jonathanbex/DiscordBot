﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Domain.Services.Interfaces;
using Domain.Utility;

namespace BotApplication.Methods
{
  public class CommandHelper
  {
    IServerCommandService _commandService;
    public CommandHelper(IServerCommandService commandService)
    {
      _commandService = commandService;
    }

    public async Task AddOrUpdateCommand(SocketCommandContext context, string command)
    {
      if (context.Guild == null || context.User is not SocketGuildUser guildUser)
      {
        return;
      }

      var channel = context.Channel as ITextChannel;

      // Ensure the channel is a text channel
      if (channel == null)
      {

        return;
      }

      // Check if the user has the "Manage Messages" permission in the specific channel
      var permissions = guildUser.GetPermissions(channel);

      if (!permissions.ManageMessages)
      {

        return;
      }
      var guildId = context.Guild.Id.ToString();
      var parts = StringUtility.SmartSplit(command);
      var key = parts[1];
      var value = parts[2];
      try
      {
        var model = await _commandService.AddOrUpdateCommand(guildId, key, value);
      }
      catch (Exception ex)
      {
        var hej = ex;
      }
      var confirmationMessage = await context.Channel.SendMessageAsync($"added command !{key} with message {value}");
      await Task.Delay(TimeSpan.FromSeconds(2));
      await confirmationMessage.DeleteAsync();
    }
  }
}
