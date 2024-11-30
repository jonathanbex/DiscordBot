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
      var useSqlLite = _configuration.GetValue<bool>("UseSqlLite");

      if (string.IsNullOrEmpty(connectionString) && !useSqlLite)
      {
        Console.WriteLine("Database configuration is required to run advanced commands like !addEditCommand.");
        Console.WriteLine("Please choose one of the following options:");
        Console.WriteLine("1. Enable SQLite (an embedded database). Choose this if you don't want to host your own database but want to save commands.");
        Console.WriteLine("2. Enter a SQL Server connection string.");
        Console.WriteLine("3. Skip database configuration (some features will be disabled).");
        Console.Write("Enter your choice (1, 2, or 3): ");

        var choice = Console.ReadLine();

        switch (choice)
        {
          case "1":
            Console.WriteLine("Enabling SQLite database. The database file will be created if it does not already exist.");
            JsonConfigurationHelper.UpdateAppSettings("UseSqlLite", true.ToString());
            ReloadConfiguration(); // Reload configuration after updating
            break;

          case "2":
            Console.WriteLine("Please enter your SQL Server connection string. Here is an example:");
            Console.WriteLine("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;MultipleActiveResultSets=True;TrustServerCertificate=True");

            Console.WriteLine("\nEnter your connection string:");
            connectionString = Console.ReadLine();

            if (string.IsNullOrEmpty(connectionString))
            {
              Console.WriteLine("Connection String is required to run the bot.");
              throw new ArgumentNullException(nameof(connectionString));
            }

            JsonConfigurationHelper.UpdateAppSettings("ConnectionStrings:DiscordBotEntities", connectionString);
            ReloadConfiguration(); // Reload configuration after updating
            break;

          case "3":
            Console.WriteLine("Skipping database configuration. Some features will be disabled.");
            break;

          default:
            Console.WriteLine("Invalid choice. Exiting setup.");
            throw new ArgumentException("Invalid choice for database configuration.");
        }
      }
      else if (useSqlLite)
      {
        Console.WriteLine("SQLite database is enabled.");
      }
      else
      {
        Console.WriteLine("Database connection string is already configured.");
      }
    }





    private void ReloadConfiguration()
    {
      _configuration = TryGetConfigurationHelper.LoadConfiguration("appsettings.json");
    }
  }

}
