using System.Globalization;
using System.Text;
using System.Text.Json;
using Domain.Infrastructure.Repositories.Interfaces;
using Domain.Models.BusinessLayer;
using Domain.Services.Interfaces;

namespace Domain.Services.Implementations
{
  public class GuildLineupService : IGuildLineupService
  {
    private const string DefaultEventRole = "Member";
    private const int MaxMembersPerLine = 3;
    private static readonly string[] EventRoleOrder = ["Tank", "Support", "Healer", "Dps"];
    private readonly IGuildLineupRepository _guildLineupRepository;
    public GuildLineupService(IGuildLineupRepository guildLineupRepository)
    {
      _guildLineupRepository = guildLineupRepository;
    }

    public async Task<GuildLineup> AddOrUpdateLineup(string guildId, string lineupData, string name, DateTimeOffset userCurrentTime, string? validFor = null)
    {
      ValidateKeys(guildId, lineupData, name, validFor);
      GuildLineup? lineup = await _guildLineupRepository.GetLineup(guildId, name);
      if (lineup == null)
      {
        ValidateValidFor(validFor);
        lineup = new GuildLineup(true) { GuildId = guildId, Name = name };
      }

      lineup.Value = lineupData;
      if (!string.IsNullOrWhiteSpace(validFor)) lineup.ValidFor = ParseValidForToUtc(validFor, userCurrentTime.Offset);
      return await _guildLineupRepository.AddOrUpdateGuildLineup(lineup);
    }

    public async Task<GuildLineup> AddOrUpdateEvent(string guildId, string eventName, DateTimeOffset eventAt, ulong channelId, DateTimeOffset userCurrentTime)
    {
      ValidateKeys(guildId, eventName);
      if (eventAt <= userCurrentTime.UtcDateTime) throw new InvalidDataException("Event date is in the past");

      GuildLineup? lineup = await _guildLineupRepository.GetLineup(guildId, eventName);
      if (lineup == null) lineup = new GuildLineup(true) { GuildId = guildId, Name = eventName };

      var model = ParseEventPayload(lineup.Value);
      model.ChannelId = channelId;
      lineup.ValidFor = eventAt.UtcDateTime;
      lineup.Value = JsonSerializer.Serialize(model);
      return await _guildLineupRepository.AddOrUpdateGuildLineup(lineup);
    }

    public async Task<GuildLineup> AddMemberToEvent(string guildId, string eventName, string member, string role)
    {
      ValidateKeys(guildId, eventName);
      if (string.IsNullOrWhiteSpace(member)) throw new InvalidDataException("Missing member");
      if (string.IsNullOrWhiteSpace(role)) role = DefaultEventRole;

      GuildLineup? lineup = await _guildLineupRepository.GetLineup(guildId, eventName);
      if (lineup == null) throw new InvalidDataException($"Event '{eventName}' was not found");

      var model = ParseEventPayload(lineup.Value);
      var existing = model.Members.FirstOrDefault(x => x.Member.Equals(member, StringComparison.OrdinalIgnoreCase));
      if (existing == null) model.Members.Add(new EventLineupMember { Member = member.Trim(), Role = role.Trim() });
      else existing.Role = role.Trim();

      lineup.Value = JsonSerializer.Serialize(model);
      lineup.Updated = DateTime.UtcNow;
      return await _guildLineupRepository.AddOrUpdateGuildLineup(lineup);
    }

    public async Task<GuildLineup?> GetLineup(string guildId, DateTimeOffset userCurrentTime, string? validFor = null, string? name = null)
    {
      if (!string.IsNullOrEmpty(validFor))
      {
        ValidateValidFor(validFor);
        return await _guildLineupRepository.GetLineup(guildId, ParseValidForToUtc(validFor, userCurrentTime.Offset));
      }

      ValidateKeys(guildId, name);
      return await _guildLineupRepository.GetLineup(guildId, name!);
    }

