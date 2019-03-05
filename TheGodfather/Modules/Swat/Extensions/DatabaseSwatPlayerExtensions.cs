#region USING_DIRECTIVES
using DSharpPlus;

using TheGodfather.Database.Entities;
#endregion

namespace TheGodfather.Modules.Swat.Extensions
{
    public static class DatabaseSwatPlayerExtensions
    {
        public static string Stringify(this DatabaseSwatPlayer p)
        {
            return $"{Formatter.Bold(p.Name)} {(p.IsBlacklisted ? " (BLACKLISTED)" : "")}\n" +
                string.Join(", ", p.Aliases) + "\n" +
                Formatter.BlockCode(string.Join('\n', p.IPs)) +
                Formatter.Italic(p.Info ?? "No info provided.");
        }
    }
}
