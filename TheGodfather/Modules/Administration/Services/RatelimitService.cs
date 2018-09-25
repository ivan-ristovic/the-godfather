#region USING_DIRECTIVES
using DSharpPlus.Entities;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Common;
#endregion

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class RatelimitService : ProtectionService
    {
        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, UserRatelimitInfo>> guildSpamInfo;
        private readonly Timer refreshTimer;


        private static void RefreshCallback(object _)
        {
            var service = _ as RatelimitService;

            foreach (ulong gid in service.guildSpamInfo.Keys) {
                var toRemove = service.guildSpamInfo[gid]
                    .Where(kvp => !kvp.Value.IsActive)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (ulong uid in toRemove)
                    service.guildSpamInfo[gid].TryRemove(uid, out UserRatelimitInfo _);
            }
        }


        public RatelimitService(TheGodfatherShard shard)
            : base(shard)
        {
            this.guildSpamInfo = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, UserRatelimitInfo>>();
            this.refreshTimer = new Timer(RefreshCallback, this, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));
            this.reason = "_gf: Ratelimit hit";
        }


        public override bool TryAddGuildToWatch(ulong gid)
            => this.guildSpamInfo.TryAdd(gid, new ConcurrentDictionary<ulong, UserRatelimitInfo>());

        public override bool TryRemoveGuildFromWatch(ulong gid)
            => this.guildSpamInfo.TryRemove(gid, out _);


        public async Task HandleNewMessageAsync(DiscordGuild guild, DiscordUser member)
        {
            if (!this.guildSpamInfo.ContainsKey(guild.Id) && !this.TryAddGuildToWatch(guild.Id))
                throw new ConcurrentOperationException("Failed to add guild to ratelimit watch list!");

            RatelimitSettings settings = this.shard.SharedData.GetGuildConfig(guild.Id).RatelimitSettings;

            if (!this.guildSpamInfo[guild.Id].ContainsKey(member.Id)) {
                if (!this.guildSpamInfo[guild.Id].TryAdd(member.Id, new UserRatelimitInfo(settings.Sensitivity - 1)))
                    throw new ConcurrentOperationException("Failed to add member to ratelimit watch list!");
                return;
            }

            if (!this.guildSpamInfo[guild.Id][member.Id].TryDecrementAllowedMessageCount())
                await this.PunishMemberAsync(guild, member as DiscordMember, settings.Action);
        }
    }
}
