using BotApplication.Methods;
using BotApplication.Queue;
using BotApplication.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Domain.Services.Implementations;
using Domain.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BotApplication.Worker
{
  public class Bot
  {
    private readonly DiscordSocketClient _client;
    private const string Prefix = "!";
    private readonly IConfiguration _config;
    private readonly TaskQueue _taskQueue;
    private readonly IServerCommandService _serverCommandService;
    private readonly DiscordInteractionService _discordInteractionService;
    private readonly ClearingHelper _clearingHelper;
    private readonly RoleHelper _roleHelper;
    private readonly CommandHelper _commandHelper;
    private readonly GuildLineupHelper _guildLineupHelper;

    public Bot(
      DiscordSocketClient client,
      IConfiguration config,
      TaskQueue taskQueue,
      IServerCommandService serverCommandService,
      DiscordInteractionService discordInteractionService,
      ClearingHelper clearingHelper,
      RoleHelper roleHelper,
      CommandHelper commandHelper,
      GuildLineupHelper guildLineupHelper)
    {
      _client = client;
      _config = config;
      _taskQueue = taskQueue;
      _serverCommandService = serverCommandService;
      _discordInteractionService = discordInteractionService;
      _clearingHelper = clearingHelper;
      _roleHelper = roleHelper;
      _commandHelper = commandHelper;
      _guildLineupHelper = guildLineupHelper;

      _client.Log += Log;
      _client.Ready += _discordInteractionService.RegisterSlashCommands;
      _client.MessageReceived += MessageReceivedAsync;
      _client.SlashCommandExecuted += _discordInteractionService.HandleSlashCommand;
      _client.ButtonExecuted += _discordInteractionService.HandleButton;
      _client.SelectMenuExecuted += _discordInteractionService.HandleSelectMenu;
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
        await context.Channel.SendMessageAsync(BotResponseTextService.Hello());
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
      else if (command.StartsWith("createEvent", StringComparison.InvariantCultureIgnoreCase))
      {
        await _guildLineupHelper.CreateEvent(context, command);
      }
      else if (command.StartsWith("addEventMember", StringComparison.InvariantCultureIgnoreCase))
      {
        await _guildLineupHelper.AddMemberToEvent(context, command);
      }
      else if (command.StartsWith("renderLineup", StringComparison.InvariantCultureIgnoreCase))
      {
        await _guildLineupHelper.RenderEvent(context, command);
      }
      else if (command.StartsWith("jHelp", StringComparison.InvariantCultureIgnoreCase))
      {
        await context.Channel.SendMessageAsync(await BotResponseTextService.Help(_serverCommandService, context.Guild.Id.ToString()));
      }

      else
      {
        var response = await BotResponseTextService.CustomCommandResponse(_serverCommandService, context.Guild.Id.ToString(), command);
        if (response != null) await context.Channel.SendMessageAsync(response);
        else await context.Channel.SendMessageAsync(BotResponseTextService.UnknownCommand(command));
      }

    });

    }
  }
}
