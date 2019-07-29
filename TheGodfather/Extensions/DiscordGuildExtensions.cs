#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Extensions
{
    internal static class DiscordGuildExtensions
    {
        public static async Task<T> GetLatestAuditLogEntryAsync<T>(this DiscordGuild guild, AuditLogActionType type) where T : DiscordAuditLogEntry
        {
            try {
                DiscordAuditLogEntry entry = (await guild.GetAuditLogsAsync(1, action_type: type))
                    ?.FirstOrDefault();
                return entry is null || DateTime.UtcNow - entry.CreationTimestamp.ToUniversalTime() > TimeSpan.FromSeconds(5) ? null : entry as T;
            } catch {

            }
            return null;
        }
    }
}
