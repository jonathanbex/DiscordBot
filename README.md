# Discord Bot

BotApplication is the main file, rest are just prepped for webhooks.

This is a simple Discord bot implemented in C# using the Discord.Net library.

## Features

- **Clear Command**: Allows users to clear a specified number of messages from a channel.
- **Add Roles Command**: Allows users to add roles to another user.
- **Guild Info**: Returns list of preformatted guild info.
- **J Help**: Returns list of commands

## Setup

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
        "DiscordBotEntitites": "Persist Security Info=True;Server=tcp:IPADDRESS,1433;Initial Catalog=DB;User     
            ID=DBUSER;Password=DBPassword;multipleactiveresultsets=True;TrustServerCertificate=True"
      }
    }
    ```

4. **Run the Bot**:

    ```bash
    dotnet run
    ```

## Commands
- `!hello`: Greets the user and provides info about jHelp
- `!clear [number]`: Deletes the specified number of messages from the channel (up to 100).
- `!addRole [string] [string]`: Add specific roles to user. I.e !addRoles Megapap Member Officer. Use [] For space separated roles I.e [Guild member]
- `!addEditCommand`: adds or updates command with text I.e !addCommand GuildInfo [We are the biggest guild] which will result in !addCommand returns We are the Biggest guild
- `!jHelp`: Returns list of commands
## License

This project is licensed under the MIT License.
