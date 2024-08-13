using System.Text.RegularExpressions;

namespace Domain.Utility
{
  public static class StringUtility
  {
    public static List<string> SmartSplit(this string input)
    {
      var matches = Regex.Matches(input, @"\[([^\[\]]+)\]|(\S+)");
      return matches.Cast<Match>().Select(m => m.Groups[1].Value != "" ? m.Groups[1].Value : m.Groups[2].Value).ToList();
    }
  }
}
