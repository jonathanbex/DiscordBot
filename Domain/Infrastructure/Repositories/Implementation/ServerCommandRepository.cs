using Domain.Infrastructure.Context;
using Domain.Infrastructure.Repositories.Interfaces;
using Domain.Mapping;
using Domain.Models.BusinessLayer;
using Microsoft.EntityFrameworkCore;

namespace Domain.Infrastructure.Repositories.Implementation
{
  public class ServerCommandRepository : IServerCommandRepository
  {
    private DiscordbotContext _discordbotContext;
    public ServerCommandRepository(DiscordbotContext discordbotContext)
    {
      _discordbotContext = discordbotContext;
    }
    public async Task<ServerCommand> AddOrUpdateServerCommand(ServerCommand command)
    {
      var entry = await _discordbotContext.ServerCommands.FirstOrDefaultAsync(x => x.Guid == command.GUID && x.GuildId == command.GuildId);
      if (entry == null)
      {
        entry = DBMapper.MapToEntityViewModel(command);
        await _discordbotContext.AddAsync(entry);
      }
      entry.Key = command.Key;
      entry.Value = command.Value;
      entry.GuildId = command.GuildId;
      entry.Updated = command.Updated;
      await _discordbotContext.SaveChangesAsync();
      return DBMapper.MapToViewModel(entry);
    }

    public async Task<bool> DeleteCommand(ServerCommand command)
    {
      var entry = await _discordbotContext.ServerCommands.FirstOrDefaultAsync(x => x.Guid == command.GUID && x.GuildId == command.GuildId);
      if (entry == null) return true;
      _discordbotContext.Remove(entry);
      await _discordbotContext.SaveChangesAsync();
      return true;
    }

    public async Task<List<ServerCommand>?> GetAllCommands(string guildId)
    {
      var commands = await _discordbotContext.ServerCommands.Where(x => x.GuildId == guildId).ToListAsync();
      var result = new List<ServerCommand>();
      foreach (var command in commands)
      {
        result.Add(DBMapper.MapToViewModel(command));
      }
      return result;
    }

    public async Task<ServerCommand?> GetCommand(string guildId, string key)
    {
      var dbCommand = await _discordbotContext.ServerCommands.Where(x => x.GuildId == guildId && x.Key == key).FirstOrDefaultAsync();
      if (dbCommand == null) return null;
      return DBMapper.MapToViewModel(dbCommand);
    }

    public async Task<ServerCommand?> GetCommandWithGuid(string guildId, string guid)
    {
      var dbCommand = await _discordbotContext.ServerCommands.Where(x => x.GuildId == guildId && x.Guid == guid).FirstOrDefaultAsync();
      if (dbCommand == null) return null;
      return DBMapper.MapToViewModel(dbCommand);
    }
  }
}