    public async Task<GuildLineup?> GetLineupForDate(string guildId, DateTimeOffset userCurrentTime, string timeFrame)
    {
      ValidateKeys(guildId);
      var result = GetTargetDateTimeRange(timeFrame, userCurrentTime);
      return await _guildLineupRepository.GetLineup(guildId, result.DateFrom, result.DateTo);
    }

    public async Task<bool> DeleteLineup(string guildId, string name)
    {
      ValidateKeys(guildId, name);
      var lineup = await _guildLineupRepository.GetLineup(guildId, name);
      if (lineup == null) return true;
      return await _guildLineupRepository.DeleteLineup(lineup);
    }

    public Task<bool> DeleteLineup(GuildLineup lineup) => _guildLineupRepository.DeleteLineup(lineup);
    public async Task<List<GuildLineup>?> ListAllLineups(string guildId)
    {
      ValidateKeys(guildId);
      return await _guildLineupRepository.GetLineups(guildId);
    }

    public string RenderLineup(GuildLineup lineup)
    {
      var model = ParseEventPayload(lineup.Value);
      if (model.Members.Count == 0) return $"**{lineup.Name}** ({lineup.ValidFor:yyyy-MM-dd HH:mm} UTC)\n_No members added yet._";

      var sb = new StringBuilder($"**{lineup.Name}** ({lineup.ValidFor:yyyy-MM-dd HH:mm} UTC)\n");
      var roleGroups = model.Members
        .GroupBy(member => NormalizeEventRole(member.Role), StringComparer.InvariantCultureIgnoreCase)
        .OrderBy(group => GetEventRoleOrder(group.Key))
        .ThenBy(group => group.Key, StringComparer.InvariantCultureIgnoreCase);

      foreach (var group in roleGroups)
      {
        var members = group
          .Select(member => member.Member)
          .OrderBy(member => member, StringComparer.InvariantCultureIgnoreCase)
          .ToList();

        sb.AppendLine();
        sb.AppendLine($"**{group.Key} ({members.Count})**");
        foreach (var memberLine in members.Chunk(MaxMembersPerLine))
        {
          sb.AppendLine(string.Join(" ", memberLine));
        }
      }

      return sb.ToString();
    }

    public IEnumerable<GuildLineup> GetDueLineups(IEnumerable<GuildLineup> lineups, DateTimeOffset nowUtc, int postBeforeMinutes)
    {
      var from = nowUtc.UtcDateTime.AddMinutes(-1 * postBeforeMinutes);
      var to = from.AddMinutes(1);
      return lineups.Where(x => x.ValidFor.HasValue && x.ValidFor.Value >= from && x.ValidFor.Value < to);
    }

    public ulong? TryGetChannelId(GuildLineup lineup) => ParseEventPayload(lineup.Value).ChannelId;

    private static EventLineupPayload ParseEventPayload(string? value)
    {
      if (string.IsNullOrWhiteSpace(value)) return new EventLineupPayload();
      try { return JsonSerializer.Deserialize<EventLineupPayload>(value) ?? new EventLineupPayload(); }
      catch { return new EventLineupPayload(); }
    }

    private static DateTime ParseValidForToUtc(string validFor, TimeSpan offset)
    {
      DateTime.TryParseExact(validFor, "yyyyMMdd HH:mm", null, DateTimeStyles.None, out var parsedDate);
      return new DateTimeOffset(parsedDate, offset).ToUniversalTime().DateTime;
    }

    private static string NormalizeEventRole(string? role)
    {
      if (string.IsNullOrWhiteSpace(role)) return DefaultEventRole;

      role = role.Trim();
      if (role.Equals("Tanks", StringComparison.InvariantCultureIgnoreCase)) return "Tank";
      if (role.Equals("Healers", StringComparison.InvariantCultureIgnoreCase)) return "Healer";
      if (role.Equals("Supports", StringComparison.InvariantCultureIgnoreCase)) return "Support";
      if (role.Equals("Dps", StringComparison.InvariantCultureIgnoreCase)) return "Dps";
      if (role.Equals("Damage", StringComparison.InvariantCultureIgnoreCase)) return "Dps";

      return char.ToUpperInvariant(role[0]) + role[1..];
    }

