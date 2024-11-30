using Domain.Infrastructure.Context;
using Domain.Infrastructure.Context.Sqlite.Models;
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

    public static GuildLineup MapToViewModel(SqliteGuildLineup dbModel)
    {
      if (dbModel == null) throw new InvalidDataException("Source cannot be null");
      return new GuildLineup(false)
      {
        GUID = dbModel.Guid,
        GuildId = dbModel.GuildId,
        Name = dbModel.Name,
        ValidFor = dbModel.ValidFor,
        Value = dbModel.Value,
        Created = dbModel.Created,
        Updated = dbModel.Updated
      };
    }

    public static ServerCommand MapToViewModel(SqliteServerCommand dbModel)
    {
      if (dbModel == null) throw new InvalidDataException("Source cannot be null");
      return new ServerCommand(false)
      {
        GUID = dbModel.Guid,
        GuildId = dbModel.GuildId,
        Key = dbModel.Key,
        Value = dbModel.Value,
        Created = dbModel.Created,
        Updated = dbModel.Updated
      };
    }

    public static SqliteServerCommand MapToEntityFromViewModel(ServerCommand viewModel)
    {
      if (viewModel == null) throw new InvalidDataException("Source cannot be null");

      return new SqliteServerCommand
      {
        Guid = viewModel.GUID,
        GuildId = viewModel.GuildId,
        Key = viewModel.Key,
        Value = viewModel.Value,
        Created = viewModel.Created,
        Updated = viewModel.Updated
      };
    }
    public static SqliteGuildLineup MapToDbModel(GuildLineup viewModel)
    {
      if (viewModel == null) throw new InvalidDataException("Source cannot be null");
      return new SqliteGuildLineup
      {
        Guid = viewModel.GUID,
        GuildId = viewModel.GuildId,
        Name = viewModel.Name,
        ValidFor = viewModel.ValidFor,
        Value = viewModel.Value,
        Created = viewModel.Created,
        Updated = viewModel.Updated
      };
    }
    public static SqliteGuildLineup MapToEntityFromViewModel(GuildLineup viewModel)
    {
      if (viewModel == null) throw new InvalidDataException("Source cannot be null");

      return new SqliteGuildLineup
      {
        Guid = viewModel.GUID,
        GuildId = viewModel.GuildId,
        Name = viewModel.Name,
        ValidFor = viewModel.ValidFor,
        Value = viewModel.Value,
        Created = viewModel.Created,
        Updated = viewModel.Updated
      };
    }

 


  }
}
