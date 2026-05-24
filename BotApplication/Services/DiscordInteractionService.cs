using Discord;
using Discord.WebSocket;
using Domain.Models.BusinessLayer;
using Domain.Services.Implementations;
using Domain.Services.Interfaces;
using System.Globalization;

namespace BotApplication.Services
{
  public class DiscordInteractionService
  {
    private const string EventSignupPrefix = "eventsignup";
    private const string RoleSelfServiceCustomId = "roleself";
    private const string DefaultEventSignupRole = "Default";

    private readonly DiscordSocketClient _client;
    private readonly IServerCommandService _serverCommandService;
    private readonly IGuildLineupService _guildLineupService;

    public DiscordInteractionService(
      DiscordSocketClient client,
      IServerCommandService serverCommandService,
      IGuildLineupService guildLineupService)
    {
      _client = client;
      _serverCommandService = serverCommandService;
      _guildLineupService = guildLineupService;
    }

    public async Task RegisterSlashCommands()
    {
      foreach (var guild in _client.Guilds)
      {
        try
        {
          await guild.BulkOverwriteApplicationCommandAsync(BuildSlashCommands());
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Failed to register slash commands for {guild.Name}: {ex.Message}");
        }
      }
    }

    public async Task HandleSlashCommand(SocketSlashCommand command)
    {
      try
      {
        switch (command.Data.Name)
        {
          case "hello":
            await command.RespondAsync(BotResponseTextService.Hello());
            break;
          case "jhelp":
            await command.RespondAsync(await BotResponseTextService.Help(_serverCommandService, command.GuildId!.Value.ToString()), ephemeral: true);
            break;
          case "event-create":
            await CreateEventFromSlashCommand(command);
            break;
          case "event-render":
            await RenderEventFromSlashCommand(command);
            break;
          case "event-signup-roles":
            await AddEventSignupRoles(command);
            break;
          case "role-self-service":
            await CreateRoleSelfServiceMessage(command);
            break;
          default:
            await command.RespondAsync(BotResponseTextService.UnknownCommand(command.Data.Name), ephemeral: true);
            break;
        }
      }
      catch (Exception ex)
      {
        await RespondSafely(command, $"Something went sideways: {ex.Message}", true);
      }
    }

    public async Task HandleButton(SocketMessageComponent component)
    {
      if (!component.Data.CustomId.StartsWith($"{EventSignupPrefix}:", StringComparison.Ordinal)) return;

      var parts = component.Data.CustomId.Split(':', 3);
      if (parts.Length != 3)
      {
        await component.RespondAsync("That signup button is missing its event details.", ephemeral: true);
        return;
      }

      var role = parts[1];
      var eventName = Uri.UnescapeDataString(parts[2]);
      if (role.Equals("Remove", StringComparison.InvariantCultureIgnoreCase))
      {
        var user = component.User as SocketGuildUser;
        if (user == null || component.GuildId == null)
        {
          await component.RespondAsync("I can only update signups for server members.", ephemeral: true);
          return;
        }

        await _guildLineupService.RemoveMemberFromEvent(component.GuildId.Value.ToString(), eventName, user.DisplayName);
        await component.RespondAsync($"Removed `{user.DisplayName}` from `{eventName}`.", ephemeral: true);
        return;
      }

      var guildUser = component.User as SocketGuildUser;
      if (guildUser == null || component.GuildId == null)
      {
        await component.RespondAsync("I can only sign up server members.", ephemeral: true);
        return;
      }

      var lineup = await _guildLineupService.AddMemberToEvent(component.GuildId.Value.ToString(), eventName, guildUser.DisplayName, role);
      await component.RespondAsync(BotResponseTextService.EventMemberAdded(guildUser.DisplayName, lineup.Name ?? eventName, role), ephemeral: true);
    }

    public async Task HandleSelectMenu(SocketMessageComponent component)
    {
      if (!component.Data.CustomId.Equals(RoleSelfServiceCustomId, StringComparison.Ordinal)) return;

      var guildUser = component.User as SocketGuildUser;
      if (guildUser == null)
      {
        await component.RespondAsync("I can only update roles for server members.", ephemeral: true);
        return;
      }

      var selectedRoleIds = component.Data.Values
        .Select(value => ulong.TryParse(value, out var roleId) ? roleId : 0)
        .Where(roleId => roleId != 0)
        .ToHashSet();

      var selectableRoleIds = component.Message.Components
        .SelectMany(row => row.Components)
        .OfType<SelectMenuComponent>()
        .Where(menu => menu.CustomId == RoleSelfServiceCustomId)
        .SelectMany(menu => menu.Options)
        .Select(option => ulong.TryParse(option.Value, out var roleId) ? roleId : 0)
        .Where(roleId => roleId != 0)
        .ToHashSet();

      var rolesToAdd = selectableRoleIds
        .Where(selectedRoleIds.Contains)
        .Select(roleId => guildUser.Guild.GetRole(roleId))
        .Where(role => role != null && !guildUser.Roles.Any(userRole => userRole.Id == role.Id))
        .Cast<IRole>()
        .ToList();

      var rolesToRemove = selectableRoleIds
        .Where(roleId => !selectedRoleIds.Contains(roleId))
        .Select(roleId => guildUser.Guild.GetRole(roleId))
        .Where(role => role != null && guildUser.Roles.Any(userRole => userRole.Id == role.Id))
        .Cast<IRole>()
        .ToList();

      if (rolesToAdd.Count > 0) await guildUser.AddRolesAsync(rolesToAdd);
      if (rolesToRemove.Count > 0) await guildUser.RemoveRolesAsync(rolesToRemove);

      await component.RespondAsync("Your roles are updated.", ephemeral: true);
    }

