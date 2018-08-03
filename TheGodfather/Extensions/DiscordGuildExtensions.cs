#region USING_DIRECTIVES
using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Extensions
{
    internal static class DiscordGuildExtensions
    {
        public static async Task<DiscordAuditLogEntry> GetFirstAuditLogEntryAsync(this DiscordGuild guild, AuditLogActionType type)
        {
            try {
                var entry = (await guild.GetAuditLogsAsync(1, action_type: type))
                    ?.FirstOrDefault();
                if (entry == null || DateTime.UtcNow - entry.CreationTimestamp.ToUniversalTime() > TimeSpan.FromSeconds(5))
                    return null;
                return entry;
            } catch {

            }
            return null;
        }
    }
}
