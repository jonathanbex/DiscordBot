using Domain.Models.BusinessLayer;

namespace Domain.Infrastructure.Repositories.Interfaces
{
  public interface IServerCommandRepository
  {
    Task<ServerCommand> AddOrUpdateServerCommand(ServerCommand command);
    Task<bool> DeleteCommand(ServerCommand command);
    Task<ServerCommand?> GetCommandWithGuid(string guildId, string guid);
    Task<ServerCommand?> GetCommand(string guildId, string key);
    Task<List<ServerCommand>?> GetAllCommands(string guildId);
  }
}
