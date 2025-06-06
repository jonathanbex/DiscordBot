# Discord Bot

BotApplication is the main file application, Domain is shared and DiscordBot is just prepped for webhook.

This is a simple Discord bot implemented in C# using the Discord.Net library.

## Quick Add to server
1. Click Link
   https://discord.com/oauth2/authorize?client_id=1217025143067381781&permissions=8&integration_type=0&scope=bot
2. Add to the server you want to use
3. Use commands

# Normal Setup
1. **Download latest release**
2. **Start application**
3. **Setup token and optionally a database connection to store custom commands, via command line**
   
see https://discord.com/developers/docs/intro for more info


## Download Solution (Source-code)

1. **Clone the Repository**:

    ```bash
    git clone https://github.com/jonathanbex/DiscordBot.git
    ```

2. **Install Dependencies**:

    Ensure you have .NET 8 installed. Run:

    ```bash
    dotnet restore
    ```

3. **Configure the Bot**:

    Update the `appsettings.json` file with your bot's token and optionally a connectionString for entity framework if you want to save commands:

    ```json
    {
      "DiscordBot": {
        "Token": "YOUR_BOT_TOKEN"
      },
      "ConnectionStrings": {
        "DiscordBotEntitites": "Persist Security Info=True;Server=tcp:IPADDRESS,1433;Initial Catalog=DB;User ID=DBUSER;Password=DBPassword;multipleactiveresultsets=True;TrustServerCertificate=True"
      }
    }
    ```

4. **Run the Bot**:

    ```bash
    dotnet run
    ```

## Commands (All commands are case insensitive)
- `!hello`: Greets the user and provides info about jHelp
- `!clear [number]`: Deletes the specified number of messages from the channel (up to 100).
- `!addRole [string] [string]`: Add specific roles to user. I.e !addRoles Megapap Member Officer. Use [] For space separated roles I.e [Guild member]
- `!addEditCommand`: adds or updates command with text I.e !addCommand GuildInfo [We are the biggest guild] which will result in !addCommand returns We are the Biggest guild
- `!removeCommand`: Removes command if found. I.e !removeCommand Tacolaco
- `!jHelp`: Returns list of commands
## License

This project is licensed under the MIT License.
