using Domain.Infrastructure.Context;
using Domain.Infrastructure.Repositories.Implementation;
using Domain.Infrastructure.Repositories.Implementation.Sqlite;
using Domain.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Domain.DependencyInjection
{
  public class RepositoryResolver
  {
    IConfiguration _configuration;
    DiscordbotContext? _discordbotContext;
    public RepositoryResolver(IConfiguration configuration, DiscordbotContext? discordbotContext)
    {
      _configuration = configuration;
      _discordbotContext = discordbotContext;
    }

    public IServerCommandRepository ResolveServerCommandRepository()
    {
      //resolve here, since its set in runtime
      var useSqlLite = _configuration.GetValue<bool>("UseSqlLite"); 
      if (useSqlLite)
      {
        return new ServerCommandSqliteRepository(); // SQLite repository implementation
      }

      return new ServerCommandRepository(_discordbotContext); // SQL Server repository
    }

    public IGuildLineupRepository ResolveGuildLineupRepository()
    {
      //resolve here, since its set in runtime
      var useSqlLite = _configuration.GetValue<bool>("UseSqlLite");
      if (useSqlLite)
      {
        return new GuildLineupSqliteRepository(); // SQLite repository implementation
      }

      return new GuildLineupRepository(_discordbotContext); // SQL Server repository
    }
  }
}
