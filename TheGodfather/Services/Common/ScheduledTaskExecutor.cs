using System.Threading;

namespace TheGodfather.Services.Common;

public sealed class ScheduledTaskExecutor : IDisposable
{
    public int Id => this.Job.Id;
    public ScheduledTask Job { get; }

    public delegate Task TaskExecuted(ScheduledTask task, bool force);
    public event TaskExecuted OnTaskExecuted;

    private readonly DiscordShardedClient client;
    private readonly LocalizationService lcs;
    private readonly AsyncExecutionService async;
    private Timer? timer;


    public ScheduledTaskExecutor(DiscordShardedClient client, LocalizationService lcs, AsyncExecutionService async, ScheduledTask task)
    {
        this.client = client;
        this.lcs = lcs;
        this.async = async;
        this.Job = task;
        this.OnTaskExecuted += (_, _) => Task.CompletedTask;
    }


    public void Dispose()
        => this.timer?.Dispose();

    public void ScheduleExecution()
    {
        switch (this.Job) {
            case GuildTask gt:
                this.timer = gt.Type switch {
                    ScheduledTaskType.Unban => new Timer(this.UnbanUserCallback, this.Job, this.Job.TimeUntilExecution, Timeout.InfiniteTimeSpan),
                    ScheduledTaskType.Unmute => new Timer(this.UnmuteUserCallback, this.Job, this.Job.TimeUntilExecution, Timeout.InfiniteTimeSpan),
                    _ => this.timer
                };
                break;
            case Reminder rem:
                this.timer = new Timer(this.SendMessageCallback, this.Job, rem.TimeUntilExecution, rem.RepeatInterval);
                break;
            default:
                throw new ArgumentException("Unknown saved task info type.", nameof(this.Job));
        }
    }

    public async Task HandleMissedExecutionAsync()
    {
        try {
            switch (this.Job) {
                case GuildTask gt when gt.Type is ScheduledTaskType.Unban:
                    this.UnbanUserCallback(this.Job);
                    break;
                case GuildTask gt when gt.Type is ScheduledTaskType.Unmute:
                    this.UnmuteUserCallback(this.Job);
                    break;
                case Reminder rem:
                    IMessageChannel? channel = await this.ResolveReminderChannelAsync(rem);
                    if (channel is null) {
                        Log.Warning("Cannot find channel for reminder {ReminderId} (channel: {ChannelId}, user: {UserId})", rem.Id, rem.ChannelId, rem.UserId);
                        break;
                    }

                    await this.SendLocalizedAsync(channel, Emojis.X, Color.Red,
                        TranslationKey.fmt_remind_miss(this.lcs.GetLocalizedTimeString((channel as IGuildChannel)?.GuildId, rem.ExecutionTime), rem.Message)
                    );
                    break;
                default:
                    throw new ArgumentException("Unknown saved task info type.", nameof(this.Job));
            }
            Log.Debug("Executed missed saved task of type: {SavedTaskType}", this.Job.GetType().Name);
        } catch (Exception e) {
            Log.Debug(e, "Error while handling missed saved task");
        }
    }


    private async Task<IMessageChannel?> ResolveReminderChannelAsync(Reminder rem)
    {
        if (rem.ChannelId != 0)
            return this.client.GetChannel(rem.ChannelId) as IMessageChannel;

        IUser? user = this.client.GetUser(rem.UserId);
        return user is null ? null : await user.CreateDMChannelAsync();
    }

    private Task SendLocalizedAsync(IMessageChannel channel, string emoji, Color color, TranslationKey key)
    {
        ulong? gid = (channel as IGuildChannel)?.GuildId;
        return channel.SendMessageAsync(embed: new EmbedBuilder {
            Color = color,
            Description = $"{emoji} {this.lcs.GetString(gid, key)}"
        }.Build());
    }

    private void SendMessageCallback(object? state)
    {
        Reminder rem = state as Reminder ?? throw new InvalidCastException("Failed to cast scheduled task to Reminder");

        try {
            IMessageChannel? channel = this.async.Execute(this.ResolveReminderChannelAsync(rem));
            if (channel is null)
                return;

            IUser? user = this.client.GetUser(rem.UserId);
            string mention = user?.Mention ?? MentionUtils.MentionUser(rem.UserId);
            this.async.Execute(this.SendLocalizedAsync(channel, Emojis.AlarmClock, Color.Green, TranslationKey.fmt_remind_exec(mention, rem.Message)));
        } catch (Discord.Net.HttpException e) when (e.DiscordCode == DiscordErrorCode.MissingPermissions) {
            // User disabled DMs or the bot lost channel access.
        } catch (Exception e) {
            Log.Debug(e, "Error while handling send message saved task");
        } finally {
            try {
                this.async.Execute(this.OnTaskExecuted(this.Job, false));
            } catch (Exception e) {
                Log.Error(e, "Error while unscheduling send message saved task");
            }
        }
    }

    private void UnbanUserCallback(object? state)
    {
        GuildTask gt = state as GuildTask ?? throw new InvalidCastException("Failed to cast scheduled task to GuildTask");

        try {
            SocketGuild? guild = this.client.GetGuild(gt.GuildId);
            if (guild is not null)
                this.async.Execute(guild.RemoveBanAsync(gt.UserId, new RequestOptions { AuditLogReason = "Temporary ban time expired" }));
        } catch (Discord.Net.HttpException e) when (e.DiscordCode is DiscordErrorCode.MissingPermissions or DiscordErrorCode.UnknownBan) {
            // Permissions changed or the user was already unbanned.
        } catch (Exception e) {
            Log.Debug(e, "Error while handling unban saved task");
        } finally {
            try {
                this.async.Execute(this.OnTaskExecuted(this.Job, false));
            } catch (Exception e) {
                Log.Error(e, "Error while unscheduling unban saved task");
            }
        }
    }

    private void UnmuteUserCallback(object? state)
    {
        GuildTask gt = state as GuildTask ?? throw new InvalidCastException("Failed to cast scheduled task to GuildTask");

        try {
            SocketGuild? guild = this.client.GetGuild(gt.GuildId);
            SocketRole? role = guild?.GetRole(gt.RoleId);
            SocketGuildUser? member = guild?.GetUser(gt.UserId);
            if (role is not null && member is not null)
                this.async.Execute(member.RemoveRoleAsync(role, new RequestOptions { AuditLogReason = "Temporary mute time expired" }));
        } catch (Discord.Net.HttpException e) when (e.DiscordCode is DiscordErrorCode.MissingPermissions or DiscordErrorCode.UnknownMember) {
            // Permissions changed or the member left.
        } catch (Exception e) {
            Log.Debug(e, "Error while handling unmute saved task");
        } finally {
            try {
                this.async.Execute(this.OnTaskExecuted(this.Job, false));
            } catch (Exception e) {
                Log.Error(e, "Error while unscheduling unmute saved task");
            }
        }
    }
}
