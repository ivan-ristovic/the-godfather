#region USING_DIRECTIVES
using DSharpPlus.Entities;

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


        public async Task HandleMemberJoinAsync(DiscordGuild guild, DiscordMember member)
        {
            if (!this.guildNewMembers.ContainsKey(guild.Id) && !this.TryAddGuildToWatch(guild.Id))
                throw new ConcurrentOperationException("Failed to add guild to antiflood watch list!");

            if (!this.guildNewMembers[guild.Id].Add(member))
                throw new ConcurrentOperationException("Failed to add member to antiflood watch list!");

            await Task.Delay(TimeSpan.FromSeconds(5));

            if (this.guildNewMembers.ContainsKey(guild.Id) && !this.guildNewMembers[guild.Id].TryRemove(member))
                throw new ConcurrentOperationException("Failed to remove member from antiflood watch list!");
        }

        public async Task<bool> HandleMemberLeaveAsync(DiscordGuild guild, DiscordMember member)
        {
            if (!this.guildNewMembers.ContainsKey(guild.Id) || !this.guildNewMembers[guild.Id].Contains(member))
                return false;

            await this.PunishMemberAsync(guild, member, PunishmentActionType.PermanentBan);
            return true;
        }
    }
}
