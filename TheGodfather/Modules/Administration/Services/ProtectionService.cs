using System.Threading.Tasks;
using DSharpPlus.Entities;
using TheGodfather.Database;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public class ProtectionService : ProtectionServiceBase
    {
        public ProtectionService(DbContextBuilder dbb, LoggingService ls, SchedulingService ss, GuildConfigService gcs)
            : base(dbb, ls, ss, gcs, "_gf: Punishment") { }


        public async Task<bool> ReapplyPunishmentIfNececaryAsync(DiscordGuild guild, DiscordMember member)
        {
            return false;
        }

        public override void Dispose() { }
        public override bool TryAddGuildToWatch(ulong gid) => true;
        public override bool TryRemoveGuildFromWatch(ulong gid) => true;
    }
}
