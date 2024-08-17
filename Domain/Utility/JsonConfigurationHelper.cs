using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Domain.Utility
{

  public static class JsonConfigurationHelper
  {
    public static void UpdateAppSettings(string key, string value)
    {
      var filePath = "appsettings.json";
      IDictionary<string, object> jsonObject;

      // Check if the file exists
      if (!File.Exists(filePath))
      {
        Console.WriteLine($"Configuration file '{filePath}' does not exist. Creating a new one.");
        jsonObject = new Dictionary<string, object>();
      }
      else
      {
        try
        {
          var json = File.ReadAllText(filePath);

          // Parse the JSON file
          using var jsonDocument = JsonDocument.Parse(json);
          jsonObject = JsonDocumentToJsonObject(jsonDocument);
        }
        catch (JsonException)
        {
          Console.WriteLine($"Invalid JSON format in '{filePath}'. Overwriting with a new configuration.");
          jsonObject = new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
          Console.WriteLine($"An error occurred while reading the configuration file: {ex.Message}");
          throw;
        }
      }

      // Update the JSON object with the new value
      UpdateJsonValue(jsonObject, key.Split(':'), value);

      try
      {
        // Serialize the updated JSON object back to the file
        var updatedJson = JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, updatedJson);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred while writing to the configuration file: {ex.Message}");
        throw;
      }
    }

    private static IDictionary<string, object> JsonDocumentToJsonObject(JsonDocument document)
    {
      var jsonObject = new Dictionary<string, object>();

      foreach (var prop in document.RootElement.EnumerateObject())
      {
        if (prop.Value.ValueKind == JsonValueKind.Object)
        {
          jsonObject[prop.Name] = JsonDocumentToJsonObject(JsonDocument.Parse(prop.Value.GetRawText()));
        }
        else if (prop.Value.ValueKind == JsonValueKind.String)
        {
          jsonObject[prop.Name] = prop.Value.GetString();
        }
        else
        {
          jsonObject[prop.Name] = prop.Value.ToString();
        }
      }

      return jsonObject;
    }

    private static void UpdateJsonValue(IDictionary<string, object> jsonObject, string[] keys, string value)
    {
      var current = jsonObject;

      for (int i = 0; i < keys.Length - 1; i++)
      {
        if (!current.ContainsKey(keys[i]))
        {
          current[keys[i]] = new Dictionary<string, object>();
        }
        current = current[keys[i]] as IDictionary<string, object>;
      }

      current[keys[^1]] = value;
    }
  }
}