    private static ApplicationCommandProperties[] BuildSlashCommands()
    {
      return
      [
        new SlashCommandBuilder()
          .WithName("hello")
          .WithDescription("Say hello to Jbot.")
          .Build(),
        new SlashCommandBuilder()
          .WithName("jhelp")
          .WithDescription("Show the command board.")
          .Build(),
        new SlashCommandBuilder()
          .WithName("event-create")
          .WithDescription("Create an event and post signup buttons.")
          .AddOption("name", ApplicationCommandOptionType.String, "Event name.", isRequired: true)
          .AddOption("time", ApplicationCommandOptionType.String, "Event time as yyyyMMdd HH:mm.", isRequired: true)
          .AddOption("signup_roles", ApplicationCommandOptionType.String, "Optional comma-separated signup roles to add.", isRequired: false)
          .Build(),
        new SlashCommandBuilder()
          .WithName("event-render")
          .WithDescription("Render an event lineup.")
          .AddOption("name", ApplicationCommandOptionType.String, "Event name.", isRequired: true)
          .Build(),
        new SlashCommandBuilder()
          .WithName("event-signup-roles")
          .WithDescription("Add signup roles to an existing event.")
          .AddOption("name", ApplicationCommandOptionType.String, "Event name.", isRequired: true)
          .AddOption("roles", ApplicationCommandOptionType.String, "Comma-separated signup roles to add.", isRequired: true)
          .Build(),
        new SlashCommandBuilder()
          .WithName("role-self-service")
          .WithDescription("Post a self-service role selector.")
          .AddOption("roles", ApplicationCommandOptionType.String, "Comma-separated role names users can choose.", isRequired: true)
          .Build()
      ];
    }

    private async Task CreateEventFromSlashCommand(SocketSlashCommand command)
    {
      if (!CanManageMessages(command.User as SocketGuildUser, command.Channel as ITextChannel))
      {
        await command.RespondAsync(BotResponseTextService.ManageMessagesDenied, ephemeral: true);
        return;
      }

      var name = GetSlashOption(command, "name");
      var time = GetSlashOption(command, "time");
      if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(time))
      {
        await command.RespondAsync("Give me both an event name and a time.", ephemeral: true);
        return;
      }

      if (!DateTime.TryParseExact(time, "yyyyMMdd HH:mm", null, DateTimeStyles.None, out var parsedDate))
      {
        await command.RespondAsync("Use time format `yyyyMMdd HH:mm`, for example `20260601 20:00`.", ephemeral: true);
        return;
      }

      var eventAt = new DateTimeOffset(parsedDate, command.CreatedAt.Offset).ToUniversalTime();
      var signupRoles = SplitRoles(GetSlashOption(command, "signup_roles"));
      var lineup = await _guildLineupService.AddOrUpdateEvent(command.GuildId!.Value.ToString(), name, eventAt, command.Channel.Id, command.CreatedAt, signupRoles);
      await command.RespondAsync(BotResponseTextService.EventCreated(name, eventAt), components: BuildEventSignupComponents(lineup));
    }

    private async Task RenderEventFromSlashCommand(SocketSlashCommand command)
    {
      var name = GetSlashOption(command, "name");
      if (string.IsNullOrWhiteSpace(name))
      {
        await command.RespondAsync("Give me an event name to render.", ephemeral: true);
        return;
      }

      var lineup = await _guildLineupService.GetLineup(command.GuildId!.Value.ToString(), command.CreatedAt, null, name);
      if (lineup == null)
      {
        await command.RespondAsync(BotResponseTextService.EventNotFound(name), ephemeral: true);
        return;
      }

      await command.RespondAsync(_guildLineupService.RenderLineup(lineup), components: BuildEventSignupComponents(lineup));
    }

