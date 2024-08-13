using BotApplication.Methods;
using BotApplication.Queue;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Domain.Services.Interfaces;
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
    public Bot(IConfiguration config,
      TaskQueue taskQueue,
      IServerCommandService serverCommandService,
      ClearingHelper clearingHelper,
      RoleHelper roleHelper,
      CommandHelper commandHelper)
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

    }

    public async Task RunAsync()
    {
      var token = _config.GetValue<string>("DiscordBot:Token");
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

    private async Task MessageReceivedAsync(SocketMessage arg)
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
      else if (command.StartsWith("jHelp", StringComparison.InvariantCultureIgnoreCase))
      {
        var registeredCommands = await _serverCommandService.ListAllCommands(context.Guild.Id.ToString());
        var stringBuilder = new StringBuilder("**Available Commands:**\n" +
            "`!hello` - Greets the user.\n" +
            "`!clear <x>` - Clears `x` number of messages from the channel.\n" +
            "`!addRole <role>` - Adds the specified role to a user. For example: `!addRole Fyllhund Guildmaster Member`.\n" +
            "`!guildInfo` - Returns information about the guild.");
        foreach (var registeredCommand in registeredCommands)
        {
          stringBuilder.AppendLine(registeredCommand.Key);
        }
        await context.Channel.SendMessageAsync(stringBuilder.ToString());
      }
      else if (command.StartsWith("addCommand", StringComparison.InvariantCultureIgnoreCase))
      {
        await _commandHelper.AddOrUpdateCommand(context, command);

      }

    });

    }
  }
}