    private static int GetEventRoleOrder(string role)
    {
      var standardRoleIndex = Array.FindIndex(EventRoleOrder, item => item.Equals(role, StringComparison.InvariantCultureIgnoreCase));
      if (standardRoleIndex >= 0) return standardRoleIndex;
      if (role.Equals(DefaultEventRole, StringComparison.InvariantCultureIgnoreCase)) return int.MaxValue;
      return EventRoleOrder.Length;
    }

    private (DateTime DateFrom, DateTime DateTo) GetTargetDateTimeRange(string timeFrame, DateTimeOffset userCurrentTime)
    {
      DateTimeOffset userLocalNow = userCurrentTime;
      return timeFrame.ToLower() switch
      {
        "today" => (userLocalNow.UtcDateTime, new DateTimeOffset(userLocalNow.Year, userLocalNow.Month, userLocalNow.Day, 23, 59, 59, userLocalNow.Offset).UtcDateTime),
        "tonight" => (new DateTimeOffset(userLocalNow.Year, userLocalNow.Month, userLocalNow.Day, 18, 0, 0, userLocalNow.Offset) < userLocalNow ? userLocalNow.UtcDateTime : new DateTimeOffset(userLocalNow.Year, userLocalNow.Month, userLocalNow.Day, 18, 0, 0, userLocalNow.Offset).UtcDateTime, new DateTimeOffset(userLocalNow.Year, userLocalNow.Month, userLocalNow.Day, 23, 59, 59, userLocalNow.Offset).UtcDateTime),
        "tomorrow" => (new DateTimeOffset(userLocalNow.Year, userLocalNow.Month, userLocalNow.Day, 0, 0, 0, userLocalNow.Offset).AddDays(1).UtcDateTime, new DateTimeOffset(userLocalNow.Year, userLocalNow.Month, userLocalNow.Day, 23, 59, 59, userLocalNow.Offset).AddDays(1).UtcDateTime),
        _ => throw new ArgumentException("Invalid time frame specified.")
      };
    }

    private void ValidateKeys(string guildId) { if (string.IsNullOrEmpty(guildId)) throw new InvalidDataException("missing guildId"); }
    private void ValidateValidFor(string? validFor) { if (string.IsNullOrEmpty(validFor)) throw new InvalidDataException("missing Valid For"); }
    private void ValidateKeys(string guildId, string? name)
    {
      if (string.IsNullOrEmpty(guildId)) throw new InvalidDataException("missing guildId");
      if (string.IsNullOrEmpty(name)) throw new InvalidDataException("missing Name");
    }
    private void ValidateKeys(string guildId, string? value, string? name, string? validFor)
    {
      if (string.IsNullOrEmpty(guildId)) throw new InvalidDataException("missing guildId");
      if (string.IsNullOrEmpty(value)) throw new InvalidDataException("missing value");
      if (string.IsNullOrEmpty(name)) throw new InvalidDataException("missing Name");
      if (!string.IsNullOrEmpty(validFor) && DateTime.TryParseExact(validFor, "yyyyMMdd HH:mm", null, DateTimeStyles.None, out DateTime parsedDate) && parsedDate <= DateTime.UtcNow) throw new InvalidDataException("Date is in the past");
    }

    private sealed class EventLineupPayload
    {
      public ulong? ChannelId { get; set; }
      public List<EventLineupMember> Members { get; set; } = new();
    }

    private sealed class EventLineupMember
    {
      public string Member { get; set; } = string.Empty;
      public string Role { get; set; } = DefaultEventRole;
    }
  }
}
