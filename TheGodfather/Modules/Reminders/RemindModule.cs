using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Owner.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules.Reminders
{
    [Group("remind"), Module(ModuleType.Reminders), NotBlocked]
    [Aliases("reminders", "reminder", "todo", "todolist", "note")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public partial class RemindModule : TheGodfatherServiceModule<SchedulingService>
    {
        #region remind
        [GroupCommand, Priority(4)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-remind-t")] TimeSpan timespan,
                                     [Description("desc-remind-chn")] DiscordChannel channel,
                                     [RemainingText, Description("desc-remind-text")] string message)
            => this.AddReminderAsync(ctx, timespan, channel, message);

        [GroupCommand, Priority(3)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-remind-chn")] DiscordChannel channel,
                                     [Description("desc-remind-t")] TimeSpan timespan,
                                     [RemainingText, Description("desc-remind-text")] string message)
            => this.AddReminderAsync(ctx, timespan, channel, message);

        [GroupCommand, Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-remind-t")] TimeSpan timespan,
                                     [RemainingText, Description("desc-remind-text")] string message)
            => this.AddReminderAsync(ctx, timespan, null, message);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-chn-list")] DiscordChannel channel)
            => channel is null ? this.ListAsync(ctx) : this.ListAsync(ctx, channel);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);
        #endregion

        #region remind deleteall
        [Command("deleteall"), Priority(1), UsesInteractivity]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
        public async Task DeleteAllAsync(CommandContext ctx,
                                        [Description("desc-remind-chn-rem")] DiscordChannel channel)
        {
            this.ThrowIfDM(ctx, channel);

            if (!ctx.Channel.PermissionsFor(ctx.Member).HasPermission(Permissions.ManageGuild))
                throw new ChecksFailedException(ctx.Command, ctx, new[] { new RequireUserPermissionsAttribute(Permissions.ManageGuild) });

            if (channel.Type != ChannelType.Text)
                throw new InvalidCommandUsageException(ctx, "cmd-err-chn-text");

            if (!await ctx.WaitForBoolReplyAsync("q-remind-rem-all-chn", args: channel.Mention))
                return;

            await this.Service.UnscheduleRemindersForChannelAsync(ctx.Channel.Id);
            await ctx.InfoAsync(this.ModuleColor);
        }

        [Command("deleteall"), Priority(0)]
        public async Task DeleteAllAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("q-remind-rem-all"))
                return;

            await this.Service.UnscheduleRemindersForUserAsync(ctx.User.Id);
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region remind delete
        [Command("delete")]
        [Aliases("-", "remove", "rm", "del", "-=", ">", ">>", "unschedule")]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("desc-ids")] params int[] ids)
        {
            if (ids is null || !ids.Any())
                throw new InvalidCommandUsageException(ctx, "cmd-err-ids-none");

            IReadOnlyList<Reminder> reminders = this.Service.GetRemindTasksForUser(ctx.User.Id);
            if (!reminders.Any())
                throw new CommandFailedException(ctx, "cmd-err-remind-none");

            await Task.WhenAll(reminders.Where(r => r.ChannelId == 0).Select(r => this.Service.UnscheduleAsync(r)));
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region remind list
        [Command("list"), Priority(1)]
        [Aliases("print", "show", "view", "ls", "l", "p")]
        public Task ListAsync(CommandContext ctx,
                             [Description("desc-chn-list")] DiscordChannel channel)
        {
            this.ThrowIfDM(ctx, channel);

            if (channel.Type != ChannelType.Text)
                throw new InvalidCommandUsageException(ctx, "cmd-err-chn-text");

            IEnumerable<Reminder> reminders = this.Service.GetRemindTasksForUser(ctx.User.Id)
                .Where(r => r.ChannelId == channel.Id)
                .OrderBy(r => r.TimeUntilExecution).ThenBy(r => r.Id)
                ;

            return this.PaginateRemindersAsync(ctx, reminders, channel);
        }

        [Command("list"), Priority(0)]
        public Task ListAsync(CommandContext ctx)
        {
            IEnumerable<Reminder> reminders = this.Service.GetRemindTasksForUser(ctx.User.Id)
                .Where(r => r.ChannelId == 0)
                .OrderBy(r => r.TimeUntilExecution).ThenBy(r => r.Id)
                ;

            return this.PaginateRemindersAsync(ctx, reminders);
        }
        #endregion

        #region remind repeat
        [Command("repeat"), Priority(2)]
        [Aliases("newrep", "+r", "ar", "+=r", "<r", "<<r")]
        public Task RepeatAsync(CommandContext ctx,
                               [Description("desc-remind-dt")] TimeSpan timespan,
                               [Description("desc-remind-chn")] DiscordChannel channel,
                               [RemainingText, Description("desc-remind-text")] string message)
            => this.AddReminderAsync(ctx, timespan, channel, message, true);

        [Command("repeat"), Priority(1)]
        public Task RepeatAsync(CommandContext ctx,
                            [Description("desc-remind-chn")] DiscordChannel channel,
                            [Description("desc-remind-dt")] TimeSpan timespan,
                            [RemainingText, Description("desc-remind-text")] string message)
            => this.AddReminderAsync(ctx, timespan, channel, message, true);

        [Command("repeat"), Priority(0)]
        public Task RepeatAsync(CommandContext ctx,
                            [Description("desc-remind-dt")] TimeSpan timespan,
                            [RemainingText, Description("desc-remind-text")] string message)
            => this.AddReminderAsync(ctx, timespan, null, message, true);
        #endregion


        #region internals
        private async Task AddReminderAsync(CommandContext ctx, TimeSpan timespan, DiscordChannel? channel,
                                            string message, bool repeat = false)
        {
            this.ThrowIfDM(ctx, channel);

            if (channel is { }) {
                if (channel.Type != ChannelType.Text)
                    throw new InvalidCommandUsageException(ctx, "cmd-err-chn-text");
                if (!channel.PermissionsFor(ctx.Member).HasFlag(Permissions.SendMessages))
                    throw new CommandFailedException(ctx, "cmd-err-remind-perms", channel.Mention);
                if (!channel.PermissionsFor(ctx.Guild.CurrentMember).HasFlag(Permissions.SendMessages))
                    throw new CommandFailedException(ctx, "cmd-err-remind-permsb");
            }

            if (string.IsNullOrWhiteSpace(message) || message.Length > Reminder.MessageLimit)
                throw new InvalidCommandUsageException(ctx, "cmd-err-remind-msg", Reminder.MessageLimit);

            if (timespan < TimeSpan.Zero || timespan.TotalMinutes < 1 || timespan.TotalDays > 31)
                throw new InvalidCommandUsageException(ctx, "cmd-err-timespan-m-d", 1, 31);

            if (channel is null && await ctx.Client.CreateDmChannelAsync(ctx.User.Id) is null)
                throw new CommandFailedException(ctx, "err-dm-fail");

            bool priv = await ctx.Services.GetRequiredService<PrivilegedUserService>().ContainsAsync(ctx.User.Id);
            if (!priv && !ctx.Client.IsOwnedBy(ctx.User) && this.Service.GetRemindTasksForUser(ctx.User.Id).Count >= 20)
                throw new CommandFailedException(ctx, "cmd-err-remind-max", 20);

            DateTimeOffset when = DateTimeOffset.Now + timespan;

            var tinfo = new Reminder {
                ChannelId = channel?.Id ?? 0,
                ExecutionTime = when,
                IsRepeating = repeat,
                RepeatIntervalDb = repeat ? timespan : (TimeSpan?)null,
                UserId = ctx.User.Id,
                Message = message,
            };
            await this.Service.ScheduleAsync(tinfo);

            string rel = timespan.Humanize(4, this.Localization.GetGuildCulture(ctx.Guild?.Id), minUnit: TimeUnit.Minute);
            if (repeat) {
                if (channel is { })
                    await ctx.InfoAsync(this.ModuleColor, Emojis.AlarmClock, "fmt-remind-rep-c", channel.Mention, rel, message);
                else
                    await ctx.InfoAsync(this.ModuleColor, Emojis.AlarmClock, "fmt-remind-rep", rel, message);
            } else {
                string abs = this.Localization.GetLocalizedTimeString(ctx.Guild?.Id, when);
                if (channel is { })
                    await ctx.InfoAsync(this.ModuleColor, Emojis.AlarmClock, "fmt-remind-c", channel.Mention, rel, abs, message);
                else
                    await ctx.InfoAsync(this.ModuleColor, Emojis.AlarmClock, "fmt-remind", rel, abs, message);
            }
        }

        private void ThrowIfDM(CommandContext ctx, DiscordChannel? chn)
        {
            if (chn is { } && ctx.Guild is null)
                throw new ChecksFailedException(ctx.Command, ctx, new[] { new RequireGuildAttribute() });
        }

        private Task PaginateRemindersAsync(CommandContext ctx, IEnumerable<Reminder> reminders, DiscordChannel? chn = null)
        {
            if (!reminders.Any())
                throw new CommandFailedException(ctx, "cmd-err-remind-none");

            CultureInfo culture = this.Localization.GetGuildCulture(ctx.Guild?.Id);
            return ctx.PaginateAsync(reminders, (emb, r) => {
                if (chn is null)
                    emb.WithLocalizedTitle("str-remind-chn");
                else
                    emb.WithLocalizedTitle("fmt-remind-chn", chn.Name);

                emb.WithDescription(r.Message);
                emb.AddLocalizedTitleField("str-id", r.Id, inline: true);
                if (r.IsRepeating)
                    emb.AddLocalizedTitleField("str-repeating", r.RepeatInterval.Humanize(culture: culture), inline: true);

                if (r.TimeUntilExecution < TimeSpan.FromDays(1))
                    emb.AddLocalizedTitleField("str-executes-in", r.TimeUntilExecution.Humanize(3, culture: culture, minUnit: TimeUnit.Second), inline: true);
                else
                    emb.AddLocalizedTimestampField("str-exec-time", r.ExecutionTime, inline: true);

                return emb;
            }, this.ModuleColor);
        }
        #endregion
    }
}
