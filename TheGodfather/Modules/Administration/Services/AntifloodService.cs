#region USING_DIRECTIVES
using DSharpPlus.Entities;

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Exceptions;
#endregion

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class AntifloodService : ProtectionService
    {
        private readonly ConcurrentDictionary<ulong, ConcurrentHashSet<DiscordMember>> guildFloodUsers;


        public AntifloodService(TheGodfatherShard shard)
            : base(shard)
        {
            this.guildFloodUsers = new ConcurrentDictionary<ulong, ConcurrentHashSet<DiscordMember>>();
            this.reason = "_gf: Flooding";
        }


        public override bool TryAddGuildToWatch(ulong gid)
            => this.guildFloodUsers.TryAdd(gid, new ConcurrentHashSet<DiscordMember>());

        public override bool TryRemoveGuildFromWatch(ulong gid)
            => this.guildFloodUsers.TryRemove(gid, out _);


        public async Task HandleMemberJoinAsync(DiscordGuild guild, DiscordMember member)
        {
            if (!this.guildFloodUsers.ContainsKey(guild.Id) && !this.TryAddGuildToWatch(guild.Id))
                throw new ConcurrentOperationException("Failed to add guild to antiflood watch list!");

            if (!this.guildFloodUsers[guild.Id].Add(member))
                throw new ConcurrentOperationException("Failed to add member to antiflood watch list!");

            CachedGuildConfig gcfg = this.shard.SharedData.GetGuildConfig(guild.Id);

            if (this.guildFloodUsers[guild.Id].Count >= gcfg.AntifloodSensitivity) {
                foreach (DiscordMember m in this.guildFloodUsers[guild.Id]) {
                    await this.PunishMemberAsync(guild, m, gcfg.AntifloodAction);
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                }
                this.guildFloodUsers[guild.Id].Clear();
            }

            await Task.Delay(TimeSpan.FromSeconds(gcfg.AntifloodCooldown));

            if (this.guildFloodUsers.ContainsKey(guild.Id) && !this.guildFloodUsers[guild.Id].TryRemove(member))
                throw new ConcurrentOperationException("Failed to remove member from antiflood watch list!");
        }
    }
}
