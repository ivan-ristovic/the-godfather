using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace TheGodfather.Extensions
{
    internal static class DiscordGuildExtensions
    {
        public static async Task<bool> HasMember(this DiscordGuild guild, ulong uid)
            => await GetMemberSilentAsync(guild, uid) is { };

        public static async Task<DiscordMember?> GetMemberSilentAsync(this DiscordGuild guild, ulong uid)
        {
            try {
                return await guild.GetMemberAsync(uid);
            } catch (NotFoundException) {
                return null;
            }
        }

        public static async Task<DiscordAuditLogEntry?> GetRecentAuditLogEntryAsync(this DiscordGuild guild, AuditLogActionType? type = null, 
                                                                                    TimeSpan? period = null)
        {
            period ??= TimeSpan.FromSeconds(5);
            try {
                DiscordAuditLogEntry? entry = (await guild.GetAuditLogsAsync(1, action_type: type))?.FirstOrDefault();
                return entry is { } && DateTimeOffset.UtcNow - entry.CreationTimestamp.ToUniversalTime() <= period ? entry : null;
            } catch {
                // no perms, ignored
            }
            return null;
        }

        public static async Task<T?> GetLatestAuditLogEntryAsync<T>(this DiscordGuild guild, AuditLogActionType? type = null, 
                                                                    TimeSpan? period = null) 
            where T : DiscordAuditLogEntry
        {
            DiscordAuditLogEntry? entry = await GetRecentAuditLogEntryAsync(guild, type, period);
            return entry as T;
        }
    }
}
