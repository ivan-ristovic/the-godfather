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
    public sealed class AntispamService : ProtectionService
    {
        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, UserSpamInfo>> guildSpamInfo;
        private readonly Timer refreshTimer;


        private static void RefreshCallback(object _)
        {
            var service = _ as AntispamService;

            foreach (ulong gid in service.guildSpamInfo.Keys) {
                IEnumerable<ulong> toRemove = service.guildSpamInfo[gid]
                    .Where(kvp => !kvp.Value.IsActive)
                    .Select(kvp => kvp.Key);

                foreach (ulong uid in toRemove) 
                    service.guildSpamInfo[gid].TryRemove(uid, out UserSpamInfo _);
            }
        }


        public AntispamService(TheGodfatherShard shard)
            : base(shard)
        {
            this.guildSpamInfo = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, UserSpamInfo>>();
            this.refreshTimer = new Timer(RefreshCallback, this, TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
            this.reason = "_gf: Antispam";
        }


        public override bool TryAddGuildToWatch(ulong gid)
            => this.guildSpamInfo.TryAdd(gid, new ConcurrentDictionary<ulong, UserSpamInfo>());

        public override bool TryRemoveGuildFromWatch(ulong gid)
            => this.guildSpamInfo.TryRemove(gid, out _);


        public async Task HandleNewMessageAsync(MessageCreateEventArgs e, AntispamSettings settings)
        {
            if (!this.guildSpamInfo.ContainsKey(e.Guild.Id) && !this.TryAddGuildToWatch(e.Guild.Id))
                throw new ConcurrentOperationException("Failed to add guild to ratelimit watch list!");

            if (!this.guildSpamInfo[e.Guild.Id].ContainsKey(e.Author.Id)) {
                if (!this.guildSpamInfo[e.Guild.Id].TryAdd(e.Author.Id, new UserSpamInfo(settings.Sensitivity)))
                    throw new ConcurrentOperationException("Failed to add member to ratelimit watch list!");
                return;
            }

            if (!this.guildSpamInfo[e.Guild.Id][e.Author.Id].TryDecrementAllowedMessageCount(e.Message.Content))
                await this.PunishMemberAsync(e.Guild, e.Author as DiscordMember, settings.Action);
        }
    }
}
