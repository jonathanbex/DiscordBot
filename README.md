# Discord Bot

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

    Update the `appsettings.json` file with your bot's token:

    ```json
    {
      "DiscordBot": {
        "Token": "YOUR_BOT_TOKEN"
      }
    }
    ```

4. **Run the Bot**:

    ```bash
    dotnet run
    ```

## Commands

- `!clear [number]`: Deletes the specified number of messages from the channel (up to 100).
- `!addRoles [string] [string]`: Add specific roles to user. I.e !addRoles Megapap Member Officer.
- `!guildInfo`: Returns preformatted guild information.
- `!JHelp`: Returns list of commands
## License

This project is licensed under the MIT License.
