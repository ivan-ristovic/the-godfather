#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Administration.Services
{
    public abstract class ProtectionService : ITheGodfatherService
    {
        protected TheGodfatherShard shard;
        protected SemaphoreSlim csem = new SemaphoreSlim(1, 1);
        protected string reason;


        protected ProtectionService(TheGodfatherShard shard)
        {
            this.shard = shard;
        }


        public bool IsDisabled()
            => false;

        public async Task PunishMemberAsync(DiscordGuild guild, DiscordMember member, PunishmentActionType type)
        {
            try {
                DiscordRole muteRole;
                SavedTaskInfo task;
                switch (type) {
                    case PunishmentActionType.Kick:
                        await member.RemoveAsync(this.reason);
                        break;
                    case PunishmentActionType.Mute:
                        muteRole = await this.GetOrCreateMuteRoleAsync(guild);
                        if (member.Roles.Contains(muteRole))
                            return;
                        await member.GrantRoleAsync(muteRole, this.reason);
                        break;
                    case PunishmentActionType.PermanentBan:
                        await member.BanAsync(1, reason: this.reason);
                        break;
                    case PunishmentActionType.TemporaryBan:
                        await member.BanAsync(0, reason: this.reason);
                        task = new UnbanTaskInfo(guild.Id, member.Id);
                        await SavedTaskExecutor.ScheduleAsync(this.shard.SharedData, this.shard.DatabaseService, this.shard.Client, task);
                        break;
                    case PunishmentActionType.TemporaryMute:
                        muteRole = await this.GetOrCreateMuteRoleAsync(guild);
                        if (member.Roles.Contains(muteRole))
                            return;
                        await member.GrantRoleAsync(muteRole, this.reason);
                        task = new UnmuteTaskInfo(guild.Id, member.Id, muteRole.Id);
                        await SavedTaskExecutor.ScheduleAsync(this.shard.SharedData, this.shard.DatabaseService, this.shard.Client, task);
                        break;
                }
            } catch {
                DiscordChannel logchn = this.shard.SharedData.GetLogChannelForGuild(this.shard.Client, guild);
                if (logchn != null) {
                    var emb = new DiscordEmbedBuilder() {
                        Title = "User punish attempt failed! Check my permissions",
                        Color = DiscordColor.Red
                    };
                    emb.AddField("User", member?.ToString() ?? "unknown", inline: true);
                    emb.AddField("Reason", this.reason, inline: false);
                    await logchn.SendMessageAsync(embed: emb.Build());
                }
            }
        }

        public async Task<DiscordRole> GetOrCreateMuteRoleAsync(DiscordGuild guild)
        {
            DiscordRole muteRole = null;

            await this.csem.WaitAsync();
            try {
                muteRole = await this.shard.DatabaseService.GetMuteRoleAsync(guild);
                if (muteRole == null)
                    muteRole = guild.Roles.FirstOrDefault(r => r.Name.ToLowerInvariant() == "gf_mute");
                if (muteRole == null) {
                    muteRole = await guild.CreateRoleAsync("gf_mute", hoist: false, mentionable: false);
                    await this.shard.DatabaseService.SetMuteRoleAsync(guild.Id, muteRole.Id);
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


        public abstract bool TryAddGuildToWatch(ulong gid);
        public abstract bool TryRemoveGuildFromWatch(ulong gid);
    }
}
