using Microsoft.Extensions.Configuration;
using System.Text.Json;

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
          // If the file doesn't exist, create a default configuration file
          Console.WriteLine($"Configuration file '{filePath}' not found. Creating a default configuration file at '{fullPath}'.");

          var defaultConfig = new
          {
            DiscordBot = new
            {
              Token = ""
            },
            ConnectionStrings = new
            {
              DiscordBotEntities = ""
            }
          };

          var defaultJson = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
          File.WriteAllText(fullPath, defaultJson);
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
