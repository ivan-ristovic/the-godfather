using Discord.Commands;
using TheGodfather.Attributes;

namespace TheGodfather.Modules.Reminders;

[Group("remind")]
[Alias("reminder", "reminders", "rem")]
[Cooldown(3, 5, CooldownBucketType.User)]
public sealed class RemindModule : TheGodfatherModule
{
    private readonly SchedulingService scheduler;


    public RemindModule(LocalizationService localization, SchedulingService scheduler)
        : base(localization, new Color(0x1ABC9C))
    {
        this.scheduler = scheduler;
    }


    [Command("")]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("in")]
    public Task InAsync(string duration, [Remainder] string message)
        => this.ScheduleAsync(duration, message, this.Context.Channel.Id);

    [Command("here in")]
    public Task HereInAsync(string duration, [Remainder] string message)
        => this.ScheduleAsync(duration, message, this.Context.Channel.Id);

    [Command("at")]
    public Task AtAsync(string when, [Remainder] string message)
        => this.ScheduleAtAsync(when, message, this.Context.Channel.Id);

    [Command("here at")]
    public Task HereAtAsync(string when, [Remainder] string message)
        => this.ScheduleAtAsync(when, message, this.Context.Channel.Id);

    [Command("dm in")]
    [Alias("direct in")]
    public Task DmInAsync(string duration, [Remainder] string message)
        => this.ScheduleAsync(duration, message, 0);

    [Command("dm at")]
    [Alias("direct at")]
    public Task DmAtAsync(string when, [Remainder] string message)
        => this.ScheduleAtAsync(when, message, 0);

    [Command("tomorrow")]
    [Alias("before tomorrow")]
    public Task TomorrowAsync([Remainder] string message)
        => this.ScheduleAtAsync(DateTimeOffset.Now.Date.AddDays(1).AddHours(9), message, this.Context.Channel.Id);

    [Command("day")]
    [Alias("before day")]
    public Task DayAsync([Remainder] string message)
        => this.ScheduleAtAsync(DateTimeOffset.Now.AddDays(1), message, this.Context.Channel.Id);

    [Command("week")]
    [Alias("before week")]
    public Task WeekAsync([Remainder] string message)
        => this.ScheduleAtAsync(DateTimeOffset.Now.AddDays(7), message, this.Context.Channel.Id);

    [Command("delete")]
    [Priority(1)]
    [Alias("remove", "rm", "del", "d")]
    public async Task DeleteAsync(int id)
    {
        Reminder? reminder = (await this.scheduler.GetRemindTasksForUserAsync(this.Context.User.Id)).FirstOrDefault(r => r.Id == id);
        if (reminder is null) {
            await this.FailAsync("Reminder not found.");
            return;
        }

        await this.scheduler.UnscheduleAsync(reminder, true);
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("delete")]
    [Priority(0)]
    public async Task DeleteAsync()
    {
        IReadOnlyList<Reminder> reminders = await this.scheduler.GetRemindTasksForUserAsync(this.Context.User.Id);
        if (!reminders.Any()) {
            await this.FailAsync("No reminders found.");
            return;
        }
        await this.scheduler.UnscheduleAsync(reminders.OrderBy(r => r.ExecutionTime).First(), true);
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("deleteall")]
    [Alias("removeall", "clear")]
    public async Task DeleteAllAsync()
    {
        if (!await this.ConfirmAsync(TranslationKey.q_remind_rem_all))
            return;

        await this.scheduler.UnscheduleRemindersForUserAsync(this.Context.User.Id);
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("list")]
    [Priority(1)]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public Task ListAsync()
        => this.ListUserAsync(this.Context.User);

    [Command("list")]
    [Priority(0)]
    public async Task ListUserAsync(IUser user)
    {
        IReadOnlyList<Reminder> reminders = await this.scheduler.GetRemindTasksForUserAsync(user.Id);
        if (!reminders.Any()) {
            await this.FailAsync("No reminders found.");
            return;
        }

        await this.RespondPagedAsync(
            TranslationKey.str_remind_chn,
            reminders.OrderBy(r => r.ExecutionTime),
            r => $"{InlineCode(r.Id)} | {this.Localization.GetLocalizedTimeString(this.Context.Guild?.Id, r.ExecutionTime)} | {r.Message}"
        );
    }

    [Command("repeat")]
    public async Task RepeatAsync(int id, string interval)
    {
        Reminder? reminder = (await this.scheduler.GetRemindTasksForUserAsync(this.Context.User.Id)).FirstOrDefault(r => r.Id == id);
        if (reminder is null || !TryParseDuration(interval, out TimeSpan repeat)) {
            await this.FailAsync("Reminder or interval not found.");
            return;
        }

        await this.scheduler.UnscheduleAsync(reminder, true);
        reminder.Id = 0;
        reminder.IsRepeating = true;
        reminder.RepeatIntervalDb = repeat;
        await this.scheduler.ScheduleAsync(reminder);
        await this.InfoAsync(this.ModuleColor);
    }


    private Task ScheduleAsync(string duration, string message, ulong channelId)
    {
        if (!TryParseDuration(duration, out TimeSpan span))
            return this.FailAsync("Invalid duration. Use a TimeSpan or a value like 30m, 2h, 7d.");
        return this.ScheduleAtAsync(DateTimeOffset.Now + span, message, channelId);
    }

    private Task ScheduleAtAsync(string when, string message, ulong channelId)
    {
        if (!DateTimeOffset.TryParse(when, this.Localization.GetGuildCulture(this.Context.Guild?.Id), DateTimeStyles.AssumeLocal, out DateTimeOffset parsed))
            return this.FailAsync("Invalid date/time.");
        return this.ScheduleAtAsync(parsed, message, channelId);
    }

    private async Task ScheduleAtAsync(DateTimeOffset when, string message, ulong channelId)
    {
        if (string.IsNullOrWhiteSpace(message) || message.Length > Reminder.MessageLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_remind_msg(Reminder.MessageLimit));

        if (when <= DateTimeOffset.Now) {
            await this.FailAsync("Reminder time must be in the future.");
            return;
        }

        await this.scheduler.ScheduleAsync(new Reminder {
            UserId = this.Context.User.Id,
            ChannelId = channelId,
            ExecutionTime = when,
            Message = message
        });

        await this.InfoAsync(this.ModuleColor, $"I will remind you at {this.Localization.GetLocalizedTimeString(this.Context.Guild?.Id, when)} to:\n\n{message}");
    }

    private static bool TryParseDuration(string value, out TimeSpan span)
    {
        if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out span))
            return span > TimeSpan.Zero;

        if (value.Length < 2 || !double.TryParse(value[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out double amount)) {
            span = TimeSpan.Zero;
            return false;
        }

        span = value[^1] switch {
            's' or 'S' => TimeSpan.FromSeconds(amount),
            'm' or 'M' => TimeSpan.FromMinutes(amount),
            'h' or 'H' => TimeSpan.FromHours(amount),
            'd' or 'D' => TimeSpan.FromDays(amount),
            _ => TimeSpan.Zero
        };
        return span > TimeSpan.Zero;
    }
}
