#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Humanizer;
using Humanizer.Localisation;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("remind")]
    [Description("Manage reminders.")]
    [Aliases("reminders", "reminder", "todo", "todolist", "note")]
    [UsageExamples("!remind 1h Drink water!")]
    [Cooldown(3, 5, CooldownBucketType.Channel), NotBlocked]
    public partial class RemindModule : TheGodfatherModule
    {

        public RemindModule(SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.LightGray;
        }
  

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);

        

        #region COMMAND_REMIND_DELETE
        [Command("delete")]
        [Description("Unschedules reminders.")]
        [Aliases("-", "remove", "rm", "del", "-=", ">", ">>", "unschedule")]
        [UsageExamples("!remind delete 1")]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Reminder ID.")] params int[] ids)
        {
            if (ids is null || !ids.Any())
                throw new InvalidCommandUsageException("Missing IDs of reminders to remove.");

            if (!this.Shared.RemindExecuters.TryGetValue(ctx.User.Id, out var texecs))
                throw new CommandFailedException("You have no reminders scheduled.");

            var eb = new StringBuilder();
            foreach (int id in ids) {
                if (!texecs.Any(texec => texec.Id == id)) {
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

        #region COMMAND_REMIND_LIST
        [Command("list")]
        [Description("Lists your reminders.")]
        [Aliases("ls")]
        [UsageExamples("!remind list")]
        public Task ListAsync(CommandContext ctx)
        {
            if (!this.Shared.RemindExecuters.TryGetValue(ctx.User.Id, out var texecs) || !texecs.Any(t => ((SendMessageTaskInfo)t.TaskInfo).InitiatorId == ctx.User.Id))
                throw new CommandFailedException("You haven't issued any reminders.");

            return ctx.SendCollectionInPagesAsync(
                "Your reminders:",
                texecs.Select(t => (TaskId: t.Id, TaskInfo: (SendMessageTaskInfo)t.TaskInfo))
                      .OrderBy(tup => tup.TaskInfo.ExecutionTime),
                tup => {
                    (int id, SendMessageTaskInfo tinfo) = tup;
                    if (tinfo.IsRepeating)
                        return $"ID: {Formatter.Bold(id.ToString())} (repeating every {tinfo.RepeatingInterval.Humanize()}):{Formatter.BlockCode(tinfo.Message)}";
                    else
                        return $"ID: {Formatter.Bold(id.ToString())} ({tinfo.ExecutionTime.ToUtcTimestamp()}):{Formatter.BlockCode(tinfo.Message)}";
                },
                this.ModuleColor,
                1
            );
        }
        #endregion

        #region COMMAND_REMIND_REPEAT
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

            if (timespan < TimeSpan.Zero || timespan.TotalMinutes < 1 || timespan.TotalDays > 31)
                throw new InvalidCommandUsageException("Time span cannot be less than 1 minute or greater than 31 days.");

            bool privileged;
            using (DatabaseContext db = this.Database.CreateContext())
                privileged = db.PrivilegedUsers.Any(u => u.UserId == ctx.User.Id);

            if (ctx.User.Id != ctx.Client.CurrentApplication?.Owner.Id && !privileged) {
                if (this.Shared.RemindExecuters.TryGetValue(ctx.User.Id, out var texecs) && texecs.Count >= 20)
                    throw new CommandFailedException("You cannot have more than 20 reminders scheduled!");
            }

            DateTimeOffset when = DateTimeOffset.Now + timespan;

            var task = new SendMessageTaskInfo(channel?.Id ?? 0, ctx.User.Id, message, when, repeat, timespan);
            await SavedTaskExecutor.ScheduleAsync(this.Shared, this.Database, ctx.Client, task);

            if (repeat)
                await this.InformAsync(ctx, StaticDiscordEmoji.AlarmClock, $"I will repeatedly remind {channel?.Mention ?? "you"} every {Formatter.Bold(timespan.Humanize(4, minUnit: TimeUnit.Second))} to:\n\n{message}", important: false);
            else
                await this.InformAsync(ctx, StaticDiscordEmoji.AlarmClock, $"I will remind {channel?.Mention ?? "you"} in {Formatter.Bold(timespan.Humanize(4, minUnit: TimeUnit.Second))} ({when.ToUtcTimestamp()}) to:\n\n{message}", important: false);
        }
        #endregion



        [Group("in")]
        [Description("Send a reminder after specific time span.")]
        [UsageExamples("!remind in 3h Drink water!",
                       "!remind in 3h5m Drink water!")]
        public class RemindInModule : RemindModule
        {

            public RemindInModule(SharedData shared, DatabaseContextBuilder db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.NotQuiteBlack;
            }


            [GroupCommand, Priority(2)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Time span until reminder.")] TimeSpan timespan,
                                         [Description("Channel to send message to.")] DiscordChannel channel,
                                         [RemainingText, Description("What to send?")] string message)
                => this.AddReminderAsync(ctx, timespan, channel, message);

            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Channel to send message to.")] DiscordChannel channel,
                                         [Description("Time span until reminder.")] TimeSpan timespan,
                                         [RemainingText, Description("What to send?")] string message)
                => this.AddReminderAsync(ctx, timespan, channel, message);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Time span until reminder.")] TimeSpan timespan,
                                         [RemainingText, Description("What to send?")] string message)
                => this.AddReminderAsync(ctx, timespan, null, message);
        }

        [Group("at")]
        [Description("Send a reminder at a specific point in time (given by date and time string).")]
        [UsageExamples("!remind at 17:20 Drink water!",
                       "!remind at 03.15.2019 Drink water!",
                       "!remind at \"03.15.2019 17:20\" Drink water!")]
        public class RemindAtModule : RemindModule
        {

            public RemindAtModule(SharedData shared, DatabaseContextBuilder db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.NotQuiteBlack;
            }


            [GroupCommand, Priority(2)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Date and/or time.")] DateTimeOffset when,
                                         [Description("Channel to send message to.")] DiscordChannel channel,
                                         [RemainingText, Description("What to send?")] string message)
                => this.AddReminderAsync(ctx, when - DateTimeOffset.Now, channel, message);

            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Channel to send message to.")] DiscordChannel channel,
                                         [Description("Date and/or time.")] DateTimeOffset when,
                                         [RemainingText, Description("What to send?")] string message)
                => this.AddReminderAsync(ctx, when - DateTimeOffset.Now, channel, message);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Date and/or time.")] DateTimeOffset when,
                                         [RemainingText, Description("What to send?")] string message)
                => this.AddReminderAsync(ctx, when - DateTimeOffset.Now, null, message);
        }
    }
}
