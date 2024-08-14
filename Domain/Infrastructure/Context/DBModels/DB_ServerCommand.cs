namespace Domain.Infrastructure.Context;

public partial class DB_ServerCommand
{
    public string Guid { get; set; } = null!;

    public string? GuildId { get; set; }

    public string? Key { get; set; }

    public string? Value { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Updated { get; set; }
}
