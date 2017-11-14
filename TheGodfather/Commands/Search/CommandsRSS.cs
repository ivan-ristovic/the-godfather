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
    [PreExecutionCheck]
    public class CommandsRSS
    {
        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                           [RemainingText, Description("URL.")] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidCommandUsageException("URL missing.");

            await SendFeedResultsAsync(ctx, ctx.Dependencies.GetDependency<FeedManager>().GetFeedResults(url))
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

            bool nameset = string.IsNullOrWhiteSpace(name);
            if (ctx.Dependencies.GetDependency<FeedManager>().TryAdd(ctx.Channel.Id, url, nameset ? name : url))
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
            var feeds = ctx.Dependencies.GetDependency<FeedManager>().GetFeedListForChannel(ctx.Channel.Id);
            await ctx.RespondAsync("Subscriptions for this channel:\n" + string.Join("\n", feeds))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RSS_WM
        [Command("wm")]
        [Description("Get newest topics from WM forum.")]
        public async Task WmRssAsync(CommandContext ctx)
        {
            await SendFeedResultsAsync(ctx, ctx.Dependencies.GetDependency<FeedManager>().GetFeedResults("http://worldmafia.net/forum/forums/-/index.rss"))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RSS_NEWS
        [Command("news")]
        [Description("Get newest world news.")]
        public async Task NewsRssAsync(CommandContext ctx)
        {
            await SendFeedResultsAsync(ctx, ctx.Dependencies.GetDependency<FeedManager>().GetFeedResults("https://news.google.com/news/rss/headlines/section/topic/WORLD?ned=us&hl=en"))
                .ConfigureAwait(false);
        }
        #endregion


        #region GROUP_RSS_REDDIT
        [Group("reddit", CanInvokeWithoutSubcommand = true)]
        [Description("Reddit feed manipulation.")]
        [Aliases("r")]
        public class CommandsRSSReddit : CommandsRSS
        {
            public new async Task ExecuteGroupAsync(CommandContext ctx,
                                                   [Description("Subreddit.")] string sub = "all")
            {
                if (string.IsNullOrWhiteSpace(sub))
                    throw new InvalidCommandUsageException("Subreddit missing.");

                string url = $"https://www.reddit.com/r/{ sub.ToLower() }/new/.rss";
                await SendFeedResultsAsync(ctx, ctx.Dependencies.GetDependency<FeedManager>().GetFeedResults(url))
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
                if (ctx.Dependencies.GetDependency<FeedManager>().TryAdd(ctx.Channel.Id, url, "/r/" + sub))
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

                if (ctx.Dependencies.GetDependency<FeedManager>().TryRemoveUsingQualified(ctx.Channel.Id, "/r/" + sub))
                    await ctx.RespondAsync($"Unsubscribed from {Formatter.Bold("/r/" + sub)} !").ConfigureAwait(false);
                else
                    await ctx.RespondAsync("Failed to remove some subscriptions!").ConfigureAwait(false);
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
                                                   [Description("Channel URL.")] string url)
            {
                if (string.IsNullOrWhiteSpace(url))
                    throw new InvalidCommandUsageException("Channel URL missing.");

                var ytid = await GetYoutubeIdAsync(ctx, url)
                    .ConfigureAwait(false);
                var res = ctx.Dependencies.GetDependency<FeedManager>().GetFeedResults(YoutubeRSSFeedLink(ytid));
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

                var chid = await GetYoutubeIdAsync(ctx, url)
                    .ConfigureAwait(false);
                if (chid == null)
                    throw new CommandFailedException("Failed retrieving channel ID for that URL.");

                var feedurl = YoutubeRSSFeedLink(chid);
                bool nameset = string.IsNullOrWhiteSpace(name);
                if (ctx.Dependencies.GetDependency<FeedManager>().TryAdd(ctx.Channel.Id, feedurl, nameset ? name : url))
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

                var chid = await GetYoutubeIdAsync(ctx, url)
                    .ConfigureAwait(false);
                if (chid == null)
                    throw new CommandFailedException("Failed retrieving channel ID for that URL.");

                var feedurl = YoutubeRSSFeedLink(chid);
                if (ctx.Dependencies.GetDependency<FeedManager>().TryRemove(ctx.Channel.Id, feedurl))
                    await ctx.RespondAsync("Unsubscribed!").ConfigureAwait(false);
                else
                    await ctx.RespondAsync("Failed to remove some subscriptions!").ConfigureAwait(false);
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
                    var ytkey = ctx.Dependencies.GetDependency<TheGodfather>().Config.YoutubeKey;
                    try {
                        var wc = new WebClient();
                        var jsondata = await wc.DownloadStringTaskAsync("https://www.googleapis.com/youtube/v3/channels?key=" + ytkey + "&forUsername=" + id + "&part=id")
                            .ConfigureAwait(false);
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
