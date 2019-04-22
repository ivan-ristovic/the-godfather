using DSharpPlus.Entities;

using System.Linq;

using TheGodfather.Database;
using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.EventListeners.Extensions
{
    public static class DatabaseContextExemptExtensions
    {
        public static bool IsExempted(this DiscordChannel channel, TheGodfatherShard shard)
        {
            using (DatabaseContext db = shard.Database.CreateContext()) {
                if (db.LoggingExempts.Any(ee => ee.GuildId == channel.GuildId &&
                                          ee.Type == ExemptedEntityType.Channel &&
                                         (ee.Id == channel.Id || ee.Id == channel.Parent.Id)))
                    return true;
            }
            return false;
        }

        public static bool IsExempted(this DiscordMember member, TheGodfatherShard shard)
        {
            if (member is null)
                return false;

            using (DatabaseContext db = shard.Database.CreateContext()) {
                if (db.LoggingExempts.Any(ee => ee.Type == ExemptedEntityType.Member && ee.Id == member.Id))
                    return true;
                if (member.Roles.Any(r => db.LoggingExempts.Any(ee => ee.Type == ExemptedEntityType.Role && ee.Id == r.Id)))
                    return true;
            }
            return false;
        }
    }
}
