using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace BotApplication.Helper
{
  public static class TryGetConfigurationHelper
  {
    public static IConfiguration LoadConfiguration(string filePath = "appsettings.json")
    {

      // Check if the file exists before attempting to load it
      if (!File.Exists(filePath))
      {
        throw new FileNotFoundException($"The configuration file '{filePath}' was not found. Please ensure it exists and is correctly named.");
      }
      try
      {
        // Attempt to read and parse the file as JSON
        var json = File.ReadAllText(filePath);
        var parsedJson = System.Text.Json.JsonDocument.Parse(json);
      }
      catch (JsonException ex)
      {
        throw new JsonException($"Configuration Error: The configuration file '{filePath}' is not valid JSON. Detailed Error: {ex.Message}");
      }
      catch (Exception ex)
      {

        throw new JsonException($"Configuration Error: The configuration file '{filePath}' is not valid JSON. Detailed Error: {ex.Message}");
      }

      var configuration = new ConfigurationBuilder()
            .AddJsonFile(filePath, optional: false, reloadOnChange: true)
            .Build();

      return configuration;

    }
  }
}
