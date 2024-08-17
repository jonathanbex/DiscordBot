using Domain.Utility;
using Microsoft.Extensions.Configuration;

namespace BotApplication.Helper
{


  public class ConfigurationValidator
  {
    private IConfiguration _configuration;

    public ConfigurationValidator(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    public void ValidateAndUpdateConfiguration()
    {
      ValidateAndPromptForToken();
      ValidateAndPromptForConnectionString();
    }

    private void ValidateAndPromptForToken()
    {
      var token = _configuration.GetValue<string>("DiscordBot:Token");
      if (string.IsNullOrEmpty(token))
      {
        Console.WriteLine("Missing Token for Discord. Please enter your bot token, if you do not have one you need to register a bot see https://discord.com/developers/docs/intro, :");
        token = Console.ReadLine();
        if (string.IsNullOrEmpty(token))
        {
          Console.WriteLine("Token is required to run the bot.");
          throw new ArgumentNullException(nameof(token));
        }
        JsonConfigurationHelper.UpdateAppSettings("DiscordBot:Token", token);
        ReloadConfiguration();
      }
    }

    private void ValidateAndPromptForConnectionString()
    {
      var connectionString = _configuration.GetConnectionString("DiscordBotEntities");
      if (string.IsNullOrEmpty(connectionString))
      {
        Console.WriteLine("Missing Connection String for the database. This is needed to run more advanced commands like !addEditCommand.");
        Console.WriteLine("Do you want to set it now? (y/n)");
        var response = Console.ReadLine();

        if (response == null)
          throw new ArgumentNullException(nameof(response));

        if (response.Equals("y", StringComparison.InvariantCultureIgnoreCase) || response.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
        {
          Console.WriteLine("Please enter your connection string. Here are some examples for different databases:");

          Console.WriteLine("\nSQL Server:");
          Console.WriteLine("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;MultipleActiveResultSets=True;TrustServerCertificate=True");

          Console.WriteLine("\nPostgreSQL:");
          Console.WriteLine("Host=myServer;Database=myDataBase;Username=myUsername;Password=myPassword;SSL Mode=Require;Trust Server Certificate=true");

          Console.WriteLine("\nSQLite:");
          Console.WriteLine("Data Source=mydatabase.db;");

          Console.WriteLine("\nMySQL:");
          Console.WriteLine("Server=myServerAddress;Database=myDataBase;User=myUsername;Password=myPassword;SslMode=Preferred;");

          Console.WriteLine("\nPlease enter your connection string:");
          connectionString = Console.ReadLine();

          if (string.IsNullOrEmpty(connectionString))
          {
            Console.WriteLine("Connection String is required to run the bot.");
            throw new ArgumentNullException(nameof(connectionString));
          }

          JsonConfigurationHelper.UpdateAppSettings("ConnectionStrings:DiscordBotEntities", connectionString);
          ReloadConfiguration(); // Reload configuration after updating
        }
        else
        {
          throw new ArgumentNullException(nameof(connectionString), "Connection String is required to run the bot.");
        }
      }
    }


    private void ReloadConfiguration()
    {
      _configuration = TryGetConfigurationHelper.LoadConfiguration("appsettings.json");
    }
  }

}
