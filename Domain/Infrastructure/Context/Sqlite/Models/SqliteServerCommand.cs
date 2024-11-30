using SQLite;

namespace Domain.Infrastructure.Context.Sqlite.Models
{
  public class SqliteServerCommand
  {
    [PrimaryKey] 
    public string Guid { get; set; }

    public string? GuildId { get; set; }

    public string? Key { get; set; }

    public string? Value { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Updated { get; set; }
  }

}
