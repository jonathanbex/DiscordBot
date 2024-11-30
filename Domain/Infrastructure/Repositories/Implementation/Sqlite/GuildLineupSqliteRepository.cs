using Domain.Infrastructure.Context.Sqlite.Models;
using Domain.Infrastructure.Repositories.Interfaces;
using Domain.Mapping;
using Domain.Models.BusinessLayer;
using SQLite;

namespace Domain.Infrastructure.Repositories.Implementation.Sqlite
{
  public class GuildLineupSqliteRepository : IGuildLineupRepository
  {
    private SQLiteAsyncConnection _database;

    private async Task<SQLiteAsyncConnection> GetDatabaseConnectionAsync()
    {
      if (_database == null)
      {
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "guildlineups.db");
        _database = new SQLiteAsyncConnection(dbPath);
        await _database.CreateTableAsync<SqliteGuildLineup>();
      }
      return _database;
    }

    public async Task<GuildLineup> AddOrUpdateGuildLineup(GuildLineup lineup)
    {
      var db = await GetDatabaseConnectionAsync();
      var existingLineup = await db.Table<SqliteGuildLineup>()
                                   .FirstOrDefaultAsync(x => x.Guid == lineup.GUID && x.GuildId == lineup.GuildId);

      if (existingLineup != null)
      {
        // Update existing lineup
        existingLineup.ValidFor = lineup.ValidFor;
        existingLineup.Value = lineup.Value;
        existingLineup.GuildId = lineup.GuildId;
        existingLineup.Updated = lineup.Updated;
        await db.UpdateAsync(existingLineup);

        return DBMapper.MapToViewModel(existingLineup);
      }
      else
      {
        // Insert new lineup
        var dbModel = DBMapper.MapToEntityViewModel(lineup);
        await db.InsertAsync(dbModel);
        return lineup;
      }
    }

    public async Task<bool> DeleteLineup(GuildLineup lineup)
    {
      var db = await GetDatabaseConnectionAsync();
      var result = await db.Table<SqliteGuildLineup>()
                           .DeleteAsync(x => x.Guid == lineup.GUID && x.GuildId == lineup.GuildId);
      return result > 0;
    }

    public async Task<GuildLineup?> GetLineup(string guildId, DateTime validFrom, DateTime? validTo = null)
    {
      var db = await GetDatabaseConnectionAsync();
      var query = db.Table<SqliteGuildLineup>().Where(x => x.GuildId == guildId && x.ValidFor >= validFrom);

      if (validTo != null)
      {
        query = query.Where(x => x.ValidFor <= validTo);
      }

      var dbModel = await query.OrderBy(x => x.ValidFor).FirstOrDefaultAsync();
      return dbModel != null ? DBMapper.MapToViewModel(dbModel) : null;
    }

    public async Task<GuildLineup?> GetLineup(string guildId, string name)
    {
      var db = await GetDatabaseConnectionAsync();
      var dateTimeNow = DateTime.UtcNow;
      var dbModel = await db.Table<SqliteGuildLineup>()
                            .FirstOrDefaultAsync(x => x.GuildId == guildId && x.Name == name && x.ValidFor >= dateTimeNow);

      return dbModel != null ? DBMapper.MapToViewModel(dbModel) : null;
    }

    public async Task<List<GuildLineup>?> GetLineups(string guildId)
    {
      var db = await GetDatabaseConnectionAsync();
      var datetimeNow = DateTime.UtcNow;
      var startOfDay = new DateTime(datetimeNow.Year, datetimeNow.Month, datetimeNow.Day, 0, 0, 1);

      var queryResult = await db.Table<SqliteGuildLineup>()
                                .Where(x => x.GuildId == guildId && x.ValidFor >= startOfDay)
                                .ToListAsync();

      return queryResult.Select(DBMapper.MapToViewModel).ToList();
    }

  }

}
