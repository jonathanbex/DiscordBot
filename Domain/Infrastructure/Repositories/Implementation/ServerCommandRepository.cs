using Domain.Infrastructure.Repositories.Interfaces;
using Domain.Models.BusinessLayer;

namespace Domain.Infrastructure.Repositories.Implementation
{
  public class ServerCommandRepository : IServerCommandRepository
  {
    public async Task<ServerCommand> AddOrUpdateServerCommand(ServerCommand command)
    {
      throw new NotImplementedException();
    }

    public async Task<bool> DeleteCommand(ServerCommand command)
    {
      throw new NotImplementedException();
    }

    public async Task<List<ServerCommand>?> GetAllCommands(string guildId)
    {
      throw new NotImplementedException();
    }

    public async Task<ServerCommand?> GetCommand(string guildId, string key)
    {
      throw new NotImplementedException();
    }

    public async Task<ServerCommand?> GetCommandWithGuid(string guildId, string guid)
    {
      throw new NotImplementedException();
    }
  }
}
