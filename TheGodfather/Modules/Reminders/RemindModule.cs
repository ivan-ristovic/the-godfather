using System.Globalization;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Owner.Services;

namespace TheGodfather.Modules.Reminders;

[Group("remind")][Module(ModuleType.Reminders)][NotBlocked]
[Aliases("reminders", "reminder", "todo", "todolist", "note", "td")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public partial class RemindModule : TheGodfatherServiceModule<SchedulingService>
{
    #region remind
    [GroupCommand][Priority(4)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_remind_t)] TimeSpan timespan,
        [Description(TranslationKey.desc_remind_chn)] DiscordChannel channel,
        [RemainingText][Description(TranslationKey.desc_remind_text)] string message)
        => this.AddReminderAsync(ctx, timespan, channel, message);

    [GroupCommand][Priority(3)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_remind_chn)] DiscordChannel channel,
        [Description(TranslationKey.desc_remind_t)] TimeSpan timespan,
        [RemainingText][Description(TranslationKey.desc_remind_text)] string message)
        => this.AddReminderAsync(ctx, timespan, channel, message);

    [GroupCommand][Priority(2)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_remind_t)] TimeSpan timespan,
        [RemainingText][Description(TranslationKey.desc_remind_text)] string message)
        => this.AddReminderAsync(ctx, timespan, null, message);

    [GroupCommand][Priority(1)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_chn_list)] DiscordChannel channel)
        => this.ListAsync(ctx, channel);

    [GroupCommand][Priority(0)]
    public Task ExecuteGroupAsync(CommandContext ctx)
        => this.ListAsync(ctx);
    #endregion

    #region remind deleteall
    [Command("deleteall")][Priority(1)][UsesInteractivity]
    [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    public async Task DeleteAllAsync(CommandContext ctx,
        [Description(TranslationKey.desc_remind_chn_rem)] DiscordChannel channel)
    {
        this.ThrowIfDM(ctx, channel);

        if (!ctx.Channel.PermissionsFor(ctx.Member).HasPermission(Permissions.ManageGuild))
            throw new ChecksFailedException(ctx.Command!, ctx, new[] { new RequireUserPermissionsAttribute(Permissions.ManageGuild) });

        if (channel.Type != ChannelType.Text)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_chn_type_text);

        if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_remind_rem_all_chn(channel.Mention)))
            return;

        await this.Service.UnscheduleRemindersForChannelAsync(ctx.Channel.Id);
        await ctx.InfoAsync(this.ModuleColor);
    }

    [Command("deleteall")][Priority(0)]
    public async Task DeleteAllAsync(CommandContext ctx)
    {
        if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_remind_rem_all))
            return;

        await this.Service.UnscheduleRemindersForUserAsync(ctx.User.Id);
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region remind delete
    [Command("delete")][Priority(1)]
    [Aliases("-", "remove", "rm", "del", "-=", ">", ">>", "unschedule")]
    public async Task DeleteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_remind_chn_rem)] DiscordChannel channel,
        [Description(TranslationKey.desc_ids)] params int[] ids)
    {
        if (ids is null || !ids.Any())
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_ids_none);

        if (channel.Guild != ctx.Guild || (ctx.Member is not null && !ctx.Member.PermissionsIn(channel).HasPermission(Permissions.Administrator)))
            throw new CommandFailedException(ctx, TranslationKey.cmd_chk_perms_usr(Permissions.Administrator));

        IReadOnlyList<Reminder> reminders = await this.Service.GetRemindTasksForChannelAsync(channel.Id);
        if (!reminders.Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_remind_none);

        await Task.WhenAll(reminders.Where(r => r.ChannelId == channel.Id).Select(r => this.Service.UnscheduleAsync(r, true)));
        await ctx.InfoAsync(this.ModuleColor);
    }

    [Command("delete")][Priority(0)]
    public async Task DeleteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_ids)] params int[] ids)
    {
        if (ids is null || !ids.Any())
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_ids_none);

        IReadOnlyList<Reminder> reminders = await this.Service.GetRemindTasksForUserAsync(ctx.User.Id);
        if (!reminders.Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_remind_none);

        await Task.WhenAll(reminders.Where(r => r.ChannelId == 0).Select(r => this.Service.UnscheduleAsync(r, true)));
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region remind list
    [Command("list")][Priority(1)]
    [Aliases("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync(CommandContext ctx,
        [Description(TranslationKey.desc_chn_list)] DiscordChannel channel)
    {
        this.ThrowIfDM(ctx, channel);

        if (channel.Type != ChannelType.Text)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_chn_type_text);

        IReadOnlyList<Reminder> reminders = await this.Service.GetRemindTasksForUserAsync(ctx.User.Id);
        IEnumerable<Reminder> orderedReminders = reminders
                .Where(r => r.ChannelId == channel.Id)
                .OrderBy(r => r.TimeUntilExecution).ThenBy(r => r.Id)
            ;

        await this.PaginateRemindersAsync(ctx, orderedReminders, channel);
    }

    [Command("list")][Priority(0)]
    public async Task ListAsync(CommandContext ctx)
    {
        IEnumerable<Reminder> reminders = await this.Service.GetRemindTasksForUserAsync(ctx.User.Id);
        IEnumerable<Reminder> orderedReminders = reminders
                .Where(r => r.ChannelId == 0)
                .OrderBy(r => r.TimeUntilExecution).ThenBy(r => r.Id)
            ;

        await this.PaginateRemindersAsync(ctx, orderedReminders);
    }
    #endregion

    #region remind repeat
    [Command("repeat")][Priority(2)]
    [Aliases("newrep", "+r", "ar", "+=r", "<r", "<<r")]
    public Task RepeatAsync(CommandContext ctx,
        [Description(TranslationKey.desc_remind_dt)] TimeSpan timespan,
        [Description(TranslationKey.desc_remind_chn)] DiscordChannel channel,
        [RemainingText][Description(TranslationKey.desc_remind_text)] string message)
        => this.AddReminderAsync(ctx, timespan, channel, message, true);

    [Command("repeat")][Priority(1)]
    public Task RepeatAsync(CommandContext ctx,
        [Description(TranslationKey.desc_remind_chn)] DiscordChannel channel,
        [Description(TranslationKey.desc_remind_dt)] TimeSpan timespan,
        [RemainingText][Description(TranslationKey.desc_remind_text)] string message)
        => this.AddReminderAsync(ctx, timespan, channel, message, true);

    [Command("repeat")][Priority(0)]
    public Task RepeatAsync(CommandContext ctx,
        [Description(TranslationKey.desc_remind_dt)] TimeSpan timespan,
        [RemainingText][Description(TranslationKey.desc_remind_text)] string message)
        => this.AddReminderAsync(ctx, timespan, null, message, true);
    #endregion


    #region internals
    private async Task AddReminderAsync(CommandContext ctx, TimeSpan timespan, DiscordChannel? channel,
        string message, bool repeat = false)
    {
        this.ThrowIfDM(ctx, channel);

        if (channel is not null) {
            if (channel.Type != ChannelType.Text)
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_chn_type_text);
            if (!channel.PermissionsFor(ctx.Member).HasFlag(Permissions.SendMessages))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_remind_perms(channel.Mention));
            if (!channel.PermissionsFor(ctx.Guild.CurrentMember).HasFlag(Permissions.SendMessages))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_remind_permsb(channel.Mention));
        }

        if (string.IsNullOrWhiteSpace(message) || message.Length > Reminder.MessageLimit)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_remind_msg(Reminder.MessageLimit));

        bool bypassLimits = ctx.Client.IsOwnedBy(ctx.User) || await ctx.Services.GetRequiredService<PrivilegedUserService>().ContainsAsync(ctx.User.Id);
        if (timespan < TimeSpan.Zero || (!bypassLimits && (timespan.TotalMinutes < 1 || timespan.TotalDays > 31)))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_timespan_m_d(1, 31));

        if (channel is null && await ctx.Client.CreateDmChannelAsync(ctx.User.Id) is null)
            throw new CommandFailedException(ctx, TranslationKey.err_dm_fail);

        if (!bypassLimits) {
            IReadOnlyList<Reminder> reminders = await this.Service.GetRemindTasksForUserAsync(ctx.User.Id);
            if (reminders.Count >= 20)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_remind_max(20));
        }

        DateTimeOffset when = DateTimeOffset.Now + timespan;

        var tinfo = new Reminder {
            ChannelId = channel?.Id ?? 0,
            ExecutionTime = when,
            IsRepeating = repeat,
            RepeatIntervalDb = repeat ? timespan : null,
            UserId = ctx.User.Id,
            Message = message,
        };
        await this.Service.ScheduleAsync(tinfo);

        string rel = timespan.Humanize(4, this.Localization.GetGuildCulture(ctx.Guild?.Id), minUnit: TimeUnit.Minute);
        if (repeat) {
            if (channel is not null)
                await ctx.InfoAsync(this.ModuleColor, Emojis.AlarmClock, TranslationKey.fmt_remind_rep_c(channel.Mention, rel, message));
            else
                await ctx.InfoAsync(this.ModuleColor, Emojis.AlarmClock, TranslationKey.fmt_remind_rep(rel, message));
        } else {
            string abs = this.Localization.GetLocalizedTimeString(ctx.Guild?.Id, when);
            if (channel is not null)
                await ctx.InfoAsync(this.ModuleColor, Emojis.AlarmClock, TranslationKey.fmt_remind_c(channel.Mention, rel, abs, message));
            else
                await ctx.InfoAsync(this.ModuleColor, Emojis.AlarmClock, TranslationKey.fmt_remind(rel, abs, message));
        }
    }

    private void ThrowIfDM(CommandContext ctx, DiscordChannel? chn)
    {
        if (chn is not null && ctx.Guild is null)
            throw new ChecksFailedException(ctx.Command!, ctx, new[] { new RequireGuildAttribute() });
    }

    private Task PaginateRemindersAsync(CommandContext ctx, IEnumerable<Reminder> reminders, DiscordChannel? chn = null)
    {
        reminders = reminders.ToList();
        if (!reminders.Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_remind_none);

        CultureInfo culture = this.Localization.GetGuildCulture(ctx.Guild?.Id);
        return ctx.PaginateAsync(reminders, (emb, r) => {
            emb.WithLocalizedTitle(chn is null ? TranslationKey.str_remind_chn : TranslationKey.fmt_remind_chn(chn.Name));
            emb.WithDescription(r.Message);
            emb.AddLocalizedField(TranslationKey.str_id, r.Id, true);
            if (r.IsRepeating)
                emb.AddLocalizedField(TranslationKey.str_repeating, r.RepeatInterval.Humanize(culture: culture), true);

            if (r.TimeUntilExecution < TimeSpan.FromDays(1))
                emb.AddLocalizedField(TranslationKey.str_executes_in, r.TimeUntilExecution.Humanize(3, culture, minUnit: TimeUnit.Second), true);
            else
                emb.AddLocalizedTimestampField(TranslationKey.str_exec_time, r.ExecutionTime, true);

            return emb;
        }, this.ModuleColor);
    }
    #endregion
}