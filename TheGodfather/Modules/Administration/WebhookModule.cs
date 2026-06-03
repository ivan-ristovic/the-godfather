using Discord.Commands;
using TheGodfather.Attributes;

namespace TheGodfather.Modules.Administration;

[Group("webhook")]
[Alias("webhooks", "wh")]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageWebhooks)]
[RequireBotPermission(GuildPermission.ManageWebhooks)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class WebhookModule : TheGodfatherModule
{
    public WebhookModule(LocalizationService localization)
        : base(localization, new Color(0x3498DB)) { }


    [Command("")]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("add")]
    [Priority(1)]
    [Alias("create", "new", "a", "+")]
    public async Task AddAsync(ITextChannel channel, [Remainder] string name)
    {
        IWebhook webhook = await channel.CreateWebhookAsync(name, options: new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor, $"{webhook.Name} ({webhook.Id})");
    }

    [Command("add")]
    [Priority(0)]
    public Task AddCurrentAsync([Remainder] string name)
    {
        if (this.Context.Channel is not ITextChannel channel)
            return this.FailAsync(TranslationKey.cmd_err_chn_type_text);
        return this.AddAsync(channel, name);
    }

    [Command("delete")]
    [Priority(1)]
    [Alias("remove", "rm", "del", "d", "-", "-=", ">", ">>")]
    public async Task DeleteAsync(ulong id)
    {
        IWebhook? webhook = await FindWebhookAsync(id.ToString());
        if (webhook is null) {
            await this.FailAsync(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.cmd_err_chn_wh_uniq_id(id)));
            return;
        }
        await webhook.DeleteAsync(new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("delete")]
    [Priority(0)]
    public async Task DeleteByNameAsync([Remainder] string name)
    {
        IWebhook? webhook = await FindWebhookAsync(name);
        if (webhook is null) {
            await this.FailAsync(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.cmd_err_chn_wh_uniq_name(name)));
            return;
        }

        await webhook.DeleteAsync(new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("deleteall")]
    [Alias("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    public async Task DeleteAllAsync(ITextChannel? channel = null)
    {
        channel ??= this.Context.Channel as ITextChannel;
        if (channel is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chn_type_text);

        IReadOnlyCollection<IWebhook> hooks = await channel.GetWebhooksAsync();
        if (!await this.ConfirmAsync(TranslationKey.q_wh_rem_all(channel.Mention, hooks.Count)))
            return;

        foreach (IWebhook hook in hooks)
            await hook.DeleteAsync(new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_wh_del_all(channel.Mention));
    }

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync(ITextChannel? channel = null)
    {
        channel ??= this.Context.Channel as ITextChannel;
        if (channel is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chn_type_text);

        IReadOnlyCollection<IWebhook> hooks = await channel.GetWebhooksAsync();
        if (!hooks.Any()) {
            await this.InfoAsync(this.ModuleColor, TranslationKey.cmd_err_chn_wh_none);
            return;
        }

        await this.RespondPagedAsync(
            TranslationKey.evt_gld_wh_upd,
            hooks.OrderBy(h => h.Name),
            hook => $"{InlineCode(hook.Id)} | {hook.Name}"
        );
    }

    [Command("listall")]
    [Alias("lsa")]
    public async Task ListAllAsync()
    {
        var hooks = new List<IWebhook>();
        foreach (SocketTextChannel channel in this.Context.Guild.TextChannels)
            hooks.AddRange(await channel.GetWebhooksAsync());

        if (!hooks.Any()) {
            await this.InfoAsync(this.ModuleColor, TranslationKey.cmd_err_chn_wh_none);
            return;
        }

        await this.RespondPagedAsync(
            TranslationKey.evt_gld_wh_upd,
            hooks.OrderBy(h => h.ChannelId).ThenBy(h => h.Name),
            hook => $"{InlineCode(hook.Id)} | <#{hook.ChannelId}> | {hook.Name}"
        );
    }


    private async Task<IWebhook?> FindWebhookAsync(string nameOrId)
    {
        ulong? id = ulong.TryParse(nameOrId, out ulong parsedId) ? parsedId : null;

        foreach (SocketTextChannel channel in this.Context.Guild.TextChannels) {
            IWebhook? hook = (await channel.GetWebhooksAsync()).FirstOrDefault(h =>
                (id is not null && h.Id == id.Value) || string.Equals(h.Name, nameOrId, StringComparison.OrdinalIgnoreCase)
            );
            if (hook is not null)
                return hook;
        }

        return null;
    }

    private string InvocationReason(string? reason = null)
        => string.IsNullOrWhiteSpace(reason)
            ? $"Requested by {this.Context.User} ({this.Context.User.Id})"
            : $"{reason} | Requested by {this.Context.User} ({this.Context.User.Id})";
}
