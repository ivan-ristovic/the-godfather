#region USING_DIRECTIVES
using DSharpPlus.Entities;

using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Database;
using TheGodfather.Database.Entities;
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
                if (entry is null || DateTime.UtcNow - entry.CreationTimestamp.ToUniversalTime() > TimeSpan.FromSeconds(5))
                    return null;
                return entry;
            } catch {

            }
            return null;
        }

        public static DatabaseGuildConfig GetGuildSettings(this DiscordGuild guild, DatabaseContextBuilder dbb)
        {
            DatabaseGuildConfig gcfg = null;
            using (DatabaseContext db = dbb.CreateContext())
                gcfg = guild.GetGuildConfig(db);
            return gcfg;
        }

        public static DatabaseGuildConfig GetGuildConfig(this DiscordGuild guild, DatabaseContext db)
            => db.GuildConfig.SingleOrDefault(cfg => cfg.GuildId == guild.Id);
    }
}
