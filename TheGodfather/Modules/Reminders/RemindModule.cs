#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Humanizer;
using Humanizer.Localisation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Reminders
{
    [Group("remind"), Module(ModuleType.Reminders), NotBlocked]
    [Description("Manage reminders.")]
    [Aliases("reminders", "reminder", "todo", "todolist", "note")]
    [UsageExamples("!remind 1h Drink water!")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public partial class RemindModule : TheGodfatherModule
    {

        public RemindModule(SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.LightGray;
        }
  

        [GroupCommand, Priority(3)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Time span until reminder.")] TimeSpan timespan,
                                     [Description("Channel to send message to.")] DiscordChannel channel,
                                     [RemainingText, Description("What to send?")] string message)
            => this.AddReminderAsync(ctx, timespan, channel, message);

        [GroupCommand, Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Channel to send message to.")] DiscordChannel channel,
                                     [Description("Time span until reminder.")] TimeSpan timespan,
                                     [RemainingText, Description("What to send?")] string message)
            => this.AddReminderAsync(ctx, timespan, channel, message);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Time span until reminder.")] TimeSpan timespan,
                                     [RemainingText, Description("What to send?")] string message)
            => this.AddReminderAsync(ctx, timespan, null, message);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);



        #region COMMAND_REMIND_CLEAR
        [Command("deleteall"), UsesInteractivity]
        [Description("Delete all your reminders. You can also specify a channel for which to remove reminders.")]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da")]
        [UsageExamples("!remind clear")]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Channel for which to remove reminders.")] DiscordChannel channel = null)
        {
            if (!(channel is null) && channel.Type != ChannelType.Text)
                throw new InvalidCommandUsageException("You must specify a text channel.");

            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to remove all your reminders" + (channel is null ? "?" : $"in {channel.Mention}?")))
                return;

            List<DatabaseReminder> reminders;
            using (DatabaseContext db = this.Database.CreateContext()) {
                if (channel is null)
                    reminders = await db.Reminders.Where(r => r.UserId == ctx.User.Id).ToListAsync();
                else
                    reminders = await db.Reminders.Where(r => r.UserId == ctx.User.Id && r.ChannelId == channel.Id).ToListAsync();
            }

            await Task.WhenAll(reminders.Select(r => SavedTaskExecutor.UnscheduleAsync(this.Shared, ctx.User.Id, r.Id)));
            await this.InformAsync(ctx, "Successfully removed the specified reminders.", important: false);
        }
        #endregion

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
                if (!texecs.TryGetValue(id, out _)) {
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
            if (!this.Shared.RemindExecuters.TryGetValue(ctx.User.Id, out var texecs) || !texecs.Values.Any(t => ((SendMessageTaskInfo)t.TaskInfo).InitiatorId == ctx.User.Id))
                throw new CommandFailedException("You haven't issued any reminders.");

            return ctx.SendCollectionInPagesAsync(
                "Your reminders:",
                texecs.Values
                    .Select(t => (TaskId: t.Id, TaskInfo: (SendMessageTaskInfo)t.TaskInfo))
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

            if (channel is null && await ctx.Client.CreateDmChannelAsync(ctx.User.Id) is null)
                throw new CommandFailedException("I cannot send DMs to you, please enable it so that I can remind you.");

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
    }
}
