using BotApplication.Methods;
using Discord;
using Discord.WebSocket;
using Domain.Models.BusinessLayer;
using Domain.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BotApplication.Worker
{
  public class ScheduledJobService : BackgroundService
  {
    private const string AutoPostChannelConfigKey = "LineupAutoPostChannelId";
    private const string AutoPostMinutesBeforeConfigKey = "LineupAutoPostMinutesBefore";
    private const int DefaultLineupAutoPostMinutesBefore = 60;
    private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(1);

    private readonly DiscordSocketClient _client;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly HashSet<string> _postedLineups = new HashSet<string>();

    public ScheduledJobService(
      DiscordSocketClient client,
      IServiceScopeFactory serviceScopeFactory)
    {
      _client = client;
      _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        try
        {
          await RunScheduledJobs(stoppingToken);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Scheduled job service error: {ex.Message}");
        }

        await Task.Delay(SweepInterval, stoppingToken);
      }
    }

    private async Task RunScheduledJobs(CancellationToken cancellationToken)
    {
      if (_client.ConnectionState != ConnectionState.Connected)
      {
        return;
      }

      using var scope = _serviceScopeFactory.CreateScope();
      var guildLineupService = scope.ServiceProvider.GetRequiredService<IGuildLineupService>();
      var serverCommandService = scope.ServiceProvider.GetRequiredService<IServerCommandService>();

      foreach (var guild in _client.Guilds)
      {
        cancellationToken.ThrowIfCancellationRequested();
        await TryPostScheduledLineups(guild, guildLineupService, serverCommandService);
      }
    }

    private async Task TryPostScheduledLineups(
      SocketGuild guild,
      IGuildLineupService guildLineupService,
      IServerCommandService serverCommandService)
    {
      var guildId = guild.Id.ToString();
      var channel = await GetConfiguredLineupChannel(guild, serverCommandService);
      if (channel == null)
      {
        return;
      }

      var lineups = await guildLineupService.ListAllLineups(guildId);
      if (lineups == null || !lineups.Any())
      {
        return;
      }

      var now = DateTime.UtcNow;
      var minutesBefore = await GetLineupAutoPostMinutesBefore(guildId, serverCommandService);
      var latestAutoPostTime = now.AddMinutes(minutesBefore);

      foreach (var lineup in lineups.Where(lineup => ShouldPostLineup(lineup, now, latestAutoPostTime)))
      {
        var postedKey = GetPostedLineupKey(guildId, lineup);
        if (_postedLineups.Contains(postedKey))
        {
          continue;
        }

        await channel.SendMessageAsync(GuildLineupFormatter.Format(lineup));
        _postedLineups.Add(postedKey);
      }
    }

    private static bool ShouldPostLineup(GuildLineup lineup, DateTime now, DateTime latestAutoPostTime)
    {
      return lineup.ValidFor != null
        && lineup.ValidFor >= now
        && lineup.ValidFor <= latestAutoPostTime;
    }

    private static string GetPostedLineupKey(string guildId, GuildLineup lineup)
    {
      return $"{guildId}:{lineup.GUID}:{lineup.ValidFor:O}";
    }

    private async Task<IMessageChannel?> GetConfiguredLineupChannel(SocketGuild guild, IServerCommandService serverCommandService)
    {
      var configuredChannel = await serverCommandService.GetCommandValue(guild.Id.ToString(), AutoPostChannelConfigKey);
      if (configuredChannel == null || string.IsNullOrWhiteSpace(configuredChannel.Value))
      {
        return null;
      }

      var channelIdValue = configuredChannel.Value.Trim().Trim('<', '#', '>');
      if (!ulong.TryParse(channelIdValue, out var channelId))
      {
        return null;
      }

      return guild.GetTextChannel(channelId);
    }

    private async Task<int> GetLineupAutoPostMinutesBefore(string guildId, IServerCommandService serverCommandService)
    {
      var configuredMinutes = await serverCommandService.GetCommandValue(guildId, AutoPostMinutesBeforeConfigKey);
      if (configuredMinutes == null || !int.TryParse(configuredMinutes.Value, out var minutes) || minutes < 1)
      {
        return DefaultLineupAutoPostMinutesBefore;
      }

      return minutes;
    }
  }
}
