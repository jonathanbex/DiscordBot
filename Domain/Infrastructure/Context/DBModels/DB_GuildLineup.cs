namespace Domain.Infrastructure.Context;

public partial class DB_GuildLineup
{
  public string Guid { get; set; } = null!;

  public string? GuildId { get; set; }

  public string? Name { get; set; }

  public DateTime? ValidFor { get; set; }

  public string? Value { get; set; }

  public DateTime? Created { get; set; }

  public DateTime? Updated { get; set; }
}
