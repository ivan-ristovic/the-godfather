#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Humanizer.Localisation;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Reminders
{
    [Group("remind"), Module(ModuleType.Reminders), NotBlocked]
    [Description("Manage reminders.")]
    [Aliases("reminders", "reminder", "todo", "todolist", "note")]

    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public partial class RemindModule : TheGodfatherServiceModule<SchedulingService>
    {

        public RemindModule(SchedulingService service, DbContextBuilder db)
            : base(service, db)
        {

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
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Channel for which to list the reminders.")] DiscordChannel channel = null)
            => channel is null ? this.ListAsync(ctx) : this.ListAsync(ctx, channel);



        #region COMMAND_REMIND_CLEAR
        [Command("deleteall"), UsesInteractivity]
        [Description("Delete all your reminders. You can also specify a channel for which to remove reminders.")]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da")]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Channel for which to remove reminders.")] DiscordChannel channel = null)
        {
            if (!(channel is null) && channel.Type != ChannelType.Text)
                throw new InvalidCommandUsageException("You must specify a text channel.");

            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to remove all your reminders" + (channel is null ? "?" : $"in {channel.Mention}?")))
                return;

            await this.Service.UnscheduleRemindersForUserAsync(ctx.User.Id);
            await this.InformAsync(ctx, "Successfully removed the specified reminders.", important: false);
        }
        #endregion

        #region COMMAND_REMIND_DELETE
        [Command("delete")]
        [Description("Unschedules reminders.")]
        [Aliases("-", "remove", "rm", "del", "-=", ">", ">>", "unschedule")]

        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Reminder ID.")] params int[] ids)
        {
            if (ids is null || !ids.Any())
                throw new InvalidCommandUsageException("Missing IDs of reminders to remove.");

            IReadOnlyList<(int Id, Reminder TaskInfo)> reminders = this.Service.GetRemindTasksForUser(ctx.User.Id);
            if (!reminders.Any())
                throw new CommandFailedException("You have no reminders scheduled.");

            await Task.WhenAll(reminders.Select(r => this.Service.UnscheduleAsync(r.Id, r.TaskInfo)));
            await this.InformAsync(ctx, "Successfully removed all specified reminders.", important: false);
        }
        #endregion

        #region COMMAND_REMIND_LIST
        [Command("list"), Priority(1)]
        [Description("Lists your reminders.")]
        [Aliases("ls")]
        public Task ListAsync(CommandContext ctx,
                             [Description("Channel for which to list the reminders.")] DiscordChannel channel)
        {
            if (channel.Type != ChannelType.Text)
                throw new InvalidCommandUsageException("Reminders can only be issued for text channels.");

            IReadOnlyList<(int Id, Reminder TaskInfo)> reminders = this.Service.GetRemindTasksForUser(ctx.User.Id);
            if (!reminders.Any(r => r.TaskInfo.ChannelId == channel.Id))
                throw new CommandFailedException("No reminders are scheduled for that channel.");

            return ctx.SendCollectionInPagesAsync(
                $"Your reminders for channel {channel.Name}:",
                reminders
                    .Where(r => r.TaskInfo.ChannelId == channel.Id)
                    .OrderBy(r => r.TaskInfo.ExecutionTime),
                r => {
                    if (r.TaskInfo.IsRepeating) {
                        return $"ID: {Formatter.Bold(r.Id.ToString())} (repeating every {r.TaskInfo.RepeatInterval.Humanize()}):{Formatter.BlockCode(r.TaskInfo.Message)}";
                    } else {
                        return r.TaskInfo.TimeUntilExecution > TimeSpan.FromDays(1)
                            ? $"ID: {Formatter.Bold(r.Id.ToString())} ({r.TaskInfo.ExecutionTime.ToUtcTimestamp()}):{Formatter.BlockCode(r.TaskInfo.Message)}"
                            : $"ID: {Formatter.Bold(r.Id.ToString())} (in {r.TaskInfo.TimeUntilExecution.Humanize(precision: 3, minUnit: TimeUnit.Minute)}):{Formatter.BlockCode(r.TaskInfo.Message)}";
                    }
                },
                this.ModuleColor,
                1
            );
        }

        [Command("list"), Priority(0)]
        public Task ListAsync(CommandContext ctx)
        {
            IReadOnlyList<(int Id, Reminder TaskInfo)> reminders = this.Service.GetRemindTasksForUser(ctx.User.Id);
            if (!reminders.Any())
                throw new CommandFailedException("No reminders are scheduled for that channel.");

            return ctx.SendCollectionInPagesAsync(
                "Your reminders:",
                reminders.OrderBy(r => r.TaskInfo.ExecutionTime),
                tup => {
                    (int id, Reminder tinfo) = tup;
                    if (tinfo.IsRepeating) {
                        return $"ID: {Formatter.Bold(id.ToString())} (repeating every {tinfo.RepeatInterval.Humanize()}):{Formatter.BlockCode(tinfo.Message)}";
                    } else {
                        if (tinfo.TimeUntilExecution > TimeSpan.FromDays(1))
                            return $"ID: {Formatter.Bold(id.ToString())} ({tinfo.ExecutionTime.ToUtcTimestamp()}):{Formatter.BlockCode(tinfo.Message)}";
                        else
                            return $"ID: {Formatter.Bold(id.ToString())} (in {tinfo.TimeUntilExecution.Humanize(precision: 3, minUnit: TimeUnit.Minute)}):{Formatter.BlockCode(tinfo.Message)}";
                    }
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
            using (TheGodfatherDbContext db = this.Database.CreateContext())
                privileged = db.PrivilegedUsers.Any(u => u.UserId == ctx.User.Id);

            if (!ctx.Client.CurrentApplication.Owners.Any(o => ctx.User.Id == o.Id) && !privileged) {
                IReadOnlyList<(int Id, Reminder TaskInfo)> reminders = this.Service.GetRemindTasksForUser(ctx.User.Id);
                if (reminders.Count >= 20)
                    throw new CommandFailedException("You cannot have more than 20 reminders scheduled!");
            }

            DateTimeOffset when = DateTimeOffset.Now + timespan;

            var tinfo = new Reminder {
                ChannelId = channel?.Id ?? 0,
                ExecutionTime = when,
                IsRepeating = repeat,
                RepeatIntervalDb = repeat ? timespan : (TimeSpan?)null,
                UserId = ctx.User.Id,
            };
            await this.Service.ScheduleAsync(tinfo);

            if (repeat)
                await this.InformAsync(ctx, Emojis.AlarmClock, $"I will repeatedly remind {channel?.Mention ?? "you"} every {Formatter.Bold(timespan.Humanize(4, minUnit: TimeUnit.Second))} to:\n\n{message}", important: false);
            else
                await this.InformAsync(ctx, Emojis.AlarmClock, $"I will remind {channel?.Mention ?? "you"} in {Formatter.Bold(timespan.Humanize(4, minUnit: TimeUnit.Second))} ({when.ToUtcTimestamp()}) to:\n\n{message}", important: false);
        }
        #endregion
    }
}
