using Domain.Infrastructure.Repositories.Interfaces;
using Domain.Models.BusinessLayer;
using Domain.Services.Interfaces;
using Newtonsoft.Json.Linq;

namespace Domain.Services.Implementations
{
  public class GuildLineupService : IGuildLineupService
  {
    IGuildLineupRepository _guildLineupRepository;
    public GuildLineupService(IGuildLineupRepository guildLineupRepository)
    {
      _guildLineupRepository = guildLineupRepository;
    }

    public async Task<GuildLineup> AddOrUpdateLineup(string guildId, string lineupData, string name, DateTimeOffset userCurrentTime, string? validFor = null)
    {
      ValidateKeys(guildId, lineupData, name, validFor);
      GuildLineup? lineup = await _guildLineupRepository.GetLineup(guildId, name);
      if (lineup == null)
      {
        ValidateValidFor(validFor);
        lineup = new GuildLineup(true);
        lineup.Name = name;
        DateTime.TryParseExact(validFor, "yyyyMMdd HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate);
        lineup.ValidFor = new DateTimeOffset(parsedDate, userCurrentTime.Offset).ToUniversalTime().DateTime;
      }
      lineup.Value = lineupData;
      if (validFor != null)
      {
        DateTime.TryParseExact(validFor, "yyyyMMdd HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate);
        lineup.ValidFor = new DateTimeOffset(parsedDate, userCurrentTime.Offset).ToUniversalTime().DateTime;
      }
      return await _guildLineupRepository.AddOrUpdateGuildLineup(lineup);
    }


    public async Task<GuildLineup?> GetLineup(string guildId, DateTimeOffset userCurrentTime, string? validFor = null, string? name = null)
    {
      if (!string.IsNullOrEmpty(validFor))
      {
        ValidateValidFor(validFor);
        DateTime.TryParseExact(validFor, "yyyyMMdd HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate);
        var parsedValidFor = new DateTimeOffset(parsedDate, userCurrentTime.Offset).ToUniversalTime().DateTime;
        return await _guildLineupRepository.GetLineup(guildId, parsedValidFor);
      }
      ValidateKeys(guildId, name);
#pragma warning disable CS8604 // Possible null reference argument.
      return await _guildLineupRepository.GetLineup(guildId, name);
#pragma warning restore CS8604 // Possible null reference argument.
    }

    public async Task<GuildLineup?> GetLineupForDate(string guildId, DateTimeOffset userCurrentTime, string timeFrame)
    {
      ValidateKeys(guildId);
      var result = GetTargetDateTimeRange(timeFrame,userCurrentTime);
      return await _guildLineupRepository.GetLineup(guildId, result.DateFrom, result.DateTo);
    }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task<bool> DeleteLineup(string guildId, string GUID)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
      throw new NotImplementedException();
    }

    public async Task<bool> DeleteLineup(GuildLineup lineup)
    {
      return await _guildLineupRepository.DeleteLineup(lineup);
    }

    public async Task<List<GuildLineup>?> ListAllLineups(string guildId)
    {
      ValidateKeys(guildId);
      return await _guildLineupRepository.GetLineups(guildId);
    }
    private (DateTime DateFrom, DateTime DateTo) GetTargetDateTimeRange(string timeFrame, DateTimeOffset userCurrentTime)
    {
      // Use userCurrentTime directly as it's already in user's local time
      DateTimeOffset userLocalNow = userCurrentTime;

      DateTime dateFrom;
      DateTime dateTo;

      switch (timeFrame.ToLower())
      {
        case "today":
          // Start of today (00:00:00 local time) to end of today (23:59:59 local time)
          DateTimeOffset startOfTodayLocal = new DateTimeOffset(userLocalNow.Year, userLocalNow.Month, userLocalNow.Day, 0, 0, 0, userLocalNow.Offset);
          DateTimeOffset endOfTodayLocal = new DateTimeOffset(userLocalNow.Year, userLocalNow.Month, userLocalNow.Day, 23, 59, 59, userLocalNow.Offset);

          // Ensure dateFrom is not earlier than userLocalNow
          if (startOfTodayLocal < userLocalNow)
          {
            dateFrom = userLocalNow.UtcDateTime;
          }
          else
          {
            dateFrom = startOfTodayLocal.UtcDateTime;
          }
          dateTo = endOfTodayLocal.UtcDateTime;
          break;

        case "tonight":
          // Define tonight as 18:00:00 local time to 23:59:59 local time
          DateTimeOffset startOfTonightLocal = new DateTimeOffset(userLocalNow.Year, userLocalNow.Month, userLocalNow.Day, 18, 0, 0, userLocalNow.Offset);
          DateTimeOffset endOfTonightLocal = new DateTimeOffset(userLocalNow.Year, userLocalNow.Month, userLocalNow.Day, 23, 59, 59, userLocalNow.Offset);

          // Ensure dateFrom is not earlier than userLocalNow
          if (startOfTonightLocal < userLocalNow)
          {
            dateFrom = userLocalNow.UtcDateTime;
          }
          else
          {
            dateFrom = startOfTonightLocal.UtcDateTime;
          }
          dateTo = endOfTonightLocal.UtcDateTime;
          break;

        case "tomorrow":
          // Start of tomorrow (00:00:00 local time) to end of tomorrow (23:59:59 local time)
          DateTimeOffset startOfTomorrowLocal = new DateTimeOffset(userLocalNow.Year, userLocalNow.Month, userLocalNow.Day, 0, 0, 0, userLocalNow.Offset).AddDays(1);
          DateTimeOffset endOfTomorrowLocal = new DateTimeOffset(userLocalNow.Year, userLocalNow.Month, userLocalNow.Day, 23, 59, 59, userLocalNow.Offset).AddDays(1);

          dateFrom = startOfTomorrowLocal.UtcDateTime;
          dateTo = endOfTomorrowLocal.UtcDateTime;
          break;

        default:
          throw new ArgumentException("Invalid time frame specified.");
      }

      return (dateFrom, dateTo);
    }

    private void ValidateKeys(string guildId)
    {
      if (string.IsNullOrEmpty(guildId)) throw new InvalidDataException("missing guildId");
    }


    private void ValidateValidFor(string? validFor)
    {
      if (string.IsNullOrEmpty(validFor)) throw new InvalidDataException("missing Valid For");
    }

    private void ValidateKeys(string guildId, string? name)
    {
      if (string.IsNullOrEmpty(guildId)) throw new InvalidDataException("missing guildId");
      if (string.IsNullOrEmpty(name)) throw new InvalidDataException("missing Name");

    }
    private void ValidateKeys(string guildId, string? value, string? name, string? validFor)
    {
      if (string.IsNullOrEmpty(guildId)) throw new InvalidDataException("missing guildId");
      if (string.IsNullOrEmpty(value)) throw new InvalidDataException("missing value");
      if (string.IsNullOrEmpty(name)) throw new InvalidDataException("missing Name");
      if (!string.IsNullOrEmpty(validFor))
      {
        var datetimeNow = DateTime.UtcNow;
        // Try to parse the validFor string into a DateTime object
        if (DateTime.TryParseExact(validFor, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
        {
          // Check if the parsed date is in the past
          if (parsedDate.Date < datetimeNow.Date)
          {
            throw new InvalidDataException("Date is in the past");
          }
        }
      }
    }


  }
}
