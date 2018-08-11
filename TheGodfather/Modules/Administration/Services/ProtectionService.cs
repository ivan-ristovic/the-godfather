using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public abstract class ProtectionService : ITheGodfatherService
    {
        protected string reason;
        protected SemaphoreSlim csem = new SemaphoreSlim(1, 1);


        public bool IsDisabled()
            => false;


        internal async Task PunishUserAsync(TheGodfatherShard shard, ulong gid, ulong uid, string reason)
        {
            DiscordGuild guild = await shard.Client.GetGuildAsync(gid);
            DiscordMember member = await guild.GetMemberAsync(uid);

            bool failed = false;
            try {
                switch (shard.SharedData.GuildConfigurations[gid].RatelimitHitAction) {
                    case PunishmentActionType.Kick:
                        await member.RemoveAsync(this.reason);
                        break;
                    case PunishmentActionType.Mute:
                        DiscordRole muteRole = await GetOrCreateMuteRoleAsync(guild);
                        await member.GrantRoleAsync(muteRole, this.reason);
                        break;

                    // TODO tempmute

                    case PunishmentActionType.PermanentBan:
                        await member.BanAsync(1, reason: this.reason);
                        break;
                    case PunishmentActionType.TemporaryBan:
                        await member.BanAsync(0, reason: this.reason);
                        var task = new SavedTask() {
                            ExecutionTime = DateTime.UtcNow + TimeSpan.FromDays(1),
                            GuildId = gid,
                            Type = SavedTaskType.Unban,
                            UserId = member.Id
                        };
                        await SavedTaskExecutor.TryScheduleAsync(shard.SharedData, shard.DatabaseService, shard.Client, task);
                        break;
                }
            } catch {
                failed = true;
            }

            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = failed ? "User punish attempt failed" : "User punished",
                    Color = DiscordColor.Red
                };
                emb.AddField("ID", uid.ToString(), inline: true);
                emb.AddField("Reason", reason, inline: true);
                await logchn.SendMessageAsync(embed: emb.Build());
            }
        }


        private async Task<DiscordRole> GetOrCreateMuteRoleAsync(DiscordGuild guild)
        {
            DiscordRole muteRole = null;

            await this.csem.WaitAsync();
            try {
                muteRole = guild.Roles.FirstOrDefault(r => r.Name.ToLowerInvariant() == "gf_mute");
                if (muteRole == null) {
                    muteRole = await guild.CreateRoleAsync("gf_mute", hoist: false, mentionable: false);
                    foreach (DiscordChannel channel in guild.Channels.Where(c => c.Type == ChannelType.Text)) {
                        await channel.AddOverwriteAsync(muteRole, deny: Permissions.SendMessages | Permissions.SendTtsMessages | Permissions.AddReactions);
                        await Task.Delay(200);
                    }
                }
            } finally {
                this.csem.Release();
            }

            return muteRole;
        }
    }
}
