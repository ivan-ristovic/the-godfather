using Discord.Commands;
using TheGodfather.Attributes;

namespace TheGodfather.Modules.Owner;

[Group("status")]
[Alias("statuses", "botstatus", "activity", "activities")]
[RequireOwner]
public sealed class StatusModule : TheGodfatherModule
{
    private readonly BotActivityService activity;


    public StatusModule(LocalizationService localization, BotActivityService activity)
        : base(localization, new Color(0xE74C3C))
    {
        this.activity = activity;
    }


    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync(ActivityType type, [Remainder] string status)
        => this.SetAsync(type, status);

    [Command("add")]
    [Alias("register", "reg", "new", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public async Task AddAsync(ActivityType type, [Remainder] string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new LocalizedCommandException(TranslationKey.cmd_err_status_none);

        if (status.Length > BotStatus.StatusLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_status_size(BotStatus.StatusLimit));

        await this.activity.AddAsync(new BotStatus {
            Activity = type,
            Status = status
        });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("delete")]
    [Alias("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public async Task DeleteAsync(params int[] ids)
    {
        int removed = await this.activity.RemoveAsync(ids);
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_status_del(removed));
    }

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync()
    {
        IReadOnlyList<BotStatus> statuses = await this.activity.GetAsync();
        await this.RespondPagedAsync(
            TranslationKey.str_statuses,
            statuses,
            status => $"{InlineCode($"{status.Id:D2}")}: {status.Activity} - {status.Status}"
        );
    }

    [Command("set")]
    [Priority(1)]
    [Alias("s")]
    public async Task SetAsync(ActivityType type, [Remainder] string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new LocalizedCommandException(TranslationKey.cmd_err_status_none);

        if (status.Length > BotStatus.StatusLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_status_size(BotStatus.StatusLimit));

        this.activity.StatusRotationEnabled = false;
        await this.Context.Client.SetGameAsync(status, type: type);
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("set")]
    [Priority(0)]
    public async Task SetAsync(int id)
    {
        BotStatus? status = await this.activity.GetAsync(id);
        if (status is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_status_404);

        this.activity.StatusRotationEnabled = false;
        await this.Context.Client.SetGameAsync(status.Status, type: status.Activity);
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("setrotation")]
    [Alias("sr", "setr", "rotate")]
    public Task SetRotationAsync(bool enable = true)
    {
        this.activity.StatusRotationEnabled = enable;
        return this.InfoAsync(this.ModuleColor, TranslationKey.fmt_status_rot(this.activity.StatusRotationEnabled));
    }
}
