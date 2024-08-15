using Domain.Models.BusinessLayer;

namespace Domain.Infrastructure.Repositories.Interfaces
{

  public interface IGuildLineupRepository
  {
    Task<GuildLineup> AddOrUpdateGuildLineup(GuildLineup lineup);
    Task<bool> DeleteLineup(GuildLineup lineup);
    Task<GuildLineup?> GetLineup(string guildId, DateTime validFrom, DateTime? validTo = null);
    Task<GuildLineup?> GetLineup(string guildId, string name);
    Task<List<GuildLineup>?> GetLineups(string guildId);
  }
}
