#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Humanizer;

using System;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("remind")]
    [Description("Manage reminders. Group call resends a message after given time span.")]
    [UsageExamples("!remind 1h Drink water!")]
    [RequireOwnerOrPermissions(Permissions.Administrator)]
    [Cooldown(3, 5, CooldownBucketType.Channel), NotBlocked]
    public class RemindersModule : TheGodfatherModule
    {

        public RemindersModule(SharedData shared, DBService db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.LightGray;
        }

        
        [GroupCommand, Priority(2)]
        public async Task RemindAsync(CommandContext ctx,
                                     [Description("Time span until reminder.")] TimeSpan timespan,
                                     [Description("Channel to send message to.")] DiscordChannel channel,
                                     [RemainingText, Description("What to send?")] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new InvalidCommandUsageException("Missing time or repeat string.");

            if (message.Length > 120)
                throw new InvalidCommandUsageException("Message must be shorter than 120 characters.");

            channel = channel ?? ctx.Channel;

            if (timespan.TotalMinutes < 1 || timespan.TotalDays > 31)
                throw new InvalidCommandUsageException("Time span cannot be less than 1 minute or greater than 31 days.");

            DateTimeOffset when = DateTimeOffset.Now + timespan;

            var task = new SendMessageTaskInfo(ctx.Channel.Id, ctx.User.Id, message, when);
            if (!await SavedTaskExecutor.TryScheduleAsync(this.Shared, this.Database, ctx.Client, task))
                throw new CommandFailedException("Failed to schedule saved task!");

            await this.InformAsync(ctx, StaticDiscordEmoji.AlarmClock, $"I will remind {channel.Mention} in {Formatter.Bold(timespan.Humanize(5))} ({when.ToUtcTimestamp()}) to:\n\n{Formatter.Italic(message)}", important: false);
        }

        [GroupCommand, Priority(1)]
        public Task RemindAsync(CommandContext ctx,
                               [Description("Channel to send message to.")] DiscordChannel channel,
                               [Description("Time span until reminder.")] TimeSpan timespan,
                               [RemainingText, Description("What to send?")] string message)
            => this.RemindAsync(ctx, timespan, channel, message);

        [GroupCommand, Priority(0)]
        public Task RemindAsync(CommandContext ctx,
                               [Description("Time span until reminder.")] TimeSpan timespan,
                               [RemainingText, Description("What to send?")] string message)
            => this.RemindAsync(ctx, timespan, null, message);
    }
}
