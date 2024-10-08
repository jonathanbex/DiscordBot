﻿using BotApplication.Methods;
using BotApplication.Queue;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Domain.Services.Interfaces;
using Domain.Utility;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace BotApplication.Worker
{
  public class Bot
  {
    private readonly DiscordSocketClient _client;
    private const string Prefix = "!";
    private readonly IConfiguration _config;
    private TaskQueue _taskQueue;
    private IServerCommandService _serverCommandService;
    private ClearingHelper _clearingHelper;
    private RoleHelper _roleHelper;
    private CommandHelper _commandHelper;
    private GuildLineupHelper _guildLineupHelper;
    public Bot(IConfiguration config,
      TaskQueue taskQueue,
      IServerCommandService serverCommandService,
      ClearingHelper clearingHelper,
      RoleHelper roleHelper,
      CommandHelper commandHelper,
      GuildLineupHelper guildLineupHelper)
    {
      _client = new DiscordSocketClient(new DiscordSocketConfig
      {
        GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.MessageContent | GatewayIntents.AllUnprivileged
      });
      _config = config;
      _taskQueue = taskQueue;
      _serverCommandService = serverCommandService;
      _clearingHelper = clearingHelper;
      _roleHelper = roleHelper;
      _commandHelper = commandHelper;
      _client.Log += Log;
      _client.MessageReceived += MessageReceivedAsync;
      _guildLineupHelper = guildLineupHelper;
    }

    public async Task RunAsync()
    {
      var token = _config.GetValue<string>("DiscordBot:Token");
      if (token == null)
      {
        Console.WriteLine("Missing Token for discord, make sure you have registered your bot and received a token from discord");
        throw new ArgumentNullException("token");
      }
      await _client.LoginAsync(TokenType.Bot, token);
      await _client.StartAsync();

      // Block the program until it is closed
      await Task.Delay(-1);
    }

    private Task Log(LogMessage log)
    {
      Console.WriteLine(log);
      return Task.CompletedTask;
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    private async Task MessageReceivedAsync(SocketMessage arg)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {

      if (arg is not SocketUserMessage message) return;
      if (message.Author.IsBot) return;
      if (!message.Content.StartsWith(Prefix, StringComparison.InvariantCultureIgnoreCase)) return;

      _taskQueue.Enqueue(async () =>
    {
      var context = new SocketCommandContext(_client, message);

      var command = message.Content.Substring(Prefix.Length).Trim();

      if (command.StartsWith("hello", StringComparison.InvariantCultureIgnoreCase))
      {
        await context.Channel.SendMessageAsync("Hello! Im Jbot. Please use !jHelp for more information about what I can do");
      }
      else if (command.StartsWith("clear", StringComparison.InvariantCultureIgnoreCase))
      {
        await _clearingHelper.Clear(context, command);
      }
      else if (command.StartsWith("addRole", StringComparison.InvariantCultureIgnoreCase))
      {
        await _roleHelper.AddRoles(context, command);
      }
      else if (command.StartsWith("addEditCommand", StringComparison.InvariantCultureIgnoreCase))
      {
        await _commandHelper.AddOrUpdateCommand(context, command);
      }
      else if (command.StartsWith("removeCommand", StringComparison.InvariantCultureIgnoreCase))
      {
        await _commandHelper.RemoveCommand(context, command);
      }
      else if (command.StartsWith("addEditLineup", StringComparison.InvariantCultureIgnoreCase))
      {
        await _guildLineupHelper.AddOrUpdateLineup(context, command);
      }
      else if (command.StartsWith("removeLineup", StringComparison.InvariantCultureIgnoreCase))
      {
        await _guildLineupHelper.RemoveLineup(context, command);
      }
      else if (command.StartsWith("getLineup", StringComparison.InvariantCultureIgnoreCase))
      {
        await _guildLineupHelper.GetLineup(context, command);
      }
      else if (command.StartsWith("jHelp", StringComparison.InvariantCultureIgnoreCase))
      {
        var registeredCommands = await _serverCommandService.ListAllCommands(context.Guild.Id.ToString());
        var stringBuilder = new StringBuilder("**Available Commands:**\n" +
            "`!hello` - Greets the user.\n" +
            "`!clear <x>` - Clears `x` number of messages from the channel.\n" +
            "`!addRole <role>` - Adds the specified role to a user. For example: `!addRole Fyllhund Guildmaster Member. use [] for space separate roles i.e [Guild Member]`.\n" +
            "`!addEditCommand <key> [Value]` - Add or updates the existing key for this value to be used later. I.e !addEditcommand GuildInformation [Ensure you have tacos for me].\n" +
            "`!removeCommand <key>` - Removes command if found. I.e !removeCommand Tacolaco.\n" +
            "`!getLineup <name or prompt>` - Returns lineup if exists. I.e !getLineup Raid1 or !getLineup tonight (tonight/today/tommorow are valid).\n" +
            "`!addEditLineup <name> [<Time> (yyyyMMdd HH:mm)]  [<value>] ` - Add or updates the existing lineup I.e !addEditLineup Raid1 [20240815 20:00] [```Tanks: Megatank, smal tank````].\n" +
            "`!removeLineup <name>` - Removes lineup if found. I.e !removeLineup Raid1.\n" +
            "`!jHelp` - Returns a list of commands.\n");
        if (registeredCommands != null && registeredCommands.Any())
        {
          stringBuilder.AppendLine($"**Custom Commands {registeredCommands.Count()}/100**");

          foreach (var registeredCommand in registeredCommands)
          {
            stringBuilder.AppendLine($"`!{registeredCommand.Key}`");
          }
          if (registeredCommands.Any())
          {
            stringBuilder.AppendLine("**All commands are case insensitive**");
          }
        }

        await context.Channel.SendMessageAsync(stringBuilder.ToString());
      }

      else
      {
        var entry = await _serverCommandService.GetCommandValue(context.Guild.Id.ToString(), command);
        if (entry != null) await context.Channel.SendMessageAsync(entry.Value);
      }

    });

    }
  }
}
