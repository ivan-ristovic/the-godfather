#region USING_DIRECTIVES
using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Helpers.DataManagers;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Search
{
    [Group("rss", CanInvokeWithoutSubcommand = true)]
    [Description("RSS feed operations.")]
    [Aliases("feed")]
    [Cooldown(2, 5, CooldownBucketType.User), Cooldown(4, 5, CooldownBucketType.Channel)]
    public class CommandsRSS
    {
        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                           [RemainingText, Description("URL.")] string url = null)
        {
            if (string.IsNullOrWhiteSpace(url)) {
                await WMRSS(ctx);
            } else {
                await SendFeedResults(ctx, ctx.Dependencies.GetDependency<FeedManager>().GetFeedResults(url));
            }
        }


        #region COMMAND_SUBSCRIBE
        [Command("subscribe")]
        [Description("Subscribe to given url.")]
        [Aliases("sub", "add", "+")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task AddURLFeed(CommandContext ctx,
                                    [RemainingText, Description("URL.")] string url = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidCommandUsageException("URL missing.");

            if (ctx.Dependencies.GetDependency<FeedManager>().TryAdd(ctx.Channel.Id, url))
                await ctx.RespondAsync($"Subscribed to {url} !");
            else
                await ctx.RespondAsync("Either URL you gave is invalid or you are already subscribed to it!");
        }
        #endregion

        #region COMMAND_FEED_LIST
        [Command("listfeeds")]
        [Description("Get feed list for the current channel.")]
        [Aliases("listsubs", "listfeed", "lf", "list")]
        public async Task FeedList(CommandContext ctx)
        {
            var feeds = ctx.Dependencies.GetDependency<FeedManager>().GetFeedListForChannel(ctx.Channel.Id);
            await ctx.RespondAsync("Subscriptions for this channel:\n" + string.Join("\n", feeds));
        }
        #endregion

        #region COMMAND_RSS_WM
        [Command("wm")]
        [Description("Get newest topics from WM forum.")]
        public async Task WMRSS(CommandContext ctx)
        {
            await SendFeedResults(ctx, ctx.Dependencies.GetDependency<FeedManager>().GetFeedResults("http://worldmafia.net/forum/forums/-/index.rss"));
        }
        #endregion

        #region COMMAND_RSS_NEWS
        [Command("news")]
        [Description("Get newest world news.")]
        public async Task NewsRSS(CommandContext ctx)
        {
            await SendFeedResults(ctx, ctx.Dependencies.GetDependency<FeedManager>().GetFeedResults("https://news.google.com/news/rss/headlines/section/topic/WORLD?ned=us&hl=en"));
        }
        #endregion


        #region GROUP_RSS_REDDIT
        [Group("reddit", CanInvokeWithoutSubcommand = true)]
        [Description("Reddit feed manipulation.")]
        [Aliases("subreddit", "r")]
        public class CommandsRSSReddit : CommandsRSS
        {
            public new async Task ExecuteGroupAsync(CommandContext ctx,
                                                   [Description("Subreddit.")] string sub = null)
            {
                if (string.IsNullOrWhiteSpace(sub))
                    throw new InvalidCommandUsageException("Subreddit missing.");

                string url = $"https://www.reddit.com/r/{ sub.ToLower() }/new/.rss";
                await SendFeedResults(ctx, ctx.Dependencies.GetDependency<FeedManager>().GetFeedResults(url));
            }


            #region COMMAND_RSS_REDDIT_ADD
            [Command("add")]
            [Description("Add new feed for a subreddit.")]
            [Aliases("a", "+", "sub", "subscribe")]
            [RequirePermissions(Permissions.ManageChannels)]
            public async Task AddSubreddit(CommandContext ctx,
                                          [Description("Subreddit.")] string sub = null)
            {
                if (string.IsNullOrWhiteSpace(sub))
                    throw new InvalidCommandUsageException("Subreddit missing.");

                string url = $"https://www.reddit.com/r/{ sub.ToLower() }/new/.rss";
                if (ctx.Dependencies.GetDependency<FeedManager>().TryAdd(ctx.Channel.Id, url, "/r/" + sub))
                    await ctx.RespondAsync($"Subscribed to {Formatter.Bold("/r/" + sub)} !");
                else
                    await ctx.RespondAsync("Either the subreddit you gave doesn't exist or you are already subscribed to it!");
            }
            #endregion

            #region COMMAND_RSS_REDDIT_REMOVE
            [Command("remove")]
            [Description("Remove a subreddit feed.")]
            [Aliases("delete", "d", "rem", "-", "unsub", "unsubscribe")]
            [RequirePermissions(Permissions.ManageChannels)]
            public async Task DelSubreddit(CommandContext ctx,
                                          [Description("Subreddit.")] string sub = null)
            {
                if (string.IsNullOrWhiteSpace(sub))
                    throw new InvalidCommandUsageException("Subreddit missing.");

                if (ctx.Dependencies.GetDependency<FeedManager>().TryRemoveUsingQualified(ctx.Channel.Id, "/r/" + sub))
                    await ctx.RespondAsync($"Unsubscribed from {Formatter.Bold("/r/" + sub)} !");
                else
                    await ctx.RespondAsync("Failed to remove some subscriptions!");
            }
            #endregion
        }
        #endregion

        #region GROUP_RSS_YOUTUBE
        [Group("youtube", CanInvokeWithoutSubcommand = true)]
        [Description("Youtube feed manipulation.")]
        [Aliases("yt", "y")]
        public class CommandsRSSYoutube : CommandsRSS
        {
            public new async Task ExecuteGroupAsync(CommandContext ctx,
                                                   [Description("Channel URL.")] string url = null)
            {
                if (string.IsNullOrWhiteSpace(url))
                    throw new InvalidCommandUsageException("Channel URL missing.");

                var ytid = await GetYoutubeIdAsync(ctx, url);
                var res = ctx.Dependencies.GetDependency<FeedManager>().GetFeedResults(YoutubeRSSFeedLink(ytid));
                await SendFeedResults(ctx, res);
            }


            #region COMMAND_RSS_YOUTUBE_ADD
            [Command("add")]
            [Description("Add new feed for a YouTube channel.")]
            [Aliases("a", "+", "sub", "subscribe")]
            [RequirePermissions(Permissions.ManageChannels)]
            public async Task AddChannelFeed(CommandContext ctx,
                                            [Description("Channel URL.")] string url = null)
            {
                if (string.IsNullOrWhiteSpace(url))
                    throw new InvalidCommandUsageException("Channel URL missing.");

                var chid = await GetYoutubeIdAsync(ctx, url);
                if (chid == null)
                    throw new CommandFailedException("Failed retrieving channel ID for that URL.");

                var feedurl = YoutubeRSSFeedLink(chid);
                if (ctx.Dependencies.GetDependency<FeedManager>().TryAdd(ctx.Channel.Id, feedurl))
                    await ctx.RespondAsync($"Subscribed!");
                else
                    await ctx.RespondAsync("Either the channel URL you is invalid or you are already subscribed to it!");
            }
            #endregion
  
            #region COMMAND_RSS_YOUTUBE_REMOVE
            [Command("remove")]
            [Description("Remove a YouTube channel feed.")]
            [Aliases("delete", "d", "rem", "-", "unsub", "unsubscribe")]
            [RequirePermissions(Permissions.ManageChannels)]
            public async Task DelYoutubeFeed(CommandContext ctx,
                                            [Description("Channel URL.")] string url = null)
            {
                if (string.IsNullOrWhiteSpace(url))
                    throw new InvalidCommandUsageException("Channel URL missing.");

                var chid = await GetYoutubeIdAsync(ctx, url);
                if (chid == null)
                    throw new CommandFailedException("Failed retrieving channel ID for that URL.");

                var feedurl = YoutubeRSSFeedLink(chid);
                if (ctx.Dependencies.GetDependency<FeedManager>().TryRemove(ctx.Channel.Id, feedurl))
                    await ctx.RespondAsync($"Unsubscribed!");
                else
                    await ctx.RespondAsync("Failed to remove some subscriptions!");
            }
            #endregion
          

            #region HELPER_FUNCTIONS_AND_CLASSES
            private string YoutubeRSSFeedLink(string id)
            {
                return "https://www.youtube.com/feeds/videos.xml?channel_id=" + id;
            }

            private async Task<string> GetYoutubeIdAsync(CommandContext ctx, string url)
            {
                string id = url.Split('/').Last();

                var results = ctx.Dependencies.GetDependency<FeedManager>().GetFeedResults(YoutubeRSSFeedLink(id));
                if (results == null) {
                    var ytkey = ctx.Dependencies.GetDependency<BotConfigManager>().CurrentConfig.YoutubeKey;
                    try {
                        var wc = new WebClient();
                        var jsondata = await wc.DownloadStringTaskAsync("https://www.googleapis.com/youtube/v3/channels?key=" + ytkey + "&forUsername=" + id + "&part=id");
                        var data = JsonConvert.DeserializeObject<DeserializedData>(jsondata);
                        if (data.Items != null)
                            return data.Items[0]["id"];
                    } catch {

                    }
                } else {
                    return id;
                }

                return null;
            }


            private sealed class DeserializedData
            {
                [JsonProperty("items")]
                public List<Dictionary<string, string>> Items { get; set; }
            }
            #endregion
        }
        #endregion


        #region HELPER_FUNCTIONS
        protected async Task SendFeedResults(CommandContext ctx, IEnumerable<SyndicationItem> results)
        {
            if (results == null)
                throw new CommandFailedException("Error getting RSS feed.");

            var embed = new DiscordEmbedBuilder() {
                Title = "Topics active recently",
                Color = DiscordColor.Green
            };

            foreach (var res in results)
                embed.AddField(res.Title.Text, res.Links[0].Uri.ToString());

            await ctx.RespondAsync(embed: embed);
        }
        #endregion
    }
}
