using Discord.Commands;
using Discord.WebSocket;
using Discord;
using Domain.Utility;

namespace BotApplication.Methods
{
  public class RoleHelper
  {
    public RoleHelper() { }
    public async Task AddRoles(SocketCommandContext context, string command)
    {
      // Example: Check if the user has the "Manage Messages" permission
      if (context.Guild == null || context.User is not SocketGuildUser guildUser)
      {
        return;
      }

      var channel = context.Channel as ITextChannel;

      // Ensure the channel is a text channel
      if (channel == null)
      {

        return;
      }

      var permissions = guildUser.GetPermissions(channel);

      if (!permissions.ManageRoles)
      {
        await context.User.SendMessageAsync("You are not allowed to do this, you need Manage Roles");
        return;
      }

      var guild = context.Guild;
      await guild.DownloadUsersAsync();
      var allMembers = guild.Users;
      var guildRoles = guild.Roles;
      var roles = StringUtility.SmartSplit(command);
      //first entry is the command addRole
      // second entry is username
      // third-n is the roles to be added
      if (roles.Count > 2)
      {
        var user = roles[1];
        if (string.IsNullOrEmpty(user)) return;
        var userToEdit = allMembers.FirstOrDefault(x => x.DisplayName == user);
        if (userToEdit == null) return;
        var rolesToAdd = new List<IRole>();
        foreach (var role in roles.Skip(2))
        {
          if (userToEdit.Roles.Any(x => x.Name == role)) continue;
          var guildRole = guild.Roles.FirstOrDefault(x => x.Name == role);
          if (guildRole == null) continue;
          rolesToAdd.Add(guildRole);
        }

        if (rolesToAdd.Count > 0) await userToEdit.AddRolesAsync(rolesToAdd);

        var addedRoleMessage = await context.Channel.SendMessageAsync($"Added roles to user {user}");

        await Task.Delay(TimeSpan.FromSeconds(2));
        await addedRoleMessage.DeleteAsync();
      }
    }
  }
}