using Domain.Infrastructure.Context.Sqlite.Models;
using Domain.Infrastructure.Repositories.Interfaces;
using Domain.Mapping;
using Domain.Models.BusinessLayer;
using SQLite;

namespace Domain.Infrastructure.Repositories.Implementation.Sqlite
{
  public class ServerCommandSqliteRepository : IServerCommandRepository
  {
    private SQLiteAsyncConnection _database;

    // Initialize the database connection
    private async Task<SQLiteAsyncConnection> GetDatabaseConnectionAsync()
    {
      if (_database == null)
      {
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "servercommands.db");
        _database = new SQLiteAsyncConnection(dbPath);
        await _database.CreateTableAsync<SqliteServerCommand>();
      }
      return _database;
    }

    public async Task<ServerCommand> AddOrUpdateServerCommand(ServerCommand command)
    {
      var db = await GetDatabaseConnectionAsync();
      var existingCommand = await db.Table<SqliteServerCommand>().FirstOrDefaultAsync(x => x.Guid == command.GUID && x.GuildId == command.GuildId);

      if (existingCommand != null)
      {
        existingCommand.Key = command.Key;
        existingCommand.Value = command.Value;
        existingCommand.Updated = command.Updated;
        await db.UpdateAsync(existingCommand);
        return DBMapper.MapToViewModel(existingCommand);
      }
      else
      {
        await db.InsertAsync(DBMapper.MapToEntityFromViewModel(command));
        return command;
      }
    }

    public async Task<bool> DeleteCommand(ServerCommand command)
    {
      var db = await GetDatabaseConnectionAsync();
      var result = await db.Table<SqliteServerCommand>().DeleteAsync(x => x.Guid == command.GUID && x.GuildId == command.GuildId);
      return result > 0;
    }

  public async Task<List<ServerCommand>?> GetAllCommands(string guildId)
{
    var db = await GetDatabaseConnectionAsync();
    var result = await db.Table<SqliteServerCommand>()
                         .Where(x => x.GuildId == guildId)
                         .ToListAsync();

    // Map each SqliteServerCommand to ServerCommand
    return result.Select(DBMapper.MapToViewModel).ToList();
}

public async Task<ServerCommand?> GetCommand(string guildId, string key)
{
    var db = await GetDatabaseConnectionAsync();
    var existingCommand = await db.Table<SqliteServerCommand>()
                                   .FirstOrDefaultAsync(x => x.GuildId == guildId && x.Key == key);

    // Map to ServerCommand if found
    return existingCommand != null ? DBMapper.MapToViewModel(existingCommand) : null;
}

public async Task<ServerCommand?> GetCommandWithGuid(string guildId, string guid)
{
    var db = await GetDatabaseConnectionAsync();
    var existingCommand = await db.Table<SqliteServerCommand>()
                                   .FirstOrDefaultAsync(x => x.GuildId == guildId && x.Guid == guid);

    // Map to ServerCommand if found
    return existingCommand != null ? DBMapper.MapToViewModel(existingCommand) : null;
}

    public async Task<int> GetCommandCount(string guildId)
    {
      var db = await GetDatabaseConnectionAsync();
      return await db.Table<SqliteServerCommand>().CountAsync(x => x.GuildId == guildId);
    }
  }

}
