using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Domain.Services.Implementations;
using Domain.Services.Interfaces;
using Domain.Utility;

namespace BotApplication.Methods
{
  public class GuildLineupHelper
  {
    private readonly IGuildLineupService _guildLineupService;
    private DateTimeOffset _lastAutoPostSweep = DateTimeOffset.MinValue;

    public GuildLineupHelper(IGuildLineupService guildLineupService)
    {
      _guildLineupService = guildLineupService;
    }


    public async Task AddOrUpdateLineup(SocketCommandContext context, string command)
    {
      if (!await ValidateManageMessagesPermission(context)) return;
      var userCurrentTime = context.Message.Timestamp.UtcDateTime;
      var guildId = context.Guild!.Id.ToString();
      var parts = StringUtility.SmartSplit(command);
      var name = parts[1];
      var validFor = parts[2];
      var value = parts[3];
      await _guildLineupService.AddOrUpdateLineup(guildId, value, name, userCurrentTime, validFor);
      var confirmationMessage = await context.Channel.SendMessageAsync(BotResponseTextService.LineupStored(name));
      await Task.Delay(TimeSpan.FromSeconds(2));
      await confirmationMessage.DeleteAsync();
    }

    public async Task CreateEvent(SocketCommandContext context, string command)
    {
      if (!await ValidateManageMessagesPermission(context)) return;
      var parts = StringUtility.SmartSplit(command);
      var name = parts[1];
      var validFor = parts[2];
      DateTime.TryParseExact(validFor, "yyyyMMdd HH:mm", null, System.Globalization.DateTimeStyles.None, out var parsedDate);
      var eventAt = new DateTimeOffset(parsedDate, context.Message.Timestamp.Offset).ToUniversalTime();
      await _guildLineupService.AddOrUpdateEvent(context.Guild!.Id.ToString(), name, eventAt, context.Channel.Id, context.Message.Timestamp);
      await context.Channel.SendMessageAsync(BotResponseTextService.EventCreated(name, eventAt));
    }

    public async Task AddMemberToEvent(SocketCommandContext context, string command)
    {
      if (context.Guild == null) return;
      var parts = StringUtility.SmartSplit(command);
      var eventName = parts[1];
      var guildUserInput = parts[2];
      var role = parts.Count > 3 ? parts[3] : "Member";

      var guildUser = ResolveGuildUser(context.Guild, guildUserInput);
      if (guildUser == null)
      {
        await context.Channel.SendMessageAsync(BotResponseTextService.GuildUserNotFound(guildUserInput));
        return;
      }

      var lineup = await _guildLineupService.AddMemberToEvent(context.Guild.Id.ToString(), eventName, guildUser.DisplayName, role);
      await context.Channel.SendMessageAsync(BotResponseTextService.EventMemberAdded(guildUser.DisplayName, lineup.Name ?? eventName, role));
    }

    private static SocketGuildUser? ResolveGuildUser(SocketGuild guild, string input)
    {
      if (MentionUtils.TryParseUser(input, out var userId)) return guild.GetUser(userId);
      if (ulong.TryParse(input, out var parsedId))
      {
        var byId = guild.GetUser(parsedId);
        if (byId != null) return byId;
      }
      return guild.Users.FirstOrDefault(x => x.Username.Equals(input, StringComparison.InvariantCultureIgnoreCase) || x.DisplayName.Equals(input, StringComparison.InvariantCultureIgnoreCase) || x.Nickname?.Equals(input, StringComparison.InvariantCultureIgnoreCase) == true);
    }

    public async Task RenderEvent(SocketCommandContext context, string command)
    {
      if (context.Guild == null) return;
      var parts = StringUtility.SmartSplit(command);
      var eventName = parts[1];
      var lineup = await _guildLineupService.GetLineup(context.Guild.Id.ToString(), context.Message.Timestamp, null, eventName);
      if (lineup == null)
      {
        await context.Channel.SendMessageAsync(BotResponseTextService.EventNotFound(eventName));
        return;
      }
      await context.Channel.SendMessageAsync(_guildLineupService.RenderLineup(lineup));
    }

    public async Task GetLineup(SocketCommandContext context, string command)
    {
      if (context.Guild == null) return;
      var userCurrentTime = context.Message.Timestamp.UtcDateTime;
      var guildId = context.Guild.Id.ToString();
      var parts = StringUtility.SmartSplit(command);
      var key = parts[1];
      if (key.Equals("Today", StringComparison.InvariantCultureIgnoreCase) || key.Equals("Tonight", StringComparison.InvariantCultureIgnoreCase) || key.Equals("Tommorow", StringComparison.InvariantCultureIgnoreCase))
      {
        var validForModel = await _guildLineupService.GetLineupForDate(guildId, userCurrentTime, key);
        if (validForModel != null) await context.Channel.SendMessageAsync(GuildLineupFormatter.Format(validForModel));
        else await context.Channel.SendMessageAsync(BotResponseTextService.LineupNotFound(key));
        return;
      }
      var model = await _guildLineupService.GetLineup(guildId, userCurrentTime, null, key);
      if (model != null) await context.Channel.SendMessageAsync(GuildLineupFormatter.Format(model));
      else await context.Channel.SendMessageAsync(BotResponseTextService.LineupNotFound(key));

    }

    public async Task RemoveLineup(SocketCommandContext context, string command)
    {
      if (!await ValidateManageMessagesPermission(context)) return;
      var guildId = context.Guild!.Id.ToString();
      var parts = StringUtility.SmartSplit(command);
      var key = parts[1];
      await _guildLineupService.DeleteLineup(guildId, key);
      var confirmationMessage = await context.Channel.SendMessageAsync(BotResponseTextService.LineupDeleted(key));
      await Task.Delay(TimeSpan.FromSeconds(2));
      await confirmationMessage.DeleteAsync();
    }

    private async Task<bool> ValidateManageMessagesPermission(SocketCommandContext context)
    {
      if (context.Guild == null || context.User is not SocketGuildUser guildUser) return false;
      var channel = context.Channel as ITextChannel;
      if (channel == null) return false;
      var permissions = guildUser.GetPermissions(channel);
      if (!permissions.ManageMessages)
      {
        await context.User.SendMessageAsync(BotResponseTextService.ManageMessagesDenied);
        return false;
      }
      return true;
    }
  }
}
