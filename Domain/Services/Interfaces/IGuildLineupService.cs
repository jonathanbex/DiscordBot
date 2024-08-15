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
    Task<bool> DeleteLineup(GuildLineup lineup);
  }
}
