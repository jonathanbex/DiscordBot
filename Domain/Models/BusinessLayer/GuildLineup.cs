namespace Domain.Models.BusinessLayer
{
  public class GuildLineup
  {
    public GuildLineup(bool create)
    {
      if (create)
      {
        GUID = Guid.NewGuid().ToString();
        Created = DateTime.UtcNow;
        Updated = DateTime.UtcNow;
      }
    }
    public string GUID { get; set; }
    public string GuildId { get; set; }
    public string? Name { get; set; }

    public DateTime? ValidFor { get; set; }

    public string? Value { get; set; }


    public DateTime? Created { get; set; }

    public DateTime? Updated { get; set; }
  }
}
