using Domain.Models.BusinessLayer;

namespace Domain.Services.Interfaces
{
  public interface IServerCommandService
  {
    Task<ServerCommand?> GetCommandValue(string guildId, string key);
    Task<List<ServerCommand>?> ListAllCommands(string guildId);
    Task<ServerCommand> AddOrUpdateCommand(string guildId, string key, string value);
    Task<bool> DeleteCommand(string guildId, string key);
    Task<bool> DeleteCommand(ServerCommand command);

    Task<int> GetCommandCount(string guildId);
  }
}
