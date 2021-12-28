using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace TheGodfather.Modules.Reminders;

public partial class RemindModule
{
    [Group("before")]
    [Aliases("due", "b")]
    public class RemindBeforeModule : RemindModule
    {
        #region remind before
        [GroupCommand][Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
            [Description(TranslationKey.desc_remind_dt)] DateTimeOffset when,
            [Description(TranslationKey.desc_chn_list)] DiscordChannel? channel = null)
            => this.InternalListAsync(ctx, when - DateTimeOffset.UtcNow, channel);

        [GroupCommand][Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
            [Description(TranslationKey.desc_remind_dts)] TimeSpan relativeWhen,
            [Description(TranslationKey.desc_chn_list)] DiscordChannel? channel = null)
            => this.InternalListAsync(ctx, relativeWhen, channel);
        #endregion

        #region remind before tomorrow
        [Command("tomorrow")]
        [Aliases("tmrw", "t", "tomo")]
        public Task TomorrowAsync(CommandContext ctx,
            [Description(TranslationKey.desc_chn_list)] DiscordChannel? channel = null)
            => this.InternalListAsync(ctx, TimeSpan.FromDays(1), channel);
        #endregion

        #region remind before next
        [Group("next")]
        [Aliases("nxt", "n")]
        public sealed class RemindBeforeNextModule : RemindBeforeModule
        {
            #region remind before next
            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx,
                [Description(TranslationKey.desc_weekday)] DayOfWeek dayOfWeek,
                [Description(TranslationKey.desc_chn_list)] DiscordChannel? channel = null)
            {
                DayOfWeek currentDayOfWeek = this.Localization.GetLocalizedTime(ctx.Guild?.Id).DayOfWeek;
                return this.InternalListAsync(ctx!, currentDayOfWeek.Until(dayOfWeek), channel);
            }
            #endregion

            #region remind before next day
            [Command("day")]
            [Aliases("d")]
            public Task DayAsync(CommandContext ctx,
                [Description(TranslationKey.desc_chn_list)] DiscordChannel? channel = null)
                => this.InternalListAsync(ctx, TimeSpan.FromDays(1), channel);
            #endregion

            #region remind before next week
            [Command("week")]
            [Aliases("w")]
            public Task WeekAsync(CommandContext ctx,
                [Description(TranslationKey.desc_chn_list)] DiscordChannel? channel = null)
            {
                DayOfWeek currentDayOfWeek = this.Localization.GetLocalizedTime(ctx.Guild?.Id).DayOfWeek;
                DayOfWeek firstDayOfWeek = this.Localization.GetGuildCulture(ctx.Guild?.Id).DateTimeFormat.FirstDayOfWeek;
                return this.InternalListAsync(ctx!, currentDayOfWeek.Until(firstDayOfWeek), channel);
            }
            #endregion
        }
        #endregion


        #region internals
        private async Task InternalListAsync(CommandContext ctx, TimeSpan until, DiscordChannel? channel)
        {
            this.ThrowIfDM(ctx, channel);

            if (channel is { } && channel.Type != ChannelType.Text)
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_chn_type_text);

            IReadOnlyList<Reminder> reminders = await this.Service.GetRemindTasksForUserAsync(ctx.User.Id);

            IEnumerable<Reminder> filtered = reminders;
            if (channel is { })
                filtered = filtered.Where(r => r.ChannelId == channel.Id);

            filtered = filtered
                    .Where(r => r.TimeUntilExecution <= until)
                    .OrderBy(r => r.TimeUntilExecution).ThenBy(r => r.Id)
                ;

            await this.PaginateRemindersAsync(ctx, filtered, channel);
        }
        #endregion
    }
}