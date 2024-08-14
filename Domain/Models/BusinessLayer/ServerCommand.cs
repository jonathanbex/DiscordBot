namespace Domain.Models.BusinessLayer
{
  public class ServerCommand
  {
    public ServerCommand(bool create)
    {
      if (create) GUID = Guid.NewGuid().ToString();
    }
    public string GUID { get; set; }
    public string GuildId { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }


    public DateTime? Created { get; set; }

    public DateTime? Updated { get; set; }
  }
}
