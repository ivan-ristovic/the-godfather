using System.IO;
using System.Net;
using System.Net.Http.Headers;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Administration;

[Group("webhook")][Module(ModuleType.Administration)][NotBlocked]
[Aliases("wh", "webhooks", "whook")]
[Cooldown(3, 5, CooldownBucketType.Guild)]
[RequireGuild][RequirePermissions(Permissions.ManageWebhooks)]
public sealed class WebhookModule : TheGodfatherModule
{
    #region webhook
    [GroupCommand]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_chn_wh_list)] DiscordChannel? channel = null)
        => this.ListAsync(ctx, channel);
    #endregion

    #region webhook add
    [Command("add")][Priority(6)]
    [Aliases("create", "c", "register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_avatar_url)] Uri? avatarUrl,
        [Description(TranslationKey.desc_chn_wh_add)] DiscordChannel channel,
        [Description(TranslationKey.desc_chn_wh_name)] string name,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        => this.CreateWebhookAsync(ctx, channel, name, avatarUrl, reason);

    [Command("add")][Priority(5)]
    public Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_avatar_url)] Uri avatarUrl,
        [Description(TranslationKey.desc_chn_wh_name)] string name,
        [Description(TranslationKey.desc_chn_wh_add)] DiscordChannel channel,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        => this.CreateWebhookAsync(ctx, channel, name, avatarUrl, reason);

    [Command("add")][Priority(4)]
    public Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_chn_wh_add)] DiscordChannel channel,
        [Description(TranslationKey.desc_avatar_url)] Uri avatarUrl,
        [Description(TranslationKey.desc_chn_wh_name)] string name,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        => this.CreateWebhookAsync(ctx, channel, name, avatarUrl, reason);

    [Command("add")][Priority(3)]
    public Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_chn_wh_add)] DiscordChannel channel,
        [Description(TranslationKey.desc_chn_wh_name)] string name,
        [Description(TranslationKey.desc_avatar_url)] Uri avatarUrl,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        => this.CreateWebhookAsync(ctx, channel, name, avatarUrl, reason);

    [Command("add")][Priority(2)]
    public Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_chn_wh_name)] string name,
        [Description(TranslationKey.desc_avatar_url)] Uri avatarUrl,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        => this.CreateWebhookAsync(ctx, ctx.Channel, name, avatarUrl, reason);

    [Command("add")][Priority(1)]
    public Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_avatar_url)] Uri avatarUrl,
        [Description(TranslationKey.desc_chn_wh_name)] string name,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        => this.CreateWebhookAsync(ctx, ctx.Channel, name, avatarUrl, reason);

    [Command("add")][Priority(0)]
    public Task AddAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_chn_wh_name)] string name)
        => this.CreateWebhookAsync(ctx, ctx.Channel, name, null, null);
    #endregion

    #region webhook delete
    [Command("delete")][Priority(3)]
    public Task DeleteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_chn_wh_del)] DiscordChannel channel,
        [RemainingText][Description(TranslationKey.desc_chn_wh_name)] string name)
        => this.DeleteAsync(ctx, name, channel);

    [Command("delete")][Priority(2)]
    public Task DeleteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_chn_wh_del)] DiscordChannel channel,
        [Description(TranslationKey.desc_id)] ulong whid)
        => this.DeleteAsync(ctx, whid, channel);

    [Command("delete")][Priority(1)]
    [Aliases("remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public async Task DeleteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_chn_wh_name)] string name,
        [Description(TranslationKey.desc_chn_wh_del)] DiscordChannel? channel = null)
    {
        channel ??= ctx.Channel;

        // TODO what about other channel types? news, store etc?
        if (channel?.Type != ChannelType.Text)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_chn_type_text);

        IEnumerable<DiscordWebhook> whs = await channel.GetWebhooksAsync();
        DiscordWebhook? wh = whs.SingleOrDefault(w => w.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        if (wh is null)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_chn_wh_uniq_name(Formatter.Bold(Formatter.Strip(name))));

        await wh.DeleteAsync();
        await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_wh_del(Formatter.Bold(Formatter.Strip(name))));
    }

    [Command("delete")][Priority(0)]
    public async Task DeleteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_id)] ulong whid,
        [Description(TranslationKey.desc_chn_wh_del)] DiscordChannel? channel = null)
    {
        channel ??= ctx.Channel;

        // TODO what about other channel types? news, store etc?
        if (channel?.Type != ChannelType.Text)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_chn_type_text);

        IEnumerable<DiscordWebhook> whs = await channel.GetWebhooksAsync();
        DiscordWebhook? wh = whs.SingleOrDefault(w => w.Id == whid);
        if (wh is null)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_chn_wh_uniq_id(Formatter.Bold(Formatter.Strip(whid.ToString()))));

        await wh.DeleteAsync();
        await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_wh_del(Formatter.Bold(Formatter.Strip(wh.Name))));
    }
    #endregion

    #region webhook deleteall
    [Command("deleteall")][Priority(1)][UsesInteractivity]
    [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    public async Task DeleteAllAsync(CommandContext ctx,
        [Description(TranslationKey.desc_chn_wh_del)] DiscordChannel? channel = null)
    {
        channel ??= ctx.Channel;

        // TODO what about other channel types? news, store etc?
        if (channel?.Type != ChannelType.Text)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_chn_type_text);

        IReadOnlyList<DiscordWebhook> whs = await channel.GetWebhooksAsync();
        if (whs.Any()) {
            if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_wh_rem_all(channel.Mention, whs.Count)))
                return;
            await Task.WhenAll(whs.Select(w => w.DeleteAsync()));
        }

        await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_wh_del_all(channel.Mention));
    }

    [Command("deleteall")][Priority(0)]
    public async Task DeleteAllAsync(CommandContext ctx,
        [Description(TranslationKey.desc_chn_wh_del)] params DiscordChannel[] channels)
    {
        foreach (DiscordChannel channel in channels.Where(c => c.Type == ChannelType.Text))
            await this.DeleteAllAsync(ctx, channel);
    }
    #endregion

    #region webhook list
    [Command("list")][UsesInteractivity]
    [Aliases("l", "ls", "show", "s", "print")]
    public Task ListAsync(CommandContext ctx,
        [Description(TranslationKey.desc_chn_wh_list)] DiscordChannel? channel = null)
        => this.PrintWebhooksAsync(ctx, channel ?? ctx.Channel);
    #endregion

    #region webhook listall
    [Command("listall")][UsesInteractivity]
    [Aliases("la", "lsa", "showall", "printall")]
    public Task ListAllAsync(CommandContext ctx)
        => this.PrintWebhooksAsync(ctx);
    #endregion


    #region internals
    private async Task CreateWebhookAsync(CommandContext ctx, DiscordChannel channel, string name, Uri? avatarUrl, string? reason)
    {
        // TODO what about other channel types? news, store etc?
        if (channel?.Type != ChannelType.Text)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_chn_type_text);

        if (string.IsNullOrWhiteSpace(name) || name.Length > DiscordLimits.NameLimit)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_name(DiscordLimits.NameLimit));

        DiscordWebhook wh;
        if (avatarUrl is null) {
            wh = await channel.CreateWebhookAsync(name, reason: ctx.BuildInvocationDetailsString(reason));
        } else {
            (_, HttpContentHeaders headers) = await HttpService.HeadAsync(avatarUrl);
            if (!headers.ContentTypeHeaderIsImage() || headers.ContentLength.GetValueOrDefault() > 8 * 1024 * 1024)
                throw new CommandFailedException(ctx, TranslationKey.err_url_image_8mb);
            try {
                await using MemoryStream ms = await HttpService.GetMemoryStreamAsync(avatarUrl);
                wh = await channel.CreateWebhookAsync(name, ms, ctx.BuildInvocationDetailsString(reason));
            } catch (WebException e) {
                throw new CommandFailedException(ctx, e, TranslationKey.err_url_image_fail);
            }
        }

        if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_send_token))
            try {
                DiscordDmChannel? dm = await ctx.Client.CreateDmChannelAsync(ctx.User.Id);
                if (dm is { }) {
                    var emb = new LocalizedEmbedBuilder(this.Localization, ctx.Guild.Id);
                    emb.WithLocalizedTitle(TranslationKey.fmt_wh_add(Formatter.Bold(Formatter.Strip(wh.Name)), channel.Mention));
                    emb.WithDescription(Formatter.Spoiler(wh.BuildUrlString()));
                    emb.WithColor(this.ModuleColor);
                    emb.WithThumbnail(wh.AvatarUrl);
                    emb.AddLocalizedField(TranslationKey.str_id, wh.Id, true);
                    emb.AddLocalizedField(TranslationKey.str_name, wh.Name, true);
                    emb.AddLocalizedField(TranslationKey.str_token, Formatter.Spoiler(wh.Token));
                    await dm.SendMessageAsync(emb.Build());
                } else {
                    await ctx.FailAsync(TranslationKey.err_dm_fail);
                }
            } catch {
                await ctx.FailAsync(TranslationKey.err_dm_fail);
            }

        await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_wh_add(Formatter.Bold(Formatter.Strip(wh.Name)), channel.Mention));
    }

    private async Task PrintWebhooksAsync(CommandContext ctx, DiscordChannel? channel = null)
    {
        // TODO what about other channel types? news, store etc?
        if (channel?.Type != ChannelType.Text)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_chn_type_text);

        IReadOnlyList<DiscordWebhook> whs = await (channel?.GetWebhooksAsync() ?? ctx.Guild.GetWebhooksAsync());
        if (!whs.Any()) {
            await ctx.InfoAsync(this.ModuleColor, TranslationKey.cmd_err_chn_wh_none);
            return;
        }

        bool displayToken = await ctx.WaitForBoolReplyAsync(TranslationKey.q_display_tokens, reply: false);

        await ctx.PaginateAsync(
            whs.OrderBy(wh => wh.ChannelId),
            (emb, wh) => emb
                .WithLocalizedTitle(TranslationKey.fmt_wh(wh.Name, wh.ChannelId))
                .WithThumbnail(wh.AvatarUrl)
                .AddLocalizedField(TranslationKey.str_id, wh.Id, true)
                .AddLocalizedField(TranslationKey.str_created_at, this.Localization.GetLocalizedTimeString(ctx.Guild.Id, wh.CreationTimestamp, unknown: true), true)
                .AddLocalizedField(TranslationKey.str_created_by, wh.User?.Mention, true)
                .AddLocalizedField(TranslationKey.str_token, SanitizeWebhookData(wh, wh.Token), unknown: false)
                .AddLocalizedField(TranslationKey.str_url, SanitizeWebhookData(wh, wh.BuildUrlString()), unknown: false)
            ,
            this.ModuleColor
        );


        string? SanitizeWebhookData(DiscordWebhook wh, string data)
            => displayToken && !string.IsNullOrWhiteSpace(wh.Token) ? Formatter.Spoiler(data) : null;
    }
    #endregion
}