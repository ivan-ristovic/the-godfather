using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Serilog;
using TheGodfather.Common;
using TheGodfather.Extensions;

namespace TheGodfather.Services.Common
{
    public sealed class SavedTaskExecutor : IDisposable
    {
        public int Id { get; private set; }
        public SavedTaskInfo TaskInfo { get; }

        public delegate Task TaskExecuted(int id, SavedTaskInfo tinfo);
        public event TaskExecuted OnTaskExecuted;

        private readonly TheGodfatherShard shard;
        private readonly AsyncExecutionService async;
        private Timer timer;


        public SavedTaskExecutor(int id, TheGodfatherShard shard, AsyncExecutionService async, SavedTaskInfo task)
        {
            this.Id = id;
            this.TaskInfo = task;
            this.shard = shard;
            this.async = async;
        }


        public void Dispose()
            => this.timer?.Dispose();

        public void ScheduleExecution()
        {
            switch (this.TaskInfo) {
                case SendMessageTaskInfo smti:
                    this.timer = new Timer(this.SendMessageCallback, this.TaskInfo, smti.TimeUntilExecution, smti.RepeatingInterval);
                    break;
                case UnbanTaskInfo _:
                    this.timer = new Timer(this.UnbanUserCallback, this.TaskInfo, this.TaskInfo.TimeUntilExecution, TimeSpan.FromMilliseconds(-1));
                    break;
                case UnmuteTaskInfo _:
                    this.timer = new Timer(this.UnmuteUserCallback, this.TaskInfo, this.TaskInfo.TimeUntilExecution, TimeSpan.FromMilliseconds(-1));
                    break;
                default:
                    throw new ArgumentException("Unknown saved task info type!", nameof(this.TaskInfo));
            }
        }

        public async Task HandleMissedExecutionAsync()
        {
            try {
                switch (this.TaskInfo) {
                    case SendMessageTaskInfo smti:
                        DiscordChannel channel;
                        channel = smti.ChannelId != 0
                            ? await this.shard.Client.GetChannelAsync(smti.ChannelId)
                            : await this.shard.Client.CreateDmChannelAsync(smti.InitiatorId);
                        DiscordUser user = await this.shard.Client.GetUserAsync(smti.InitiatorId);
                        await channel?.SendMessageAsync($"{user.Mention}'s reminder:", embed: new DiscordEmbedBuilder {
                            Description = $"{Emojis.X} I have been asleep and failed to remind {user.Mention} to:\n\n{smti.Message}\n\n{smti.ExecutionTime.ToUtcTimestamp()}",
                            Color = DiscordColor.Red
                        });
                        break;
                    case UnbanTaskInfo _:
                        this.UnbanUserCallback(this.TaskInfo);
                        break;
                    case UnmuteTaskInfo _:
                        this.UnmuteUserCallback(this.TaskInfo);
                        break;
                }
                Log.Debug("Executed missed saved task of type: {SavedTaskType}", this.TaskInfo.GetType().ToString());
            } catch (Exception e) {
                Log.Debug(e, "Error while handling missed saved task");
            }
        }


        #region Callbacks
        private void SendMessageCallback(object _)
        {
            var info = _ as SendMessageTaskInfo;

            try {
                DiscordChannel channel = info.ChannelId != 0
                    ? this.async.Execute(this.shard.Client.GetChannelAsync(info.ChannelId))
                    : this.async.Execute(this.shard.Client.CreateDmChannelAsync(info.InitiatorId));
                DiscordUser user = this.async.Execute(this.shard.Client.GetUserAsync(info.InitiatorId));
                this.async.Execute(channel.SendMessageAsync($"{user.Mention}'s reminder:", embed: new DiscordEmbedBuilder {
                    Description = $"{Emojis.AlarmClock} {info.Message}",
                    Color = DiscordColor.Orange
                }));
            } catch (UnauthorizedException) {
                // Do nothing, user has disabled DM in meantime
            } catch (Exception e) {
                Log.Debug(e, "Error while handling send message saved task");
            } finally {
                if (!info.IsRepeating) {
                    try {
                        this.async.Execute(this.OnTaskExecuted(this.Id, this.TaskInfo));
                    } catch (Exception e) {
                        Log.Error(e, "Error while unscheduling send message saved task");
                    }
                }
            }
        }

        private void UnbanUserCallback(object _)
        {
            var info = _ as UnbanTaskInfo;

            try {
                DiscordGuild guild = this.async.Execute(this.shard.Client.GetGuildAsync(info.GuildId));
                this.async.Execute(guild.UnbanMemberAsync(info.UnbanId, $"Temporary ban time expired"));
            } catch (UnauthorizedException) {
                // Do nothing, perms to unban removed in meantime
            } catch (Exception e) {
                Log.Debug(e, "Error while handling unban saved task");
            } finally {
                try {
                    this.async.Execute(this.OnTaskExecuted(this.Id, this.TaskInfo));
                } catch (Exception e) {
                    Log.Error(e, "Error while unscheduling unban saved task");
                }
            }
        }

        private void UnmuteUserCallback(object _)
        {
            var info = _ as UnmuteTaskInfo;

            try {
                DiscordGuild guild = this.async.Execute(this.shard.Client.GetGuildAsync(info.GuildId));
                DiscordRole role = guild.GetRole(info.MuteRoleId);
                DiscordMember member = this.async.Execute(guild.GetMemberAsync(info.UserId));
                if (role is null)
                    return;
                this.async.Execute(member.RevokeRoleAsync(role, $"Temporary mute time expired"));
            } catch (UnauthorizedException) {
                // Do nothing, perms to unmute removed in meantime
            } catch (Exception e) {
                Log.Debug(e, "Error while handling unmute saved task");
            } finally {
                try {
                    this.async.Execute(this.OnTaskExecuted(this.Id, this.TaskInfo));
                } catch (Exception e) {
                    Log.Error(e, "Error while unscheduling unmute saved task");
                }
            }
        }
        #endregion
    }
}
