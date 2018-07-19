#region USING_DIRECTIVES
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;
using TheGodfather.Services.Database;
using TheGodfather.Services.Database.Feeds;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("rss"), Module(ModuleType.Searches)]
    [Description("Commands for RSS feed querying or subscribing. If invoked without subcommand, gives the latest topic from the given RSS URL.")]
    [Aliases("feed")]
    [UsageExamples("!rss https://news.google.com/news/rss/")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class RSSModule : TheGodfatherModule
    {

        public RSSModule(DBService db) : base(db: db) { }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                           [RemainingText, Description("RSS URL.")] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidCommandUsageException("URL missing.");

            if (!RssService.IsValidFeedURL(url))
                throw new InvalidCommandUsageException("No results found for given URL (maybe forbidden?).");

            var res = RssService.GetFeedResults(url);
            if (res == null)
                throw new CommandFailedException("Error getting feed from given URL.");
            await RssService.SendFeedResultsAsync(ctx.Channel, res)
                .ConfigureAwait(false);
        }


        #region COMMAND_RSS_LIST
        [Command("list"), Module(ModuleType.Searches)]
        [Description("Get feed list for the current channel.")]
        [Aliases("ls", "listsubs", "listfeeds")]
        [UsageExamples("!feed list")]
        public async Task FeedListAsync(CommandContext ctx)
        {
            var subs = await Database.GetFeedEntriesForChannelAsync(ctx.Channel.Id)
                .ConfigureAwait(false);

            await ctx.SendCollectionInPagesAsync(
                "Subscriptions for this channel:",
                subs,
                fe => {
                    string qname = fe.Subscriptions.First().QualifiedName;
                    return (string.IsNullOrWhiteSpace(qname) ? fe.Url : qname) + $" (ID: {fe.Id})";
                },
                DiscordColor.Goldenrod
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RSS_SUBSCRIBE
        [Command("subscribe"), Module(ModuleType.Searches)]
        [Description("Subscribe to given RSS feed URL. The bot will send a message when the latest topic is changed.")]
        [Aliases("sub", "add", "+")]
        [UsageExamples("!rss subscribe https://news.google.com/news/rss/",
                       "!rss subscribe https://news.google.com/news/rss/ news")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddUrlFeedAsync(CommandContext ctx,
                                         [Description("URL.")] string url,
                                         [Description("Friendly name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidCommandUsageException("URL missing.");

            if (!RssService.IsValidFeedURL(url))
                throw new InvalidCommandUsageException("Given URL isn't a valid RSS feed URL.");

            if (!await Database.TryAddSubscriptionAsync(ctx.Channel.Id, url, name ?? url).ConfigureAwait(false))
                throw new CommandFailedException("You are already subscribed to this RSS feed URL!");

            await ctx.InformSuccessAsync($"Subscribed to {url}!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RSS_UNSUBSCRIBE
        [Command("unsubscribe"), Priority(1)]
        [Module(ModuleType.Searches)]
        [Description("Remove an existing feed subscription.")]
        [Aliases("del", "d", "rm", "-", "unsub")]
        [UsageExamples("!rss unsubscribe 1")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task UnsubscribeAsync(CommandContext ctx,
                                          [Description("ID of the subscription.")] int id)
        {
            await Database.RemoveSubscriptionByIdAsync(ctx.Channel.Id, id)
                .ConfigureAwait(false);
            await ctx.InformSuccessAsync($"Unsubscribed from feed with ID {Formatter.Bold(id.ToString())}")
                .ConfigureAwait(false);
        }

        [Command("unsubscribe"), Priority(0)]
        public async Task UnsubscribeAsync(CommandContext ctx,
                                          [Description("Name of the subscription.")] string name)
        {
            await Database.RemoveSubscriptionByNameAsync(ctx.Channel.Id, name)
                .ConfigureAwait(false);
            await ctx.InformSuccessAsync($"Unsubscribed from feed with name {Formatter.Bold(name)}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RSS_WM
        [Command("wm"), Module(ModuleType.Searches)]
        [Description("Get newest topics from WM forum.")]
        [UsageExamples("!rss wm")]
        public async Task WmRssAsync(CommandContext ctx)
        {
            var res = RssService.GetFeedResults("http://worldmafia.net/forum/forums/-/index.rss");
            if (res == null)
                throw new CommandFailedException("An error occured while reaching WM forum. Possibly Pakistani didn't pay this month?");
            await RssService.SendFeedResultsAsync(ctx.Channel, res)
                .ConfigureAwait(false);
        }
        #endregion
    }
}
