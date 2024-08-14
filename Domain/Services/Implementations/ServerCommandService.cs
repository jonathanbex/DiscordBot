using Domain.Infrastructure.Repositories.Interfaces;
using Domain.Models.BusinessLayer;
using Domain.Services.Interfaces;

namespace Domain.Services.Implementations
{
  public class ServerCommandService : IServerCommandService
  {
    IServerCommandRepository _serverCommandRepository;
    public ServerCommandService(IServerCommandRepository serverCommandRepository)
    {
      _serverCommandRepository = serverCommandRepository;
    }

    public async Task<ServerCommand> AddOrUpdateCommand(string guildId, string key, string value)
    {
      ValidateKeys(guildId, key, value);
      var command = await _serverCommandRepository.GetCommand(guildId, key);

      if (command == null) command = new ServerCommand(true) { GuildId = guildId, Key = key, Value = value };
      command.Value = value;
      var result = await _serverCommandRepository.AddOrUpdateServerCommand(command);
      return result;
    }

    public async Task<bool> DeleteCommand(string guildId, string key)
    {
      ValidateKeys(guildId, key);
      var command = await _serverCommandRepository.GetCommand(guildId, key);
      if (command == null) return true;
      return await _serverCommandRepository.DeleteCommand(command);
    }

    public async Task<bool> DeleteCommand(ServerCommand command)
    {
      return await _serverCommandRepository.DeleteCommand(command);
    }

    public async Task<ServerCommand?> GetCommandValue(string guildId, string key)
    {
      ValidateKeys(guildId, key);
      return await _serverCommandRepository.GetCommand(guildId, key);
    }

    public async Task<List<ServerCommand>?> ListAllCommands(string guildId)
    {
      ValidateKeys(guildId);
      return await _serverCommandRepository.GetAllCommands(guildId);
    }

    private void ValidateKeys(string guildId)
    {
      if (string.IsNullOrEmpty(guildId)) throw new InvalidDataException("missing guildId");
    }

    private void ValidateKeys(string guildId, string? key)
    {
      if (string.IsNullOrEmpty(guildId)) throw new InvalidDataException("missing guildId");
      if (string.IsNullOrEmpty(key)) throw new InvalidDataException("missing key");
    }


    private void ValidateKeys(string guildId, string? key, string? value)
    {
      if (string.IsNullOrEmpty(guildId)) throw new InvalidDataException("missing guildId");
      if (string.IsNullOrEmpty(key)) throw new InvalidDataException("missing key");
      if (string.IsNullOrEmpty(value)) throw new InvalidDataException("missing value");
    }

  }
}
