#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Humanizer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Owner.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("remind")]
    [Description("Manage reminders. Group call resends a message after given time span.")]
    [Aliases("reminders", "reminder", "todo")]
    [UsageExamples("!remind 1h Drink water!")]
    [Cooldown(3, 5, CooldownBucketType.Channel), NotBlocked]
    public class RemindersModule : TheGodfatherModule
    {

        public RemindersModule(SharedData shared, DBService db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.LightGray;
        }


        [GroupCommand, Priority(3)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Time span until reminder.")] TimeSpan timespan,
                                     [Description("Channel to send message to.")] DiscordChannel channel,
                                     [RemainingText, Description("What to send?")] string message)
            => this.AddAsync(ctx, timespan, channel, message);

        [GroupCommand, Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Channel to send message to.")] DiscordChannel channel,
                                     [Description("Time span until reminder.")] TimeSpan timespan,
                                     [RemainingText, Description("What to send?")] string message)
            => this.AddAsync(ctx, timespan, channel, message);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Time span until reminder.")] TimeSpan timespan,
                                     [RemainingText, Description("What to send?")] string message)
            => this.AddAsync(ctx, timespan, null, message);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);



        #region COMMAND_REMINDERS_ADD
        [Command("add"), Priority(2)]
        [Description("Schedule a new reminder. You can also specify a channel where to send the reminder.")]
        [Aliases("new", "+", "a", "+=", "<", "<<")]
        [UsageExamples("!remind add 1h Drink water!")]
        public Task AddAsync(CommandContext ctx,
                            [Description("Time span until reminder.")] TimeSpan timespan,
                            [Description("Channel to send message to.")] DiscordChannel channel,
                            [RemainingText, Description("What to send?")] string message)
            => this.AddReminderAsync(ctx, timespan, channel, message);

        [Command("add"), Priority(1)]
        public Task AddAsync(CommandContext ctx,
                            [Description("Channel to send message to.")] DiscordChannel channel,
                            [Description("Time span until reminder.")] TimeSpan timespan,
                            [RemainingText, Description("What to send?")] string message)
            => this.AddReminderAsync(ctx, timespan, channel, message);

        [Command("add"), Priority(0)]
        public Task AddAsync(CommandContext ctx,
                            [Description("Time span until reminder.")] TimeSpan timespan,
                            [RemainingText, Description("What to send?")] string message)
            => this.AddReminderAsync(ctx, timespan, null, message);
        #endregion

        #region COMMAND_REMINDERS_DELETE
        [Command("delete")]
        [Description("Unschedule a reminder.")]
        [Aliases("-", "remove", "rm", "del", "-=", ">", ">>", "unschedule")]
        [UsageExamples("!remind delete 1")]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Reminder ID.")] params int[] ids)
        {
            if (!ids.Any())
                throw new InvalidCommandUsageException("Missing IDs of reminders to remove.");

            if (!this.Shared.RemindExecuters.ContainsKey(ctx.User.Id))
                throw new CommandFailedException("You have no reminders scheduled.");

            var eb = new StringBuilder();
            foreach (int id in ids) {
                if (!this.Shared.RemindExecuters[ctx.User.Id].Any(texec => texec.Id == id)) {
                    eb.AppendLine($"Reminder with ID {Formatter.Bold(id.ToString())} does not exist (or is not scheduled by you)!");
                    continue;
                }

                await SavedTaskExecutor.UnscheduleAsync(this.Shared, ctx.User.Id, id);
            }

            if (eb.Length > 0)
                await this.InformFailureAsync(ctx, $"Action finished with following warnings/errors:\n\n{eb.ToString()}");
            else
                await this.InformAsync(ctx, "Successfully removed all specified reminders.", important: false);
        }
        #endregion

        #region COMMAND_REMINDERS_LIST
        [Command("list")]
        [Description("List your registered reminders in the current channel.")]
        [Aliases("ls")]
        [UsageExamples("!remind list")]
        public Task ListAsync(CommandContext ctx)
        {
            if (!this.Shared.RemindExecuters.ContainsKey(ctx.User.Id) || !this.Shared.RemindExecuters[ctx.User.Id].Any(t => ((SendMessageTaskInfo)t.TaskInfo).ChannelId == ctx.Channel.Id))
                throw new CommandFailedException("No reminders meet the speficied criteria.");

            return ctx.SendCollectionInPagesAsync(
                $"Your reminders in this channel:",
                this.Shared.RemindExecuters[ctx.User.Id]
                    .Select(t => (TaskId: t.Id, TaskInfo: (SendMessageTaskInfo)t.TaskInfo))
                    .Where(tup => tup.TaskInfo.ChannelId == ctx.Channel.Id)
                    .OrderBy(tup => tup.TaskInfo.ExecutionTime),
                tup => {
                    (int id, SendMessageTaskInfo tinfo) = tup;
                    if (tinfo.IsRepeating)
                        return $"ID: {Formatter.Bold(id.ToString())} (repeating every {tinfo.RepeatingInterval.ToString()}):{Formatter.BlockCode(tinfo.Message)}";
                    else
                        return $"ID: {Formatter.Bold(id.ToString())} ({tinfo.ExecutionTime.ToUtcTimestamp()}):{Formatter.BlockCode(tinfo.Message)}";
                },
                this.ModuleColor,
                1
            );
        }
        #endregion
        
        #region COMMAND_REMINDERS_REPEAT
        [Command("repeat"), Priority(2)]
        [Description("Schedule a new repeating reminder. You can also specify a channel where to send the reminder.")]
        [Aliases("newrep", "+r", "ar", "+=r", "<r", "<<r")]
        [UsageExamples("!remind repeat 1h Drink water!")]
        public Task RepeatAsync(CommandContext ctx,
                               [Description("Repeat timespan.")] TimeSpan timespan,
                               [Description("Channel to send message to.")] DiscordChannel channel,
                               [RemainingText, Description("What to send?")] string message)
            => this.AddReminderAsync(ctx, timespan, channel, message, true);

        [Command("repeat"), Priority(1)]
        public Task RepeatAsync(CommandContext ctx,
                            [Description("Channel to send message to.")] DiscordChannel channel,
                            [Description("Repeat timespan.")] TimeSpan timespan,
                            [RemainingText, Description("What to send?")] string message)
            => this.AddReminderAsync(ctx, timespan, channel, message, true);

        [Command("repeat"), Priority(0)]
        public Task RepeatAsync(CommandContext ctx,
                            [Description("Repeat timespan.")] TimeSpan timespan,
                            [RemainingText, Description("What to send?")] string message)
            => this.AddReminderAsync(ctx, timespan, null, message, true);
        #endregion


        #region HELPER_FUNCTIONS
        private async Task AddReminderAsync(CommandContext ctx, TimeSpan timespan, DiscordChannel channel, 
                                            string message, bool repeat = false)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new InvalidCommandUsageException("Missing time or repeat string.");

            if (message.Length > 250)
                throw new InvalidCommandUsageException("Message must be shorter than 250 characters.");

            if (timespan.TotalMinutes < 1 || timespan.TotalDays > 31)
                throw new InvalidCommandUsageException("Time span cannot be less than 1 minute or greater than 31 days.");
            
            if (this.Shared.RemindExecuters.ContainsKey(ctx.User.Id) && this.Shared.RemindExecuters[ctx.User.Id].Count >= 20)
                throw new CommandFailedException("You cannot have more than 20 reminders scheduled!");

            DateTimeOffset when = DateTimeOffset.Now + timespan;

            var task = new SendMessageTaskInfo(channel?.Id ?? 0, ctx.User.Id, message, when, repeat, timespan);
            await SavedTaskExecutor.ScheduleAsync(this.Shared, this.Database, ctx.Client, task);

            if (repeat) 
                await this.InformAsync(ctx, StaticDiscordEmoji.AlarmClock, $"I will repeatedly remind {channel?.Mention ?? "you"} every {Formatter.Bold(timespan.Humanize(5))} to:\n\n{message}", important: false);
            else
                await this.InformAsync(ctx, StaticDiscordEmoji.AlarmClock, $"I will remind {channel?.Mention ?? "you"} in {Formatter.Bold(timespan.Humanize(5))} ({when.ToUtcTimestamp()}) to:\n\n{message}", important: false);
        }
        #endregion
    }
}
