#region USING_DIRECTIVES
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class RatelimitService : ITheGodfatherService
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


        public RatelimitService()
        {
            this.guildSpamInfo = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, UserRatelimitInfo>>();
            this.refreshTimer = new Timer(RefreshCallback, this, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));
        }


        public bool TryAddGuildToWatch(ulong gid)
            => this.guildSpamInfo.TryAdd(gid, new ConcurrentDictionary<ulong, UserRatelimitInfo>());

        public bool HandleNewMessage(ulong gid, ulong uid, int maxAllowed)
        {
            if (!this.guildSpamInfo.ContainsKey(gid) && !TryAddGuildToWatch(gid))
                throw new ConcurrentOperationException("Failed to add guild to watch list!");

            if (!this.guildSpamInfo[gid].ContainsKey(uid)) {
                if (!this.guildSpamInfo[gid].TryAdd(uid, new UserRatelimitInfo(maxAllowed - 1)))
                    throw new ConcurrentOperationException("Failed to add guild to watch list!");
                return true;
            }

            return this.guildSpamInfo[gid][uid].TryDecrementAllowedMessageCount();
        }

        public bool TryRemoveGuildFromWatch(ulong gid)
            => true;

        public bool IsDisabled()
            => false;



        internal async Task PunishUserAsync(TheGodfatherShard shard, ulong gid, ulong uid)
        {
            DiscordGuild guild = await shard.Client.GetGuildAsync(gid);
            DiscordMember member = await guild.GetMemberAsync(uid);
            // await member.GrantRoleAsync()
            await guild.GetDefaultChannel().SendMessageAsync($"Slapped {member.DisplayName}!");
        }
    }
}
