#region USING_DIRECTIVES
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Common;
#endregion

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class RatelimitService : ProtectionService
    {
        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, UserRatelimitInfo>> guildRatelimitInfo;
        private readonly Timer refreshTimer;


        private static void RefreshCallback(object _)
        {
            var service = _ as RatelimitService;

            foreach (ulong gid in service.guildRatelimitInfo.Keys) {
                IEnumerable<ulong> toRemove = service.guildRatelimitInfo[gid]
                    .Where(kvp => !kvp.Value.IsActive)
                    .Select(kvp => kvp.Key);

                foreach (ulong uid in toRemove)
                    service.guildRatelimitInfo[gid].TryRemove(uid, out UserRatelimitInfo _);
            }
        }


        public RatelimitService(TheGodfatherShard shard)
            : base(shard)
        {
            this.guildRatelimitInfo = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, UserRatelimitInfo>>();
            this.refreshTimer = new Timer(RefreshCallback, this, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));
            this.reason = "_gf: Ratelimit hit";
        }


        public override bool TryAddGuildToWatch(ulong gid)
            => this.guildRatelimitInfo.TryAdd(gid, new ConcurrentDictionary<ulong, UserRatelimitInfo>());

        public override bool TryRemoveGuildFromWatch(ulong gid)
            => this.guildRatelimitInfo.TryRemove(gid, out _);


        public async Task HandleNewMessageAsync(MessageCreateEventArgs e, RatelimitSettings settings)
        {
            if (!this.guildRatelimitInfo.ContainsKey(e.Guild.Id) && !this.TryAddGuildToWatch(e.Guild.Id))
                throw new ConcurrentOperationException("Failed to add guild to ratelimit watch list!");

            if (!this.guildRatelimitInfo[e.Guild.Id].ContainsKey(e.Author.Id)) {
                if (!this.guildRatelimitInfo[e.Guild.Id].TryAdd(e.Author.Id, new UserRatelimitInfo(settings.Sensitivity - 1)))
                    throw new ConcurrentOperationException("Failed to add member to ratelimit watch list!");
                return;
            }

            if (!this.guildRatelimitInfo[e.Guild.Id][e.Author.Id].TryDecrementAllowedMessageCount())
                await this.PunishMemberAsync(e.Guild, e.Author as DiscordMember, settings.Action);
        }
    }
}
