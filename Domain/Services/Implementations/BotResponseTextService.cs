using Domain.Services.Interfaces;
using System.Text;

namespace Domain.Services.Implementations
{
  public static class BotResponseTextService
  {
    private const string ManageMessagesDeniedText = "I can't do that for you yet. You need the `Manage Messages` permission.";
    private const string ManageRolesDeniedText = "I can't hand out roles for you yet. You need the `Manage Roles` permission.";
    private const string HelloText = "Hey! I'm Jbot. Try `!jHelp` and I'll show you the command board.";
    private const string HelpText = "**Command Board**\n" +
      "Pick a command, mind the brackets, and I'll handle the fiddly bits.\n" +
      "`!hello` - Greets the user.\n" +
      "`!clear <x>` - Clears `x` number of messages from the channel.\n" +
      "`!addRole <role>` - Adds the specified role to a user. For example: `!addRole Fyllhund Guildmaster Member. use [] for space separate roles i.e [Guild Member]`.\n" +
      "`!addEditCommand <key> [Value]` - Add or updates the existing key for this value to be used later. I.e !addEditcommand GuildInformation [Ensure you have tacos for me].\n" +
      "`!removeCommand <key>` - Removes command if found. I.e !removeCommand Tacolaco.\n" +
      "`!getLineup <name or prompt>` - Returns lineup if exists. I.e !getLineup Raid1 or !getLineup tonight (tonight/today/tommorow are valid).\n" +
      "`!addEditLineup <name> [<Time> (yyyyMMdd HH:mm)]  [<value>] ` - Add or updates the existing lineup I.e !addEditLineup Raid1 [20240815 20:00] [```Tanks: Megatank, smal tank````].\n" +
      "`!removeLineup <name>` - Removes lineup if found. I.e !removeLineup Raid1.\n" +
      "`!createEvent <name> [yyyyMMdd HH:mm]` - Creates/updates an event for lineup tracking.\n" +
      "`!addEventMember <name> [member] [role]` - Adds a guild member to the event lineup. role defaults to Member (also supports tank/healer/support/dps/custom).\n" +
      "`!renderLineup <name>` - Renders event lineup in a friendly format.\n" +
      "`!jHelp` - Returns a list of commands.\n";
    private const string CaseInsensitiveText = "**All commands are case insensitive**";
    private const string ClearInvalidQuantityText = "Give me a number from 1 to 100 and I'll do the sweep.";

    public static string ManageMessagesDenied => ManageMessagesDeniedText;
    public static string ManageRolesDenied => ManageRolesDeniedText;

    public static string Hello()
    {
      return HelloText;
    }

    public static async Task<string> Help(IServerCommandService serverCommandService, string guildId)
    {
      var registeredCommands = await serverCommandService.ListAllCommands(guildId);
      var stringBuilder = new StringBuilder(HelpText);

      if (registeredCommands != null && registeredCommands.Any())
      {
        stringBuilder.AppendLine($"**Custom Commands {registeredCommands.Count}/100**");

        foreach (var registeredCommand in registeredCommands)
        {
          stringBuilder.AppendLine($"`!{registeredCommand.Key}`");
        }

        stringBuilder.AppendLine(CaseInsensitiveText);
      }

      return stringBuilder.ToString();
    }

    public static async Task<string?> CustomCommandResponse(IServerCommandService serverCommandService, string guildId, string command)
    {
      var entry = await serverCommandService.GetCommandValue(guildId, command);
      return entry?.Value;
    }

    public static string UnknownCommand(string command)
    {
      return $"I don't know `!{command}` yet. Try `!jHelp` to see what I can do.";
    }

    public static string CommandStored(string key, string value)
    {
      return $"Stored `!{key}`. When called, I'll answer with: {value}";
    }

    public static string CommandStoreFailed(string key, string message)
    {
      return $"I could not store `!{key}`. {message}";
    }

    public static string CommandDeleted(string key)
    {
      return $"Removed `!{key}` from the command shelf.";
    }

    public static string ClearComplete(int messagesDeleted)
    {
      return $"Sweep complete. Cleared {messagesDeleted} message(s).";
    }

    public static string ClearInvalidQuantity()
    {
      return ClearInvalidQuantityText;
    }

    public static string RolesAdded(string user)
    {
      return $"All set. Added the requested roles to `{user}`.";
    }

    public static string LineupStored(string name)
    {
      return $"Lineup `{name}` is saved and ready.";
    }

    public static string LineupDeleted(string key)
    {
      return $"Lineup `{key}` has been packed away.";
    }

    public static string LineupNotFound(string key)
    {
      return $"I could not find a lineup for `{key}`.";
    }

    public static string EventCreated(string name, DateTimeOffset eventAt)
    {
      return $"Event `{name}` is on the board for {eventAt:yyyy-MM-dd HH:mm} UTC.";
    }

    public static string GuildUserNotFound(string userInput)
    {
      return $"I could not find guild user `{userInput}`. Try a mention, user id, username, nickname, or display name.";
    }

    public static string EventMemberAdded(string displayName, string lineupName, string role)
    {
      return $"Added `{displayName}` to `{lineupName}` as `{role}`. Nice, the roster grows.";
    }

    public static string EventNotFound(string eventName)
    {
      return $"No event found for `{eventName}`. Check the name or create it with `!createEvent`.";
    }
  }
}
