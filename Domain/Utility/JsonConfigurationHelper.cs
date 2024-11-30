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

          var processedJson = RemoveCommentsFromJson(json);
          using var jsonDocument = System.Text.Json.JsonDocument.Parse(processedJson);

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

#pragma warning disable CS8601 // Possible null reference assignment.
          jsonObject[prop.Name] = prop.Value.GetString();
#pragma warning restore CS8601 // Possible null reference assignment.
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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      var current = jsonObject;
      if (current == null) return;

      for (int i = 0; i < keys.Length - 1; i++)
      {

        if (!current.ContainsKey(keys[i]))
        {
          current[keys[i]] = new Dictionary<string, object>();
        }

        current = current[keys[i]] as IDictionary<string, object>;
      }

      current[keys[^1]] = value;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
    public static string RemoveCommentsFromJson(string json)
    {
      var lines = json.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
      var filteredLines = lines
          .Where(line => !line.TrimStart().StartsWith("//") && !string.IsNullOrWhiteSpace(line)) // Remove lines with comments or empty lines
          .ToList();
      return string.Join(Environment.NewLine, filteredLines);
    }
  }
}
