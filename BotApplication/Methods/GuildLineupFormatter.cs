using Domain.Models.BusinessLayer;
using System.Text;
using System.Text.RegularExpressions;

namespace BotApplication.Methods
{
  public static class GuildLineupFormatter
  {
    private const string DefaultRole = "Member";
    private const int MaxMembersPerLine = 3;
    private static readonly string[] RoleOrder = ["Tank", "Support", "Healer", "Dps"];

    public static string Format(GuildLineup lineup)
    {
      var result = new StringBuilder();
      result.AppendLine($"**Line up {lineup.Name}**");

      var roleGroups = Parse(lineup.Value)
        .GroupBy(member => member.Role, StringComparer.InvariantCultureIgnoreCase)
        .OrderBy(group => GetRoleOrder(group.Key))
        .ThenBy(group => group.Key, StringComparer.InvariantCultureIgnoreCase)
        .ToList();

      if (!roleGroups.Any())
      {
        result.Append(lineup.Value);
        return result.ToString();
      }

      foreach (var group in roleGroups)
      {
        var members = group
          .Select(member => member.Name)
          .OrderBy(member => member, StringComparer.InvariantCultureIgnoreCase)
          .ToList();

        result.AppendLine();
        result.AppendLine($"**{group.Key} ({members.Count})**");
        foreach (var memberLine in members.Chunk(MaxMembersPerLine))
        {
          result.AppendLine(string.Join(" ", memberLine));
        }
      }

      return result.ToString();
    }

    private static IEnumerable<LineupMember> Parse(string? value)
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        return [];
      }

      value = value.Replace("```", string.Empty).Replace("`", string.Empty);

      var members = new List<LineupMember>();
      var lines = value.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
      foreach (var line in lines)
      {
        var roleSplit = line.Split(':', 2, StringSplitOptions.TrimEntries);
        var role = roleSplit.Length == 2 ? NormalizeRole(roleSplit[0]) : DefaultRole;
        var memberValue = roleSplit.Length == 2 ? roleSplit[1] : line;

        members.AddRange(SplitMembers(memberValue)
          .Select(memberName => new LineupMember(role, memberName)));
      }

      if (!members.Any() && !string.IsNullOrWhiteSpace(value))
      {
        members.AddRange(SplitMembers(value)
          .Select(memberName => new LineupMember(DefaultRole, memberName)));
      }

      return members;
    }

    private static IEnumerable<string> SplitMembers(string value)
    {
      var splitPattern = value.Contains(',')
        ? @"\s*,\s*"
        : @"\s+";

      return Regex.Split(value, splitPattern)
        .Select(member => member.Trim())
        .Where(member => !string.IsNullOrWhiteSpace(member));
    }

    private static string NormalizeRole(string role)
    {
      role = role.Trim();
      if (role.Equals("Tanks", StringComparison.InvariantCultureIgnoreCase)) return "Tank";
      if (role.Equals("Healers", StringComparison.InvariantCultureIgnoreCase)) return "Healer";
      if (role.Equals("Supports", StringComparison.InvariantCultureIgnoreCase)) return "Support";
      if (role.Equals("Dps", StringComparison.InvariantCultureIgnoreCase)) return "Dps";
      if (role.Equals("Damage", StringComparison.InvariantCultureIgnoreCase)) return "Dps";
      if (string.IsNullOrWhiteSpace(role)) return DefaultRole;
      return char.ToUpperInvariant(role[0]) + role[1..];
    }

    private static int GetRoleOrder(string role)
    {
      var standardRoleIndex = Array.FindIndex(RoleOrder, item => item.Equals(role, StringComparison.InvariantCultureIgnoreCase));
      if (standardRoleIndex >= 0) return standardRoleIndex;
      if (role.Equals(DefaultRole, StringComparison.InvariantCultureIgnoreCase)) return int.MaxValue;
      return RoleOrder.Length;
    }

    private sealed record LineupMember(string Role, string Name);
  }
}
