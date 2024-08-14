using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace BotApplication.Helper
{
  public static class TryGetConfigurationHelper
  {
    public static IConfiguration LoadConfiguration(string filePath = "appsettings.json")
    {
      // Get the path where the executable is located
      string exePath = AppContext.BaseDirectory;
      string fullPath = Path.Combine(exePath, filePath);

      // Check if the file exists in the executable directory
      if (!File.Exists(fullPath))
      {
        // If not found, check the current working directory
        fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
        if (!File.Exists(fullPath))
        {
          throw new FileNotFoundException($"The configuration file '{filePath}' was not found in either the executable directory or the current working directory.");
        }
      }


      try
      {
        // Attempt to read and parse the file as JSON
        var json = File.ReadAllText(fullPath);
        var parsedJson = System.Text.Json.JsonDocument.Parse(json);
      }
      catch (JsonException ex)
      {
        throw new JsonException($"Configuration Error: The configuration file '{fullPath}' is not valid JSON. Detailed Error: {ex.Message}");
      }
      catch (Exception ex)
      {
        throw new Exception($"Unexpected error when loading the configuration file '{fullPath}'. Detailed Error: {ex.Message}");
      }

      var configuration = new ConfigurationBuilder()
          .AddJsonFile(fullPath, optional: false, reloadOnChange: true)
          .Build();

      return configuration;
    }
  }
}
