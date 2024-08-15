namespace Domain.Infrastructure.Context;

public partial class VerifiedGuild
{
  public int Id { get; set; }

  public string? GuildId { get; set; }

  public bool? Subscriber { get; set; }

  public DateTime? LastPayment { get; set; }

  public string? TimeZone { get; set; }

  public DateTime? Created { get; set; }

}
