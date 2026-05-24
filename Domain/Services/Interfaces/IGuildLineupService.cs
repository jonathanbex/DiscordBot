using Domain.Models.BusinessLayer;

namespace Domain.Services.Interfaces
{
  public interface IGuildLineupService
  {
    Task<GuildLineup?> GetLineup(string guildId, DateTimeOffset userCurrentTime, string? validFor = null, string? name = null);
    Task<GuildLineup?> GetLineupForDate(string guildId, DateTimeOffset userCurrentTime, string timeFrame);
    Task<List<GuildLineup>?> ListAllLineups(string guildId);
    Task<GuildLineup> AddOrUpdateLineup(string guildId, string lineupData, string name, DateTimeOffset userCurrentTime, string? validFor = null);
    Task<bool> DeleteLineup(string guildId, string name);
    Task<GuildLineup> AddOrUpdateEvent(string guildId, string eventName, DateTimeOffset eventAt, ulong channelId, DateTimeOffset userCurrentTime, IEnumerable<string>? signupRoles = null);
    Task<GuildLineup> AddMemberToEvent(string guildId, string eventName, string member, string role);
    Task<GuildLineup> RemoveMemberFromEvent(string guildId, string eventName, string member);
    Task<GuildLineup> AddSignupRolesToEvent(string guildId, string eventName, IEnumerable<string> signupRoles);
    IReadOnlyList<string> GetSignupRoles(GuildLineup lineup);
    string RenderLineup(GuildLineup lineup);
    IEnumerable<GuildLineup> GetDueLineups(IEnumerable<GuildLineup> lineups, DateTimeOffset nowUtc, int postBeforeMinutes);
    ulong? TryGetChannelId(GuildLineup lineup);
    Task<bool> DeleteLineup(GuildLineup lineup);
  }
}
