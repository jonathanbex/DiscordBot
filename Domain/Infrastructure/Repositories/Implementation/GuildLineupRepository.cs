using Domain.Infrastructure.Context;
using Domain.Infrastructure.Repositories.Interfaces;
using Domain.Mapping;
using Domain.Models.BusinessLayer;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Domain.Infrastructure.Repositories.Implementation
{
  public class GuildLineupRepository : IGuildLineupRepository
  {
    private DiscordbotContext? _discordbotContext;
    public GuildLineupRepository(DiscordbotContext? discordbotContext)
    {
      _discordbotContext = discordbotContext;
    }
    public async Task<GuildLineup> AddOrUpdateGuildLineup(GuildLineup lineup)
    {
      ValidateConnection();
      var entry = await _discordbotContext.GuildLineups.FirstOrDefaultAsync(x => x.Guid == lineup.GUID && x.GuildId == lineup.GuildId);
      if (entry == null)
      {
        entry = DBMapper.MapToEntityViewModel(lineup);
        await _discordbotContext.AddAsync(entry);
      }
      entry.ValidFor = lineup.ValidFor;
      entry.Value = lineup.Value;
      entry.GuildId = lineup.GuildId;
      entry.Updated = lineup.Updated;
      await _discordbotContext.SaveChangesAsync();
      return DBMapper.MapToViewModel(entry);
    }

    public async Task<bool> DeleteLineup(GuildLineup lineup)
    {
      ValidateConnection();
      var entry = await _discordbotContext.GuildLineups.FirstOrDefaultAsync(x => x.Guid == lineup.GUID && x.GuildId == lineup.GuildId);
      if (entry == null) return true;
      _discordbotContext.GuildLineups.Remove(entry);
      await _discordbotContext.SaveChangesAsync();
      return true;
    }

    public async Task<GuildLineup?> GetLineup(string guildId, DateTime validFrom, DateTime? validTo = null)
    {
      ValidateConnection();
      var query = _discordbotContext.GuildLineups.Where(x => x.GuildId == guildId && x.ValidFor >= validFrom);
      if (validTo != null) query = query.Where(x => x.ValidFor <= validTo);

      var entry = await query.OrderBy(x=>x.ValidFor).FirstOrDefaultAsync();
      if (entry == null) return null;
      return DBMapper.MapToViewModel(entry);
    }

    public async Task<GuildLineup?> GetLineup(string guildId, string name)
    {
      ValidateConnection();
      var dateTimeNow = DateTime.UtcNow;
      var entry = await _discordbotContext.GuildLineups.FirstOrDefaultAsync(x => x.GuildId == guildId && x.Name == name && x.ValidFor >= dateTimeNow);
      if (entry == null) return null;
      return DBMapper.MapToViewModel(entry);
    }

    public async Task<List<GuildLineup>?> GetLineups(string guildId)
    {
      ValidateConnection();
      var result = new List<GuildLineup>();
      var datetimeNow = DateTime.UtcNow;
      var startOfDay = new DateTime(datetimeNow.Year, datetimeNow.Month, datetimeNow.Day, 0, 0, 1);
      var queryResult = await _discordbotContext.GuildLineups.Where(x => x.GuildId == guildId && x.ValidFor >= startOfDay).ToListAsync();
      foreach (var entry in queryResult) result.Add(DBMapper.MapToViewModel(entry));
      return result;
    }

    private bool ValidateConnection()
    {
      if (_discordbotContext == null) throw new InvalidDataException("Can not use Guild Lineup service without a database");
      return true;
    }
  }
}
