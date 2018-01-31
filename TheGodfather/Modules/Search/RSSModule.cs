#region USING_DIRECTIVES
using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Attributes;
using TheGodfather.Services;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("rss")]
    [Description("RSS feed operations.")]
    [Aliases("feed")]
    [Cooldown(2, 5, CooldownBucketType.User), Cooldown(4, 5, CooldownBucketType.Channel)]
    [ListeningCheckAttribute]
    public class RSSModule : BaseCommandModule
    {

        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                           [RemainingText, Description("URL.")] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidCommandUsageException("URL missing.");

            await SendFeedResultsAsync(ctx, FeedService.GetFeedResults(url))
                .ConfigureAwait(false);
        }


        #region COMMAND_SUBSCRIBE
        [Command("subscribe")]
        [Description("Subscribe to given url.")]
        [Aliases("sub", "add", "+")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task AddUrlFeedAsync(CommandContext ctx,
                                         [RemainingText, Description("URL.")] string url,
                                         [Description("Friendly name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidCommandUsageException("URL missing.");
            
            if (await ctx.Services.GetService<DatabaseService>().AddFeedAsync(ctx.Channel.Id, url, string.IsNullOrWhiteSpace(name) ? name : url).ConfigureAwait(false))
                await ctx.RespondAsync($"Subscribed to {url} !").ConfigureAwait(false);
            else
                await ctx.RespondAsync("Either URL you gave is invalid or you are already subscribed to it!").ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_FEED_LIST
        [Command("listsubs")]
        [Description("Get feed list for the current channel.")]
        [Aliases("ls", "list")]
        public async Task FeedListAsync(CommandContext ctx)
        {
            var feeds = await ctx.Services.GetService<DatabaseService>().GetSubscriptionsForChannelAsync(ctx.Channel.Id)
                .ConfigureAwait(false);
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = "Subscriptions for this channel:",
                Description = string.Join("\n", feeds.Select(fe => {
                    string qname = fe.Subscriptions.First().QualifiedName;
                    return (string.IsNullOrWhiteSpace(qname) ? fe.URL : qname) + $" (ID: {fe.Id})";
                })),
                Color = DiscordColor.Goldenrod
            }).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RSS_WM
        [Command("wm")]
        [Description("Get newest topics from WM forum.")]
        public async Task WmRssAsync(CommandContext ctx)
        {
            await SendFeedResultsAsync(ctx, FeedService.GetFeedResults("http://worldmafia.net/forum/forums/-/index.rss"))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RSS_NEWS
        [Command("news")]
        [Description("Get newest world news.")]
        public async Task NewsRssAsync(CommandContext ctx)
        {
            await SendFeedResultsAsync(ctx, FeedService.GetFeedResults("https://news.google.com/news/rss/headlines/section/topic/WORLD?ned=us&hl=en"))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RSS_UNSUBSCRIBE
        [Command("unsubscribe")]
        [Description("Remove an existing feed subscription.")]
        [Aliases("del", "d", "rm", "-", "unsub")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task UnsubscribeAsync(CommandContext ctx,
                                          [Description("ID.")] int id)
        {
            await ctx.Services.GetService<DatabaseService>().RemoveSubscriptionAsync(ctx.Channel.Id, id)
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Unsubscribed from feed with ID {Formatter.Bold(id.ToString())} !")
                .ConfigureAwait(false);
        }
        #endregion


        #region GROUP_RSS_REDDIT
        [Group("reddit")]
        [Description("Reddit feed manipulation.")]
        [Aliases("r")]
        public class CommandsRSSReddit : RSSModule
        {

            [GroupCommand]
            public new async Task ExecuteGroupAsync(CommandContext ctx,
                                                   [Description("Subreddit.")] string sub = "all")
            {
                if (string.IsNullOrWhiteSpace(sub))
                    throw new InvalidCommandUsageException("Subreddit missing.");

                string url = $"https://www.reddit.com/r/{ sub.ToLower() }/new/.rss";
                await SendFeedResultsAsync(ctx, FeedService.GetFeedResults(url))
                    .ConfigureAwait(false);
            }


            #region COMMAND_RSS_REDDIT_SUBSCRIBE
            [Command("subscribe")]
            [Description("Add new feed for a subreddit.")]
            [Aliases("add", "a", "+", "sub")]
            [RequirePermissions(Permissions.ManageGuild)]
            public async Task SubscribeAsync(CommandContext ctx,
                                            [Description("Subreddit.")] string sub)
            {
                if (string.IsNullOrWhiteSpace(sub))
                    throw new InvalidCommandUsageException("Subreddit missing.");

                string url = $"https://www.reddit.com/r/{ sub.ToLower() }/new/.rss";
                if (await ctx.Services.GetService<DatabaseService>().AddFeedAsync(ctx.Channel.Id, url, "/r/" + sub).ConfigureAwait(false))
                    await ctx.RespondAsync($"Subscribed to {Formatter.Bold("/r/" + sub)} !").ConfigureAwait(false);
                else
                    await ctx.RespondAsync("Either the subreddit you gave doesn't exist or you are already subscribed to it!").ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_RSS_REDDIT_UNSUBSCRIBE
            [Command("unsubscribe")]
            [Description("Remove a subreddit feed.")]
            [Aliases("del", "d", "rm", "-", "unsub")]
            [RequirePermissions(Permissions.ManageGuild)]
            public async Task UnsubscribeAsync(CommandContext ctx,
                                              [Description("Subreddit.")] string sub)
            {
                if (string.IsNullOrWhiteSpace(sub))
                    throw new InvalidCommandUsageException("Subreddit missing.");

                await ctx.Services.GetService<DatabaseService>().RemoveSubscriptionUsingNameAsync(ctx.Channel.Id, "/r/" + sub)
                    .ConfigureAwait(false);
                await ctx.RespondAsync($"Unsubscribed from {Formatter.Bold("/r/" + sub)} !")
                    .ConfigureAwait(false);
            }
            #endregion
        }
        #endregion

        #region GROUP_RSS_YOUTUBE
        [Group("youtube")]
        [Description("Youtube feed manipulation.")]
        [Aliases("yt", "y")]
        public class CommandsRSSYoutube : RSSModule
        {

            [GroupCommand]
            public new async Task ExecuteGroupAsync(CommandContext ctx,
                                                   [Description("Channel URL.")] string url)
            {
                if (string.IsNullOrWhiteSpace(url))
                    throw new InvalidCommandUsageException("Channel URL missing.");
                
                var ytid = await ctx.Services.GetService<YoutubeService>().GetYoutubeIdAsync(url)
                    .ConfigureAwait(false);
                var res = FeedService.GetFeedResults(YoutubeService.GetYoutubeRSSFeedLinkForChannelId(ytid));
                await SendFeedResultsAsync(ctx, res)
                    .ConfigureAwait(false);
            }


            #region COMMAND_RSS_YOUTUBE_SUBSCRIBE
            [Command("subscribe")]
            [Description("Add new feed for a YouTube channel.")]
            [Aliases("add", "a", "+", "sub")]
            [RequirePermissions(Permissions.ManageGuild)]
            public async Task SubscribeAsync(CommandContext ctx,
                                            [Description("Channel URL.")] string url,
                                            [Description("Friendly name.")] string name = null)
            {
                if (string.IsNullOrWhiteSpace(url))
                    throw new InvalidCommandUsageException("Channel URL missing.");

                var chid = await ctx.Services.GetService<YoutubeService>().GetYoutubeIdAsync(url)
                    .ConfigureAwait(false);

                if (chid == null)
                    throw new CommandFailedException("Failed retrieving channel ID for that URL.");

                var feedurl = YoutubeService.GetYoutubeRSSFeedLinkForChannelId(chid);
                bool nameset = string.IsNullOrWhiteSpace(name);
                if (await ctx.Services.GetService<DatabaseService>().AddFeedAsync(ctx.Channel.Id, feedurl, nameset ? name : url).ConfigureAwait(false))
                    await ctx.RespondAsync("Subscribed!").ConfigureAwait(false);
                else
                    await ctx.RespondAsync("Either the channel URL you is invalid or you are already subscribed to it!").ConfigureAwait(false);
            }
            #endregion
  
            #region COMMAND_RSS_YOUTUBE_UNSUBSCRIBE
            [Command("unsubscribe")]
            [Description("Remove a YouTube channel feed.")]
            [Aliases("del", "d", "rm", "-", "unsub")]
            [RequirePermissions(Permissions.ManageGuild)]
            public async Task UnsubscribeAsync(CommandContext ctx,
                                              [Description("Channel URL.")] string url)
            {
                if (string.IsNullOrWhiteSpace(url))
                    throw new InvalidCommandUsageException("Channel URL missing.");

                var chid = await ctx.Services.GetService<YoutubeService>().GetYoutubeIdAsync(url)
                    .ConfigureAwait(false);
                if (chid == null)
                    throw new CommandFailedException("Failed retrieving channel ID for that URL.");

                var feedurl = YoutubeService.GetYoutubeRSSFeedLinkForChannelId(chid);
                await ctx.Services.GetService<DatabaseService>().RemoveSubscriptionUsingUrlAsync(ctx.Channel.Id, feedurl)
                    .ConfigureAwait(false);
                await ctx.RespondAsync("Unsubscribed!")
                    .ConfigureAwait(false);
            }
            #endregion
        }
        #endregion


        #region HELPER_FUNCTIONS
        protected async Task SendFeedResultsAsync(CommandContext ctx, IEnumerable<SyndicationItem> results)
        {
            if (results == null)
                throw new CommandFailedException("Error getting RSS feed.");

            var em = new DiscordEmbedBuilder() {
                Title = "Topics active recently",
                Color = DiscordColor.Green
            };

            foreach (var res in results)
                em.AddField(res.Title.Text, res.Links[0].Uri.ToString());

            await ctx.RespondAsync(embed: em.Build())
                .ConfigureAwait(false);
        }
        #endregion
    }
}
