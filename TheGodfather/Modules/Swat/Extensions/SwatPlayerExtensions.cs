#region USING_DIRECTIVES
using DSharpPlus;
using TheGodfather.Database.Models;
#endregion

namespace TheGodfather.Modules.Swat.Extensions
{
    public static class SwatPlayerExtensions
    {
        public static string Stringify(this SwatPlayer p)
        {
            return $"{Formatter.Bold(p.Name)} {(p.IsBlacklisted ? " (BLACKLISTED)" : "")}\n" +
                string.Join(", ", p.Aliases) + "\n" +
                Formatter.BlockCode(string.Join('\n', p.IPs)) +
                Formatter.Italic(p.Info ?? "No info provided.");
        }
    }
}
