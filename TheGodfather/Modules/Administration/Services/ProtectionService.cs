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
        protected SemaphoreSlim csem = new SemaphoreSlim(1, 1);
        protected string reason;


        public bool IsDisabled()
            => false;


        internal async Task PunishUserAsync(TheGodfatherShard shard, DiscordGuild guild, DiscordMember member, PunishmentActionType type, string reason)
        {
            DiscordRole muteRole;
            SavedTask task;

            bool failed = false;
            try {
                switch (type) {
                    case PunishmentActionType.Kick:
                        await member.RemoveAsync(this.reason);
                        break;
                    case PunishmentActionType.Mute:
                        muteRole = await GetOrCreateMuteRoleAsync(guild);
                        await member.GrantRoleAsync(muteRole, this.reason);
                        break;
                    case PunishmentActionType.PermanentBan:
                        await member.BanAsync(1, reason: this.reason);
                        break;
                    case PunishmentActionType.TemporaryBan:
                        await member.BanAsync(0, reason: this.reason);
                        task = new SavedTask() {
                            ExecutionTime = DateTime.UtcNow + TimeSpan.FromDays(1),
                            GuildId = guild.Id,
                            Type = SavedTaskType.Unban,
                            UserId = member.Id
                        };
                        await SavedTaskExecutor.TryScheduleAsync(shard.SharedData, shard.DatabaseService, shard.Client, task);
                        break;
                    case PunishmentActionType.TemporaryMute:
                        muteRole = await GetOrCreateMuteRoleAsync(guild);
                        await member.GrantRoleAsync(muteRole, this.reason);
                        task = new SavedTask() {
                            ExecutionTime = DateTime.UtcNow + TimeSpan.FromDays(1),
                            GuildId = guild.Id,
                            Type = SavedTaskType.Unmute,
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
                    Title = failed ? "User punish attempt failed! Check my permissions" : "User punished",
                    Color = DiscordColor.Red
                };
                emb.AddField("User", member.ToString(), inline: true);
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
