using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration
{
    [Group("webhook"), Module(ModuleType.Administration), NotBlocked]
    [Aliases("wh", "webhooks", "whook")]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    [RequireGuild, RequirePermissions(Permissions.ManageWebhooks)]
    public sealed class WebhookModule : TheGodfatherModule
    {
        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-chn-wh-list")] DiscordChannel? channel = null)
            => this.ListAsync(ctx, channel);


        #region webhook add
        [Command("add"), Priority(7)]
        [Aliases("create", "c", "register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-avatar-url")] Uri? avatarUrl,
                            [Description("desc-chn-wh-add")] DiscordChannel channel,
                            [Description("desc-chn-wh-name")] string name,
                            [RemainingText, Description("desc-rsn")] string? reason = null)
            => this.CreateWebhookAsync(ctx, channel, name, avatarUrl, reason);

        [Command("add"), Priority(6)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-avatar-url")] Uri avatarUrl,
                            [Description("desc-chn-wh-name")] string name,
                            [Description("desc-chn-wh-add")] DiscordChannel channel,
                            [RemainingText, Description("desc-rsn")] string? reason = null)
            => this.CreateWebhookAsync(ctx, channel, name, avatarUrl, reason);

        [Command("add"), Priority(5)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-chn-wh-add")] DiscordChannel channel,
                            [Description("desc-avatar-url")] Uri avatarUrl,
                            [Description("desc-chn-wh-name")] string name,
                            [RemainingText, Description("desc-rsn")] string? reason = null)
            => this.CreateWebhookAsync(ctx, channel, name, avatarUrl, reason);

        [Command("add"), Priority(4)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-chn-wh-add")] DiscordChannel channel,
                            [Description("desc-chn-wh-name")] string name,
                            [Description("desc-avatar-url")] Uri avatarUrl,
                            [RemainingText, Description("desc-rsn")] string? reason = null)
            => this.CreateWebhookAsync(ctx, channel, name, avatarUrl, reason);

        [Command("add"), Priority(3)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-chn-wh-name")] string name,
                            [Description("desc-avatar-url")] Uri avatarUrl,
                            [RemainingText, Description("desc-rsn")] string? reason = null)
            => this.CreateWebhookAsync(ctx, ctx.Channel, name, avatarUrl, reason);

        [Command("add"), Priority(2)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-avatar-url")] Uri avatarUrl,
                            [Description("desc-chn-wh-name")] string name,
                            [RemainingText, Description("desc-rsn")] string? reason = null)
            => this.CreateWebhookAsync(ctx, ctx.Channel, name, avatarUrl, reason);

        [Command("add"), Priority(1)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-avatar-url")] Uri avatarUrl,
                            [RemainingText, Description("desc-chn-wh-name")] string name)
            => this.CreateWebhookAsync(ctx, ctx.Channel, name, avatarUrl, null);

        [Command("add"), Priority(0)]
        public Task AddAsync(CommandContext ctx,
                            [RemainingText, Description("desc-chn-wh-name")] string name)
            => this.CreateWebhookAsync(ctx, ctx.Channel, name, null, null);
        #endregion

        #region webhook delete
        [Command("delete"), Priority(4)]
        public Task DeleteAsync(CommandContext ctx,
                               [Description("desc-chn-wh-del")] DiscordChannel channel,
                               [RemainingText, Description("desc-chn-wh-name")] string name)
            => this.DeleteAsync(ctx, name, channel);

        [Command("delete"), Priority(3)]
        public Task DeleteAsync(CommandContext ctx,
                               [Description("desc-chn-wh-del")] DiscordChannel channel,
                               [Description("str-id")] ulong whid)
            => this.DeleteAsync(ctx, whid, channel);

        [Command("delete"), Priority(2)]
        [Aliases("remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("desc-chn-wh-name")] string name,
                                     [Description("desc-chn-wh-del")] DiscordChannel? channel = null)
        {
            channel ??= ctx.Channel;

            // TODO what about other channel types? news, store etc?
            if (channel?.Type != ChannelType.Text)
                throw new InvalidCommandUsageException(ctx, "cmd-err-chn-type-text");

            IEnumerable<DiscordWebhook> whs = await channel.GetWebhooksAsync();
            DiscordWebhook? wh = whs.SingleOrDefault(w => w.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (wh is null)
                throw new CommandFailedException(ctx, "cmd-err-chn-wh-uniq-name", Formatter.Bold(Formatter.Strip(name)));

            await wh.DeleteAsync();
            await ctx.InfoAsync(this.ModuleColor, "fmt-wh-del", Formatter.Bold(Formatter.Strip(name)));
        }

        [Command("delete"), Priority(1)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("str-id")] ulong whid,
                                     [Description("desc-chn-wh-del")] DiscordChannel? channel = null)
        {
            channel ??= ctx.Channel;

            // TODO what about other channel types? news, store etc?
            if (channel?.Type != ChannelType.Text)
                throw new InvalidCommandUsageException(ctx, "cmd-err-chn-type-text");

            IEnumerable<DiscordWebhook> whs = await channel.GetWebhooksAsync();
            DiscordWebhook? wh = whs.SingleOrDefault(w => w.Id == whid);
            if (wh is null)
                throw new CommandFailedException(ctx, "cmd-err-chn-wh-uniq-id", Formatter.Bold(Formatter.Strip(whid.ToString())));

            await wh.DeleteAsync();
            await ctx.InfoAsync(this.ModuleColor, "fmt-wh-del", Formatter.Bold(Formatter.Strip(wh.Name)));
        }

        [Command("delete"), Priority(0)]
        public Task DeleteAsync(CommandContext ctx,
                               [RemainingText, Description("desc-chn-wh-name")] string name)
            => this.DeleteAsync(ctx, name, ctx.Channel);
        #endregion

        #region webhook deleteall
        [Command("deleteall"), Priority(1), UsesInteractivity]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
        public async Task DeleteAllAsync(CommandContext ctx,
                                        [Description("desc-chn-wh-del")] DiscordChannel? channel = null)
        {
            channel ??= ctx.Channel;

            // TODO what about other channel types? news, store etc?
            if (channel?.Type != ChannelType.Text)
                throw new InvalidCommandUsageException(ctx, "cmd-err-chn-type-text");

            IReadOnlyList<DiscordWebhook> whs = await channel.GetWebhooksAsync();
            if (whs.Any()) {
                if (!await ctx.WaitForBoolReplyAsync("q-wh-rem-all", args: new object[] { channel.Mention, whs.Count }))
                    return;
                await Task.WhenAll(whs.Select(w => w.DeleteAsync()));
            }

            await ctx.InfoAsync(this.ModuleColor, "fmt-wh-del-all", channel.Mention);
        }

        [Command("deleteall"), Priority(0)]
        public async Task DeleteAllAsync(CommandContext ctx,
                                        [Description("desc-chn-wh-del")] params DiscordChannel[] channels)
        {
            foreach (DiscordChannel channel in channels.Where(c => c.Type == ChannelType.Text))
                await this.DeleteAllAsync(ctx, channel);
        }
        #endregion

        #region webhook list
        [Command("list"), UsesInteractivity]
        [Aliases("l", "ls", "show", "s", "print")]
        public Task ListAsync(CommandContext ctx,
                             [Description("desc-chn-wh-list")] DiscordChannel? channel = null)
            => this.PrintWebhooksAsync(ctx, channel ?? ctx.Channel);
        #endregion

        #region webhook listall
        [Command("listall"), UsesInteractivity]
        [Aliases("la", "lsa", "showall", "printall")]
        public Task ListAllAsync(CommandContext ctx)
            => this.PrintWebhooksAsync(ctx);
        #endregion


        #region Helpers
        private async Task CreateWebhookAsync(CommandContext ctx, DiscordChannel channel, string name, Uri? avatarUrl, string? reason)
        {
            // TODO what about other channel types? news, store etc?
            if (channel?.Type != ChannelType.Text)
                throw new InvalidCommandUsageException(ctx, "cmd-err-chn-type-text");

            if (string.IsNullOrWhiteSpace(name) || name.Length > DiscordLimits.NameLimit)
                throw new CommandFailedException(ctx, "cmd-err-name", DiscordLimits.NameLimit);

            DiscordWebhook wh;
            if (avatarUrl is null) {
                wh = await channel.CreateWebhookAsync(name, reason: ctx.BuildInvocationDetailsString(reason));
            } else {
                (_, HttpContentHeaders headers) = await HttpService.HeadAsync(avatarUrl);
                if (!headers.ContentTypeHeaderIsImage() || headers.ContentLength.GetValueOrDefault() > 8 * 1024 * 1024)
                    throw new CommandFailedException(ctx, "err-url-image-8mb");
                try {
                    using MemoryStream ms = await HttpService.GetMemoryStreamAsync(avatarUrl);
                    wh = await channel.CreateWebhookAsync(name, ms, reason: ctx.BuildInvocationDetailsString(reason));
                } catch (WebException e) {
                    throw new CommandFailedException(ctx, "err-url-image-fail", e);
                }
            }

            if (await ctx.WaitForBoolReplyAsync("q-send-token")) {
                try {
                    DiscordDmChannel? dm = await ctx.Client.CreateDmChannelAsync(ctx.User.Id);
                    if (dm is { }) {
                        var emb = new LocalizedEmbedBuilder(ctx.Services.GetRequiredService<LocalizationService>(), ctx.Guild.Id);
                        emb.WithLocalizedTitle("fmt-wh-add", Formatter.Bold(Formatter.Strip(wh.Name)), channel.Mention);
                        emb.WithDescription(FormatterExt.Spoiler(wh.BuildUrlString()));
                        emb.WithColor(this.ModuleColor);
                        emb.WithThumbnail(wh.AvatarUrl);
                        emb.AddLocalizedTitleField("str-id", wh.Id, inline: true);
                        emb.AddLocalizedTitleField("str-name", wh.Name, inline: true);
                        emb.AddLocalizedTitleField("str-token", FormatterExt.Spoiler(wh.Token));
                        await dm.SendMessageAsync(embed: emb.Build());
                    } else {
                        await ctx.FailAsync("err-dm-fail");
                    }
                } catch {
                    await ctx.FailAsync("err-dm-fail");
                }
            }

            await ctx.InfoAsync(this.ModuleColor, "fmt-wh-add", Formatter.Bold(Formatter.Strip(wh.Name)), channel.Mention);
        }

        private async Task PrintWebhooksAsync(CommandContext ctx, DiscordChannel? channel = null)
        {
            // TODO what about other channel types? news, store etc?
            if (channel?.Type != ChannelType.Text)
                throw new InvalidCommandUsageException(ctx, "cmd-err-chn-type-text");

            IReadOnlyList<DiscordWebhook> whs = await (channel?.GetWebhooksAsync() ?? ctx.Guild.GetWebhooksAsync());
            if (!whs.Any()) {
                await ctx.InfoAsync(this.ModuleColor, "cmd-err-chn-wh-none");
                return;
            }

            bool displayToken = await ctx.WaitForBoolReplyAsync("q-display-tokens", reply: false);

            LocalizationService ls = ctx.Services.GetRequiredService<LocalizationService>();
            await ctx.PaginateAsync(
                whs.OrderBy(wh => wh.ChannelId),
                (emb, wh) => emb
                    .WithLocalizedTitle("fmt-wh", wh.Name, wh.ChannelId)
                    .WithThumbnail(wh.AvatarUrl)
                    .AddLocalizedTitleField("str-id", wh.Id, inline: true)
                    .AddLocalizedTitleField("str-created-at", ls.GetLocalizedTime(ctx.Guild.Id, wh.CreationTimestamp, unknown: true), inline: true)
                    .AddLocalizedTitleField("str-created-by", wh.User?.Mention, inline: true)
                    .AddLocalizedTitleField("str-token", SanitizeWebhookData(wh, wh.Token), unknown: false)
                    .AddLocalizedTitleField("str-url", SanitizeWebhookData(wh, wh.BuildUrlString()), unknown: false)
                    ,
                this.ModuleColor
            );


            string? SanitizeWebhookData(DiscordWebhook wh, string data)
                => displayToken && !string.IsNullOrWhiteSpace(wh.Token) ? FormatterExt.Spoiler(data) : null;
        }
        #endregion
    }
}
