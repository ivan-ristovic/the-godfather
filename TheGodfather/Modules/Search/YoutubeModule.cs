using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search
{
    [Group("youtube"), Module(ModuleType.Searches), NotBlocked]
    [Aliases("y", "yt", "ytube")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class YoutubeModule : TheGodfatherServiceModule<YtService>
    {
        #region youtube
        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("desc-query")] string query)
            => this.SearchAndSendResultsAsync(ctx, 5, query);
        #endregion

        #region youtube search
        [Command("search")]
        [Aliases("s")]
        public Task AdvancedSearchAsync(CommandContext ctx,
                                       [Description("desc-res-amount")] int amount,
                                       [RemainingText, Description("desc-query")] string query)
            => this.SearchAndSendResultsAsync(ctx, amount, query);
        #endregion

        #region youtube searchvideo
        [Command("searchvideo")]
        [Description("Advanced youtube search for videos only.")]
        [Aliases("searchvideos", "sv", "searchv", "video")]
        public Task SearchVideoAsync(CommandContext ctx,
                                    [RemainingText, Description("desc-query")] string query)
            => this.SearchAndSendResultsAsync(ctx, 5, query, "video");
        #endregion

        #region youtube searchchannel
        [Command("searchchannel")]
        [Aliases("searchchannels", "sc", "searchc", "channel")]
        public Task SearchChannelAsync(CommandContext ctx,
                                      [RemainingText, Description("desc-query")] string query)
            => this.SearchAndSendResultsAsync(ctx, 5, query, "channel");
        #endregion

        #region youtube searchplaylist
        [Command("searchplaylist")]
        [Aliases("searchplaylists", "sp", "searchp", "playlist")]
        public Task SearchPlaylistAsync(CommandContext ctx,
                                       [RemainingText, Description("desc-query")] string query)
            => this.SearchAndSendResultsAsync(ctx, 5, query, "playlist");
        #endregion

        #region youtube subscribe
        [Command("subscribe"), Priority(5)]
        [Aliases("sub", "follow")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public Task SubscribeAsync(CommandContext ctx,
                                  [Description("desc-sub-chn")] DiscordChannel chn,
                                  [Description("desc-sub-yt")] Uri url,
                                  [RemainingText, Description("desc-name-f")] string? name = null)
            => this.SubscribeAsync(ctx, chn, url.AbsoluteUri, name);
        
        [Command("subscribe"), Priority(4)]
        public Task SubscribeAsync(CommandContext ctx,
                                  [Description("desc-sub-yt")] Uri url,
                                  [Description("desc-sub-chn")] DiscordChannel chn,
                                  [RemainingText, Description("desc-name-f")] string? name = null)
            => this.SubscribeAsync(ctx, chn ?? ctx.Channel, url.AbsoluteUri, name);

        [Command("subscribe"), Priority(3)]
        public Task SubscribeAsync(CommandContext ctx,
                                  [Description("desc-sub-yt-username")] string username,
                                  [Description("desc-sub-chn")] DiscordChannel? chn = null,
                                  [RemainingText, Description("desc-name-f")] string? name = null)
            => this.SubscribeAsync(ctx, chn ?? ctx.Channel, username, name);

        [Command("subscribe"), Priority(2)]
        public async Task SubscribeAsync(CommandContext ctx,
                                        [Description("desc-sub-chn")] DiscordChannel chn,
                                        [Description("desc-sub-yt-username")] string username,
                                        [RemainingText, Description("desc-name-f")] string? name = null)
        {
            if (chn.Type != ChannelType.Text)
                throw new InvalidCommandUsageException(ctx, "cmd-err-chn-type-text");

            string? feed = await this.Service.GetRssUrlForChannel(username);
            if (feed is null)
                throw new CommandFailedException(ctx, "cmd-err-sub-yt");

            string fname = string.IsNullOrWhiteSpace(name) ? username : name;
            await ctx.Services.GetRequiredService<RssFeedsService>().SubscribeAsync(ctx.Guild.Id, chn.Id, feed, fname);
            await ctx.InfoAsync(this.ModuleColor);
        }

        [Command("subscribe"), Priority(1)]
        public Task SubscribeAsync(CommandContext ctx,
                                  [Description("desc-sub-yt-username")] string username,
                                  [RemainingText, Description("desc-name-f")] string? name = null)
            => this.SubscribeAsync(ctx, ctx.Channel, username, name);


        [Command("subscribe"), Priority(0)]
        public Task SubscribeAsync(CommandContext ctx,
                                  [Description("desc-sub-yt-username")] Uri url,
                                  [RemainingText, Description("desc-name-f")] string? name = null)
            => this.SubscribeAsync(ctx, ctx.Channel, url.AbsoluteUri, name);
        #endregion

        #region youtube unsubscribe
        [Command("unsubscribe")]
        [Aliases("unfollow", "unsub")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task UnsubscribeAsync(CommandContext ctx,
                                          [Description("desc-sub-name-url")] string subscription)
        {
            string? url = await this.Service.GetRssUrlForChannel(subscription);
            if (url is null)
                throw new CommandFailedException(ctx, "cmd-err-sub-yt");

            RssFeedsService rss = ctx.Services.GetRequiredService<RssFeedsService>();
            RssSubscription? sub = await rss.Subscriptions.GetByNameAsync((ctx.Guild.Id, ctx.Channel.Id), subscription);
            if (sub is null) {
                RssFeed? feed = await rss.GetByUrlAsync(url);
                if (feed is null)
                    throw new CommandFailedException(ctx, "cmd-err-sub-not");
                sub = await rss.Subscriptions.GetAsync((ctx.Guild.Id, ctx.Channel.Id), feed.Id);
            }

            if (sub is null)
                throw new CommandFailedException(ctx, "cmd-err-sub-not");
            await rss.Subscriptions.RemoveAsync(sub);
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion


        #region internals
        private async Task SearchAndSendResultsAsync(CommandContext ctx, int amount, string query, string? type = null)
        {
            if (this.Service.IsDisabled)
                throw new ServiceDisabledException(ctx);

            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException(ctx, "cmd-err-query");

            IReadOnlyList<SearchResult>? res = await this.Service.SearchAsync(query, amount, type);
            if (res is null) {
                await ctx.FailAsync("cmd-err-res-none");
                return;
            }

            await ctx.PaginateAsync(res, (emb, r) => {
                emb.WithTitle(r.Snippet.Title);
                emb.WithColor(DiscordColor.Red);
                emb.WithDescription(r.Snippet.Description, unknown: false);

                if (r.Snippet.Thumbnails is { })
                    emb.WithThumbnail(r.Snippet.Thumbnails.Default__.Url);

                emb.AddLocalizedTitleField("str-chn", r.Snippet.ChannelTitle, inline: true);
                emb.AddLocalizedTitleField("str-published", r.Snippet.PublishedAt, inline: true);

                string? url = this.Service.GetUrlForResourceId(r.Id);
                if (url is { })
                    emb.WithUrl(url);

                return emb;
            });
        }
        #endregion
    }
}
