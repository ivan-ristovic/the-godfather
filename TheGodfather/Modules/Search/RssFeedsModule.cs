using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Attributes;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search
{
    [Module(ModuleType.Searches), NotBlocked]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class RssFeedsModule : TheGodfatherServiceModule<RssFeedsService>
    {
        #region rss
        [Command("rss")]
        [Aliases("feed")]
        public Task RssAsync(CommandContext ctx,
                            [Description("desc-rss-url")] Uri url)
        {
            if (!RssFeedsService.IsValidFeedURL(url.AbsoluteUri))
                throw new CommandFailedException(ctx, "cmd-err-rss");

            IReadOnlyList<SyndicationItem>? res = RssFeedsService.GetFeedResults(url.AbsoluteUri);
            if (res is null)
                throw new CommandFailedException(ctx, "cmd-err-sub-fail", url);

            return ctx.PaginateAsync(res, (emb, r) => {
                emb.WithTitle(r.Title.Text);
                emb.WithDescription(r.Summary, unknown: false);
                if (r.Links.Any())
                    emb.WithUrl(r.Links.First().Uri);
                if (r.Authors.Any())
                    emb.AddLocalizedTitleField("str-author", r.Authors.First().Name, inline: true, unknown: false);
                emb.WithLocalizedTimestamp(r.LastUpdatedTime);
                return emb;
            }, this.ModuleColor);
        }
        #endregion

        #region subscribe
        [Group("subscribe")]
        [Aliases("sub", "subscriptions", "subscription")]
        [RequireGuild, RequireOwnerOrPermissions(Permissions.ManageGuild)]
        public sealed class SubscribeModule : TheGodfatherServiceModule<RssFeedsService>
        {
            #region subscribe
            [GroupCommand, Priority(2)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("desc-sub-chn")] DiscordChannel chn,
                                               [Description("desc-rss-url")] Uri url,
                                               [RemainingText, Description("desc-name-f")] string? name = null)
            {
                chn ??= ctx.Channel;
                if (chn.Type != ChannelType.Text)
                    throw new InvalidCommandUsageException(ctx, "cmd-err-chn-type-text");

                if (!RssFeedsService.IsValidFeedURL(url.AbsoluteUri))
                    throw new CommandFailedException(ctx, "cmd-err-rss");
                
                await this.Service.SubscribeAsync(ctx.Guild.Id, chn.Id, url.AbsoluteUri, name);
                await ctx.InfoAsync(this.ModuleColor);
            }

            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-rss-url")] Uri url,
                                         [Description("desc-sub-chn")] DiscordChannel? chn = null,
                                         [RemainingText, Description("desc-name-f")] string? name = null)
                => this.ExecuteGroupAsync(ctx, url, chn, name);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-sub-chn")] DiscordChannel? chn = null)
                => this.ListAsync(ctx, chn);
            #endregion

            #region subscribe list
            [Command("list")]
            [Aliases("ls", "listsubs", "listfeeds")]
            public async Task ListAsync(CommandContext ctx,
                                       [Description("desc-sub-chn")] DiscordChannel? chn = null)
            {
                chn ??= ctx.Channel;
                if (chn.Type != ChannelType.Text)
                    throw new InvalidCommandUsageException(ctx, "cmd-err-chn-type-text");

                IReadOnlyList<RssSubscription> subs = await this.Service.Subscriptions.GetAllAsync((ctx.Guild.Id, chn.Id));
                if (!subs.Any())
                    throw new CommandFailedException(ctx, "cmd-err-subs-none", chn.Mention);

                await ctx.PaginateAsync(
                    "str-subs",
                    subs,
                    sub => {
                        string? qname = sub.Name;
                        return $"{Formatter.InlineCode($"{sub.Id:D4}")} | {(string.IsNullOrWhiteSpace(qname) ? sub.Feed.Url : qname)}";
                    },
                    this.ModuleColor,
                    5,
                    chn.Mention
                );
            }
            #endregion

            #region subscribe reddit
            [Command("reddit"), Priority(1)]
            [Aliases("r")]
            public Task RedditAsync(CommandContext ctx,
                                   [Description("desc-sub-chn")] DiscordChannel chn,
                                   [Description("desc-sub")] string sub)
                => ctx.ExecuteOtherCommandAsync("reddit subscribe", chn.Mention, sub);
            
            [Command("reddit"), Priority(0)]
            public Task RedditAsync(CommandContext ctx,
                                   [Description("desc-sub")] string sub,
                                   [Description("desc-sub-chn")] DiscordChannel? chn = null)
                => this.RedditAsync(ctx, chn ?? ctx.Channel, sub);
            #endregion

            #region subscribe youtube
            [Command("youtube")]
            [Aliases("y", "yt", "ytube")]
            public Task YoutubeAsync(CommandContext ctx,
                                    [Description("desc-sub-chn")] DiscordChannel chn,
                                    [Description("desc-sub-url")] Uri url,
                                    [RemainingText, Description("Friendly name.")] string? name = null)
                => ctx.ExecuteOtherCommandAsync("youtube subscribe", chn.Mention, url.ToString(), name);

            [Command("youtube"), Priority(0)]
            public Task YoutubeAsync(CommandContext ctx,
                                    [Description("desc-sub-url")] Uri url,
                                    [Description("desc-sub-chn")] DiscordChannel? chn = null,
                                    [RemainingText, Description("Friendly name.")] string? name = null)
                => this.YoutubeAsync(ctx, chn ?? ctx.Channel, url, name);
            #endregion
        }
        #endregion

        #region unsubscribe
        [Group("unsubscribe")]
        [Aliases("unsub")]
        [RequireGuild, RequireOwnerOrPermissions(Permissions.ManageGuild)]
        public sealed class UnsubscribeModule : TheGodfatherServiceModule<RssFeedsService>
        {
            #region unsubscribe
            [GroupCommand, Priority(1)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("desc-ids")] params int[] ids)
            {
                if (ids is null || !ids.Any())
                    throw new CommandFailedException(ctx, "cmd-err-ids-none");

                int removed = await this.Service.Subscriptions.RemoveAsync((ctx.Guild.Id, ctx.Channel.Id), ids);
                await ctx.InfoAsync(this.ModuleColor, "fmt-unsub", removed);
            }

            [GroupCommand, Priority(0)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [RemainingText, Description("desc-name-f")] string name)
            {
                RssSubscription? sub = await this.Service.Subscriptions.GetByNameAsync((ctx.Guild.Id, ctx.Channel.Id), name);
                if (sub is null)
                    throw new CommandFailedException(ctx, "cmd-err-sub-name", ctx.Channel.Mention);

                int removed = await this.Service.Subscriptions.RemoveAsync(sub);
                await ctx.InfoAsync(this.ModuleColor, "fmt-unsub", removed);
            }
            #endregion

            #region unsubscribe all
            [Command("all"), UsesInteractivity]
            [Aliases("a")]
            public async Task AllAsync(CommandContext ctx,
                                      [Description("desc-sub-chn")] DiscordChannel? chn = null)
            {
                chn ??= ctx.Channel;

                if (!await ctx.WaitForBoolReplyAsync("q-unsub", args: chn.Mention))
                    return;

                await this.Service.Subscriptions.ClearAsync((ctx.Guild.Id, ctx.Channel.Id));
                await ctx.InfoAsync(this.ModuleColor);
            }
            #endregion

            #region unsubscribe reddit
            [Command("reddit")]
            [Aliases("r")]
            public Task RedditAsync(CommandContext ctx,
                                   [Description("desc-sub")] string sub)
                => ctx.ExecuteOtherCommandAsync("reddit unsubscribe", sub);
            #endregion

            #region unsubscribe youtube
            [Command("youtube")]
            [Aliases("y", "yt", "ytube")]
            public Task UnsubscribeAsync(CommandContext ctx,
                                        [RemainingText, Description("desc-sub-name-url")] string name_url)
            {
                if (string.IsNullOrWhiteSpace(name_url))
                    throw new InvalidCommandUsageException(ctx, "cmd-err-name-404");

                return ctx.ExecuteOtherCommandAsync("youtube unsubscribe", name_url);
            }
            #endregion
        }
        #endregion
    }
}