    private async Task AddEventSignupRoles(SocketSlashCommand command)
    {
      if (!CanManageMessages(command.User as SocketGuildUser, command.Channel as ITextChannel))
      {
        await command.RespondAsync(BotResponseTextService.ManageMessagesDenied, ephemeral: true);
        return;
      }

      var name = GetSlashOption(command, "name");
      var roles = SplitRoles(GetSlashOption(command, "roles"));
      if (string.IsNullOrWhiteSpace(name) || roles.Count == 0)
      {
        await command.RespondAsync("Give me an event name and at least one signup role.", ephemeral: true);
        return;
      }

      var lineup = await _guildLineupService.AddSignupRolesToEvent(command.GuildId!.Value.ToString(), name, roles);
      await command.RespondAsync($"Signup roles updated for `{lineup.Name ?? name}`.", components: BuildEventSignupComponents(lineup), ephemeral: true);
    }

    private async Task CreateRoleSelfServiceMessage(SocketSlashCommand command)
    {
      if (!CanManageRoles(command.User as SocketGuildUser, command.Channel as ITextChannel))
      {
        await command.RespondAsync(BotResponseTextService.ManageRolesDenied, ephemeral: true);
        return;
      }

      var roleNames = SplitRoles(GetSlashOption(command, "roles"));
      var guildUser = command.User as SocketGuildUser;
      var guild = guildUser?.Guild;
      if (guild == null || roleNames.Count == 0)
      {
        await command.RespondAsync("Give me one or more role names separated by commas.", ephemeral: true);
        return;
      }

      var roles = roleNames
        .Select(roleName => guild.Roles.FirstOrDefault(role => role.Name.Equals(roleName, StringComparison.InvariantCultureIgnoreCase)))
        .Where(role => role != null)
        .Cast<SocketRole>()
        .Take(25)
        .ToList();

      if (roles.Count == 0)
      {
        await command.RespondAsync("I could not find any of those roles on this server.", ephemeral: true);
        return;
      }

      var menu = new SelectMenuBuilder()
        .WithCustomId(RoleSelfServiceCustomId)
        .WithPlaceholder("Choose your roles")
        .WithMinValues(0)
        .WithMaxValues(roles.Count);

      foreach (var role in roles)
      {
        menu.AddOption(role.Name, role.Id.ToString());
      }

      var components = new ComponentBuilder().WithSelectMenu(menu).Build();
      await command.RespondAsync("Choose the roles you want from the menu below.", components: components);
    }

    private MessageComponent BuildEventSignupComponents(GuildLineup lineup)
    {
      var builder = new ComponentBuilder();
      var escapedEventName = Uri.EscapeDataString(lineup.Name ?? string.Empty);
      var row = 0;
      var buttonsInRow = 0;

      AddButton(builder, "Add me", $"{EventSignupPrefix}:{DefaultEventSignupRole}:{escapedEventName}", ButtonStyle.Success, ref row, ref buttonsInRow);

      foreach (var role in _guildLineupService.GetSignupRoles(lineup).Where(role => !role.Equals(DefaultEventSignupRole, StringComparison.InvariantCultureIgnoreCase)).Take(23))
      {
        AddButton(builder, role, $"{EventSignupPrefix}:{role}:{escapedEventName}", ButtonStyle.Primary, ref row, ref buttonsInRow);
      }

      AddButton(builder, "Remove me", $"{EventSignupPrefix}:Remove:{escapedEventName}", ButtonStyle.Secondary, ref row, ref buttonsInRow);
      return builder.Build();
    }

    private static void AddButton(ComponentBuilder builder, string label, string customId, ButtonStyle style, ref int row, ref int buttonsInRow)
    {
      if (buttonsInRow >= 5)
      {
        row++;
        buttonsInRow = 0;
      }

      builder.WithButton(label, customId, style, row: row);
      buttonsInRow++;
    }

    private static List<string> SplitRoles(string? roles)
    {
      return roles?
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Distinct(StringComparer.InvariantCultureIgnoreCase)
        .ToList() ?? new List<string>();
    }

    private static string? GetSlashOption(SocketSlashCommand command, string name)
    {
      return command.Data.Options.FirstOrDefault(option => option.Name == name)?.Value?.ToString();
    }

    private static bool CanManageMessages(SocketGuildUser? user, ITextChannel? channel)
    {
      return user != null && channel != null && user.GetPermissions(channel).ManageMessages;
    }

    private static bool CanManageRoles(SocketGuildUser? user, ITextChannel? channel)
    {
      return user != null && channel != null && user.GetPermissions(channel).ManageRoles;
    }

    private static async Task RespondSafely(SocketSlashCommand command, string message, bool ephemeral)
    {
      if (command.HasResponded) await command.FollowupAsync(message, ephemeral: ephemeral);
      else await command.RespondAsync(message, ephemeral: ephemeral);
    }
  }
}
