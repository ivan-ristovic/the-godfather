#region USING_DIRECTIVES
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TheGodfather.Common.Collections;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
#endregion

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class AntispamService : ProtectionService
    {
        private readonly ConcurrentDictionary<ulong, ConcurrentHashSet<ulong>> exemptedChannels;
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
            this.exemptedChannels = new ConcurrentDictionary<ulong, ConcurrentHashSet<ulong>>();
            this.guildSpamInfo = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, UserSpamInfo>>();
            this.refreshTimer = new Timer(RefreshCallback, this, TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
            this.reason = "_gf: Antispam";
        }


        public override bool TryAddGuildToWatch(ulong gid)
        {
            bool success = true;
            success &= this.exemptedChannels.TryAdd(gid, new ConcurrentHashSet<ulong>());
            success &= this.guildSpamInfo.TryAdd(gid, new ConcurrentDictionary<ulong, UserSpamInfo>());
            return success;
        }

        public override bool TryRemoveGuildFromWatch(ulong gid)
        {
            bool success = true;
            success &= this.exemptedChannels.TryRemove(gid, out _);
            success &= this.guildSpamInfo.TryRemove(gid, out _);
            return success;
        }


        public async Task LoadExemptedChannelsForGuildAsync(ulong gid)
        {
            IReadOnlyList<ulong> exempts = await this.shard.DatabaseService.GetAntispamExemptsForGuildAsync(gid);
            this.exemptedChannels[gid] = new ConcurrentHashSet<ulong>(exempts);
        }

        public async Task HandleNewMessageAsync(MessageCreateEventArgs e, AntispamSettings settings)
        {
            if (!this.guildSpamInfo.ContainsKey(e.Guild.Id)) {
                if (!this.TryAddGuildToWatch(e.Guild.Id))
                    throw new ConcurrentOperationException("Failed to add guild to antispam watch list!");
                await this.LoadExemptedChannelsForGuildAsync(e.Guild.Id);
            }

            if (this.exemptedChannels.TryGetValue(e.Guild.Id, out var exempts) && exempts.Contains(e.Channel.Id))
                return;

            var gSpamInfo = this.guildSpamInfo[e.Guild.Id];
            if (!gSpamInfo.ContainsKey(e.Author.Id)) {
                if (!gSpamInfo.TryAdd(e.Author.Id, new UserSpamInfo(settings.Sensitivity)))
                    throw new ConcurrentOperationException("Failed to add member to antispam watch list!");
                return;
            }

            if (gSpamInfo.TryGetValue(e.Author.Id, out UserSpamInfo spamInfo) && !spamInfo.TryDecrementAllowedMessageCount(e.Message.Content)) {
                await this.PunishMemberAsync(e.Guild, e.Author as DiscordMember, settings.Action);
                spamInfo.Reset();
            }
        }
    }
}
