using Domain.Infrastructure.Context;
using Domain.Models.BusinessLayer;

namespace Domain.Mapping
{
  public static class DBMapper
  {

    public static DB_ServerCommand MapToEntityViewModel(ServerCommand viewModel)
    {
      if (viewModel == null) throw new InvalidDataException("source cannot be null");
      var dbModel = new DB_ServerCommand();
      dbModel.Guid = viewModel.GUID;
      dbModel.GuildId = viewModel.GuildId;
      dbModel.Key = viewModel.Key;
      dbModel.Value = viewModel.Value;
      dbModel.Created = viewModel.Created;
      dbModel.Updated = viewModel.Updated;
      return dbModel;
    }
    public static ServerCommand MapToViewModel(DB_ServerCommand dbModel)
    {
      if (dbModel == null) throw new InvalidDataException("source cannot be null");
      var viewModel = new ServerCommand(false);
      viewModel.GUID = dbModel.Guid;
      viewModel.GuildId = dbModel.GuildId;
      viewModel.Key = dbModel.Key;
      viewModel.Value = dbModel.Value;
      viewModel.Created = dbModel.Created;
      viewModel.Updated = dbModel.Updated;
      return viewModel;
    }

    public static DB_GuildLineup MapToEntityViewModel(GuildLineup viewModel)
    {
      if (viewModel == null) throw new InvalidDataException("source cannot be null");
      var dbModel = new DB_GuildLineup();
      dbModel.Guid = viewModel.GUID;
      dbModel.GuildId = viewModel.GuildId;
      dbModel.Name = viewModel.Name;
      dbModel.ValidFor = viewModel.ValidFor;
      dbModel.Value = viewModel.Value;
      dbModel.Created = viewModel.Created;
      dbModel.Updated = viewModel.Updated;
      return dbModel;
    }
    public static GuildLineup MapToViewModel(DB_GuildLineup dbModel)
    {
      if (dbModel == null) throw new InvalidDataException("source cannot be null");
      var viewModel = new GuildLineup(false);
      viewModel.GUID = dbModel.Guid;
      viewModel.GuildId = dbModel.GuildId;
      viewModel.Name = dbModel.Name;
      viewModel.ValidFor = dbModel.ValidFor;
      viewModel.Value = dbModel.Value;
      viewModel.Created = dbModel.Created;
      viewModel.Updated = dbModel.Updated;
      return viewModel;
    }
  }
}
