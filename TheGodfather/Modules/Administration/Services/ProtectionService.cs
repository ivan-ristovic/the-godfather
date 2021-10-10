using TheGodfather.Database;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public class ProtectionService : ProtectionServiceBase
    {
        public ProtectionService(DbContextBuilder dbb, LoggingService ls, SchedulingService ss, GuildConfigService gcs)
            : base(dbb, ls, ss, gcs, "_gf: Punishment") { }


        public override bool TryAddGuildToWatch(ulong gid) => true;
        public override bool TryRemoveGuildFromWatch(ulong gid) => true;
    }
}
