using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Domain.Services.Interfaces;
using Domain.Utility;

namespace BotApplication.Methods
{
  public class GuildLineupHelper
  {
    IGuildLineupService _guildLineupService;
    public GuildLineupHelper(IGuildLineupService guildLineupService)
    {
      _guildLineupService = guildLineupService;
    }

    public async Task AddOrUpdateLineup(SocketCommandContext context, string command)
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
        await context.User.SendMessageAsync("You are not allowed to do this, you need Manage Messages");
        return;
      }

      var userCurrentTime = context.Message.Timestamp.UtcDateTime;
      var guildId = context.Guild.Id.ToString();

      var parts = StringUtility.SmartSplit(command);
      var name = parts[1];
      var validFor = parts[2];
      var value = parts[3];
      try
      {

        var model = await _guildLineupService.AddOrUpdateLineup(guildId, value, name, userCurrentTime, validFor);
      }
      //only catch notsupported as we throw that if they go over the limit;
      catch (NotSupportedException ex)
      {
        await context.User.SendMessageAsync($"Error in adding lineup {name} - {ex.Message}");
        throw;
      }
      var confirmationMessage = await context.Channel.SendMessageAsync($"added !{name} with lineup {value}");
      await Task.Delay(TimeSpan.FromSeconds(2));
      await confirmationMessage.DeleteAsync();
    }


    public async Task GetLineup(SocketCommandContext context, string command)
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

      var userCurrentTime = context.Message.Timestamp.UtcDateTime;
      var guildId = context.Guild.Id.ToString();

      var parts = StringUtility.SmartSplit(command);
      var key = parts[1];


      if (key.Equals("Today", StringComparison.InvariantCultureIgnoreCase) || key.Equals("Tonight", StringComparison.InvariantCultureIgnoreCase) || key.Equals("Tommorow", StringComparison.InvariantCultureIgnoreCase))
      {
        var validForModel = await _guildLineupService.GetLineupForDate(guildId, userCurrentTime, key);
        if (validForModel != null) await context.Channel.SendMessageAsync($"**Line up {validForModel.Name} ** " + validForModel.Value);
        return;
      }
      var model = await _guildLineupService.GetLineup(guildId, userCurrentTime, null, key);
      if (model != null) await context.Channel.SendMessageAsync($"**Line up {model.Name} ** " + model.Value);

    }


    public async Task RemoveLineup(SocketCommandContext context, string command)
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
        await context.User.SendMessageAsync("You are not allowed to do this, you need Manage Messages");
        return;
      }
      var userCurrentTime = context.Message.Timestamp.UtcDateTime;
      var guildId = context.Guild.Id.ToString();
      var parts = StringUtility.SmartSplit(command);
      var key = parts[1];

      var model = await _guildLineupService.DeleteLineup(guildId, key);


      var confirmationMessage = await context.Channel.SendMessageAsync($"Deleted lineup !{key}");
      await Task.Delay(TimeSpan.FromSeconds(2));
      await confirmationMessage.DeleteAsync();
    }
  }
}
