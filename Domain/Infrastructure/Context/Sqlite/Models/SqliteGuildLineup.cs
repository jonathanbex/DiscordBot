using SQLite;

namespace Domain.Infrastructure.Context.Sqlite.Models
{
  public class SqliteGuildLineup
  {
    [PrimaryKey] 
    public string Guid { get; set; }

    public string? GuildId { get; set; }

    public string? Name { get; set; }

    public DateTime? ValidFor { get; set; }

    public string? Value { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Updated { get; set; }
  }

}
