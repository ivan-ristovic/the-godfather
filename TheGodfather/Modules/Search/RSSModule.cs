#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("rss")]
    [Description("Commands for RSS feed querying or subscribing. If invoked without subcommand, gives the latest topic from the given RSS URL.")]
    [Aliases("feed")]
    [UsageExample("!rss https://news.google.com/news/rss/")]
    [Cooldown(2, 5, CooldownBucketType.User), Cooldown(2, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class RSSModule : TheGodfatherBaseModule
    {

        public RSSModule(DBService db) : base(db: db) { }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                           [RemainingText, Description("RSS URL.")] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidCommandUsageException("URL missing.");

            if (!RSSService.IsValidRSSFeedURL(url))
                throw new InvalidCommandUsageException("No results found for given URL (maybe forbidden?).");

            var res = RSSService.GetFeedResults(url);
            if (res == null)
                throw new CommandFailedException("Error getting feed from given URL.");
            await RSSService.SendFeedResultsAsync(ctx.Channel, res)
                .ConfigureAwait(false);
        }


        #region COMMAND_RSS_LIST
        [Command("list")]
        [Description("Get feed list for the current channel.")]
        [Aliases("ls", "listsubs", "listfeeds")]
        [UsageExample("!feed list")]
        public async Task FeedListAsync(CommandContext ctx)
        {
            var subs = await Database.GetFeedEntriesForChannelAsync(ctx.Channel.Id)
                .ConfigureAwait(false);

            await ctx.SendPaginatedCollectionAsync(
                "Subscriptions for this channel:",
                subs,
                fe => {
                    string qname = fe.Subscriptions.First().QualifiedName;
                    return (string.IsNullOrWhiteSpace(qname) ? fe.URL : qname) + $" (ID: {fe.Id})";
                },
                DiscordColor.Goldenrod
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RSS_SUBSCRIBE
        [Command("subscribe")]
        [Description("Subscribe to given RSS feed URL. The bot will send a message when the latest topic is changed.")]
        [Aliases("sub", "add", "+")]
        [UsageExample("!rss subscribe https://news.google.com/news/rss/")]
        [UsageExample("!rss subscribe https://news.google.com/news/rss/ news")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task AddUrlFeedAsync(CommandContext ctx,
                                         [Description("URL.")] string url,
                                         [Description("Friendly name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidCommandUsageException("URL missing.");

            if (!RSSService.IsValidRSSFeedURL(url))
                throw new InvalidCommandUsageException("Given URL isn't a valid RSS feed URL.");

            if (!await Database.AddSubscriptionAsync(ctx.Channel.Id, url, name ?? url).ConfigureAwait(false))
                throw new CommandFailedException("You are already subscribed to this RSS feed URL!");

            await ctx.RespondWithIconEmbedAsync($"Subscribed to {url}!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RSS_UNSUBSCRIBE
        [Command("unsubscribe"), Priority(1)]
        [Description("Remove an existing feed subscription.")]
        [Aliases("del", "d", "rm", "-", "unsub")]
        [UsageExample("!rss unsubscribe 1")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task UnsubscribeAsync(CommandContext ctx,
                                          [Description("ID of the subscription.")] int id)
        {
            await Database.RemoveSubscriptionByIdAsync(ctx.Channel.Id, id)
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"Unsubscribed from feed with ID {Formatter.Bold(id.ToString())}")
                .ConfigureAwait(false);
        }

        [Command("unsubscribe"), Priority(0)]
        public async Task UnsubscribeAsync(CommandContext ctx,
                                          [Description("Name of the subscription.")] string name)
        {
            await Database.RemoveSubscriptionByNameAsync(ctx.Channel.Id, name)
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"Unsubscribed from feed with name {Formatter.Bold(name)}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RSS_WM
        [Command("wm")]
        [Description("Get newest topics from WM forum.")]
        [UsageExample("!rss wm")]
        public async Task WmRssAsync(CommandContext ctx)
        {
            var res = RSSService.GetFeedResults("http://worldmafia.net/forum/forums/-/index.rss");
            if (res == null)
                throw new CommandFailedException("An error occured while reaching WM forum. Possibly Pakistani didn't pay this month?");
            await RSSService.SendFeedResultsAsync(ctx.Channel, res)
                .ConfigureAwait(false);
        }
        #endregion
    }
}
