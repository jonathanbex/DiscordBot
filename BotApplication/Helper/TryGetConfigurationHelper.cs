using Domain.Utility;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace BotApplication.Helper
{
  public static class TryGetConfigurationHelper
  {
    public static IConfiguration LoadConfiguration(string filePath = "appsettings.json")
    {
      string exePath = AppContext.BaseDirectory;
      string fullPath = Path.Combine(exePath, filePath);
#pragma warning disable CS8602 // Suppresses the warning for possible null referen

      string projectRootPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
#pragma warning restore CS8602 // Re-enables the warning for possible null reference
      string projectRootFullPath = Path.Combine(projectRootPath, filePath);
      // Check if the file exists in the executable directory
      if (!File.Exists(fullPath))
      {
        // If the file doesn't exist, determine the correct directory to create it in


        if (!File.Exists(projectRootFullPath))
        {
          Console.WriteLine($"Configuration file '{filePath}' not found in either the executable directory or the project root directory.");

          string creationPath = fullPath;

          if (IsDebug())
          {
            // In debug mode, create the file in the project root directory
            Console.WriteLine($"Creating the default configuration file in the project root at '{projectRootFullPath}'.");
            creationPath = projectRootFullPath;
          }
          else
          {
            // In release mode, create the file in the executable directory
            Console.WriteLine($"Creating the default configuration file in the executable directory at '{fullPath}'.");
          }

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
          File.WriteAllText(creationPath, defaultJson);
        }
      }


      try
      {


        //if (IsDebug())
        //{
        //  fullPath = projectRootFullPath;
        //}
        // Attempt to read and parse the file as JSON
        var json = File.ReadAllText(fullPath);
        var processedJson = JsonConfigurationHelper.RemoveCommentsFromJson(json);
        using var parsedJson = System.Text.Json.JsonDocument.Parse(processedJson);
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


    private static bool IsDebug()
    {
#if DEBUG
      return true;
#else
    return false;
#endif
    }
  }
}
