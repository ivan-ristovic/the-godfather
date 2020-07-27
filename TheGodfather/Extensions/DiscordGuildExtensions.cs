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
        public static async Task<DiscordAuditLogEntry?> GetLatestAuditLogEntryAsync(this DiscordGuild guild, AuditLogActionType? type = null)
        {
            try {
                DiscordAuditLogEntry? entry = (await guild.GetAuditLogsAsync(1, action_type: type))?.FirstOrDefault();
                return entry is null || DateTime.UtcNow - entry.CreationTimestamp.ToUniversalTime() > TimeSpan.FromSeconds(5) ? null : entry;
            } catch {

            }
            return null;
        }

        public static async Task<T?> GetLatestAuditLogEntryAsync<T>(this DiscordGuild guild, AuditLogActionType? type = null) where T : DiscordAuditLogEntry
        {
            DiscordAuditLogEntry? entry = await GetLatestAuditLogEntryAsync(guild, type);
            return entry as T;
        }
    }
}
