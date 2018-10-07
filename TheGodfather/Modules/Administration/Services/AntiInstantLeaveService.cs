#region USING_DIRECTIVES
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using TheGodfather.Common.Collections;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Common;
#endregion

namespace TheGodfather.Modules.Administration.Services
{
    public class AntiInstantLeaveService : ProtectionService
    {
        private readonly ConcurrentDictionary<ulong, ConcurrentHashSet<DiscordMember>> guildNewMembers;


        public AntiInstantLeaveService(TheGodfatherShard shard)
            : base(shard)
        {
            this.guildNewMembers = new ConcurrentDictionary<ulong, ConcurrentHashSet<DiscordMember>>();
            this.reason = "_gf: Instant leave";
        }


        public override bool TryAddGuildToWatch(ulong gid)
            => this.guildNewMembers.TryAdd(gid, new ConcurrentHashSet<DiscordMember>());

        public override bool TryRemoveGuildFromWatch(ulong gid)
            => this.guildNewMembers.TryRemove(gid, out _);


        public async Task HandleMemberJoinAsync(GuildMemberAddEventArgs e, AntiInstantLeaveSettings settings)
        {
            if (!this.guildNewMembers.ContainsKey(e.Guild.Id) && !this.TryAddGuildToWatch(e.Guild.Id))
                throw new ConcurrentOperationException("Failed to add guild to antiflood watch list!");

            if (!this.guildNewMembers[e.Guild.Id].Add(e.Member))
                throw new ConcurrentOperationException("Failed to add member to antiflood watch list!");

            await Task.Delay(TimeSpan.FromSeconds(settings.Cooldown));

            if (this.guildNewMembers.ContainsKey(e.Guild.Id) && !this.guildNewMembers[e.Guild.Id].TryRemove(e.Member))
                throw new ConcurrentOperationException("Failed to remove member from antiflood watch list!");
        }

        public async Task<bool> HandleMemberLeaveAsync(GuildMemberRemoveEventArgs e, AntiInstantLeaveSettings settings)
        {
            if (!this.guildNewMembers.ContainsKey(e.Guild.Id) || !this.guildNewMembers[e.Guild.Id].Contains(e.Member))
                return false;

            await this.PunishMemberAsync(e.Guild, e.Member, PunishmentActionType.PermanentBan);
            return true;
        }
    }
}
