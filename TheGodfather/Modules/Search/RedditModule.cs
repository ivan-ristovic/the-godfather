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
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search
{
    [Group("reddit"), Module(ModuleType.Searches), NotBlocked]
    [Aliases("r")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class RedditModule : TheGodfatherServiceModule<RssFeedsService>
    {
        #region reddit
        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-sub")] string sub = "all")
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.Hot);
        #endregion

        #region reddit controversial
        [Command("controversial")]
        [Description("Get newest controversial posts for a subreddit.")]
        [Aliases("c")]
        public Task ControversialAsync(CommandContext ctx,
                                      [Description("desc-sub")] string sub)
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.Controversial);
        #endregion

        #region reddit gilded
        [Command("gilded")]
        [Aliases("g")]
        public Task GildedAsync(CommandContext ctx,
                               [Description("desc-sub")] string sub)
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.Gilded);
        #endregion

        #region reddit hot
        [Command("hot")]
        [Aliases("h")]
        public Task HotAsync(CommandContext ctx,
                            [Description("desc-sub")] string sub)
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.Hot);
        #endregion

        #region reddit new
        [Command("new")]
        [Aliases("n", "newest", "latest")]
        public Task NewAsync(CommandContext ctx,
                            [Description("desc-sub")] string sub)
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.New);
        #endregion

        #region reddit rising
        [Command("rising")]
        [Aliases("r")]
        public Task RisingAsync(CommandContext ctx,
                               [Description("desc-sub")] string sub)
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.Rising);
        #endregion

        #region reddit top
        [Command("top")]
        [Aliases("t")]
        public Task TopAsync(CommandContext ctx,
                            [Description("desc-sub")] string sub)
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.Top);
        #endregion

        #region reddit subscribe
        [Command("subscribe"), Priority(1)]
        [Aliases("sub", "follow")]
        [RequireGuild, RequireUserPermissions(Permissions.ManageGuild)]
        public async Task SubscribeAsync(CommandContext ctx,
                                        [Description("desc-chn")] DiscordChannel chn,
                                        [Description("desc-sub")] string sub)
        {
            chn ??= ctx.Channel;
            if (chn.Type != ChannelType.Text)
                throw new InvalidCommandUsageException(ctx, "cmd-err-chn-type-text");

            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidCommandUsageException(ctx, "cmd-err-sub-none");

            string? url = RedditService.GetFeedURLForSubreddit(sub, RedditCategory.New, out string? rsub);
            if (url is null || rsub is null) {
                if (rsub is null)
                    throw new CommandFailedException(ctx, "cmd-err-sub-format");
                else
                    throw new CommandFailedException(ctx, "cmd-err-sub-404", rsub);
            }

            if (await this.Service.SubscribeAsync(ctx.Guild.Id, ctx.Channel.Id, url, rsub))
                await ctx.InfoAsync(this.ModuleColor);
            else
                await ctx.FailAsync("cmd-err-sub", url);
        }

        public Task SubscribeAsync(CommandContext ctx,
                                  [Description("desc-sub")] string sub,
                                  [Description("desc-chn")] DiscordChannel? chn = null)
            => this.SubscribeAsync(ctx, chn ?? ctx.Channel, sub);
        #endregion

        #region reddit unsubscribe
        [Command("unsubscribe")]
        [Aliases("unfollow", "unsub")]
        [RequireGuild, RequireUserPermissions(Permissions.ManageGuild)]
        public async Task UnsubscribeAsync(CommandContext ctx,
                                          [Description("desc-sub")] string sub)
        {
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidCommandUsageException(ctx, "cmd-err-sub-none");

            string? url = RedditService.GetFeedURLForSubreddit(sub, RedditCategory.New, out string? rsub);
            if (url is null || rsub is null) {
                if (rsub is null)
                    throw new CommandFailedException(ctx, "cmd-err-sub-format");
                else
                    throw new CommandFailedException(ctx, "cmd-err-sub-404", rsub);
            }

            RssFeed? feed = await this.Service.GetByUrlAsync(url);
            if (feed is null)
                throw new InvalidCommandUsageException(ctx, "cmd-err-sub-not");

            RssSubscription? s = await this.Service.Subscriptions.GetAsync((ctx.Guild.Id, ctx.Channel.Id), feed.Id);
            if (s is null)
                throw new InvalidCommandUsageException(ctx, "cmd-err-sub-not");

            await this.Service.Subscriptions.RemoveAsync((ctx.Guild.Id, ctx.Channel.Id), feed.Id);
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion


        #region internals
        private Task SearchAndSendResultsAsync(CommandContext ctx, string sub, RedditCategory category)
        {
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidCommandUsageException(ctx, "cmd-err-sub-none");

            string? url = RedditService.GetFeedURLForSubreddit(sub, category, out string? rsub);
            if (url is null || rsub is null) {
                if (rsub is null)
                    throw new CommandFailedException(ctx, "cmd-err-sub-format");
                else
                    throw new CommandFailedException(ctx, "cmd-err-sub-404", rsub);
            }

            IReadOnlyList<SyndicationItem>? res = RssFeedsService.GetFeedResults(url);
            if (res is null)
                throw new CommandFailedException(ctx, "cmd-err-sub-fail", rsub);

            if (!res.Any())
                return ctx.FailAsync("cmd-err-res-none");

            return ctx.PaginateAsync(res, (emb, r) => {
                emb.WithTitle(r.Title.Text);
                emb.WithDescription(r.Summary, unknown: false);
                emb.WithUrl(r.Links.First().Uri);
                if (r.Content is TextSyndicationContent content) {
                    string? url = RedditService.GetImageUrl(content);
                    if (url is { })
                        emb.WithImageUrl(url);
                }
                emb.AddLocalizedTitleField("str-author", r.Authors.First().Name, inline: true);
                emb.WithLocalizedTimestamp(r.LastUpdatedTime);
                return emb;
            }, this.ModuleColor);
        }
        #endregion
    }
}
