#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Extensions;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("rss"), Module(ModuleType.Searches), NotBlocked]
    [Description("Commands for RSS feed querying or subscribing. Group call prints the latest " +
                 "topics from the given RSS URL or prints the subscriptions for the given channel.")]
    [Aliases("feed")]
    [UsageExamples("!rss https://news.google.com/news/rss/")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class RSSModule : TheGodfatherModule
    {

        public RSSModule(SharedData shared, DBService db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Orange;
        }


        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx, 
                                     [Description("RSS URL.")] Uri url)
        {
            if (!RssService.IsValidFeedURL(url.AbsoluteUri))
                throw new InvalidCommandUsageException("No results found for given URL (maybe forbidden?).");

            IReadOnlyList<SyndicationItem> res = RssService.GetFeedResults(url.AbsoluteUri);
            if (res == null)
                throw new CommandFailedException("Error getting feed from given URL.");

            return RssService.SendFeedResultsAsync(ctx.Channel, res);
        }


        #region COMMAND_RSS_LIST
        [Command("list")]
        [Description("Get feed list for the current channel.")]
        [Aliases("ls", "listsubs", "listfeeds")]
        [UsageExamples("!feed list")]
        public async Task ListAsync(CommandContext ctx)
        {
            IReadOnlyList<FeedEntry> subs = await this.Database.GetFeedEntriesForChannelAsync(ctx.Channel.Id);
            if (!subs.Any())
                throw new CommandFailedException("No subscriptions found in this channel");

            await ctx.SendCollectionInPagesAsync(
                "Subscriptions for this channel",
                subs,
                fe => {
                    string qname = fe.Subscriptions.First().QualifiedName;
                    return $"{Formatter.InlineCode($"{fe.Id:D4}")} | {(string.IsNullOrWhiteSpace(qname) ? fe.Url : qname)}";
                },
                this.ModuleColor
            );
        }
        #endregion

        #region COMMAND_RSS_SUBSCRIBE
        [Command("subscribe")]
        [Description("Subscribe to given RSS feed URL. The bot will send a message when the latest topic is changed.")]
        [Aliases("sub", "add", "+")]
        [UsageExamples("!rss subscribe https://news.google.com/news/rss/",
                       "!rss subscribe https://news.google.com/news/rss/ news")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddUrlFeedAsync(CommandContext ctx,
                                         [Description("URL.")] Uri url,
                                         [RemainingText, Description("Friendly name.")] string name = null)
        {
            if (!RssService.IsValidFeedURL(url.AbsoluteUri))
                throw new InvalidCommandUsageException("Given URL isn't a valid RSS feed URL.");

            if (!await this.Database.TryAddSubscriptionAsync(ctx.Channel.Id, url.AbsoluteUri, name ?? url.AbsoluteUri))
                throw new CommandFailedException("You are already subscribed to this RSS feed URL!");

            await this.InformAsync(ctx, $"Subscribed to {url}!", important: false);
        }
        #endregion

        #region COMMAND_RSS_UNSUBSCRIBE
        [Command("unsubscribe"), Priority(1)]
        [Description("Remove an existing feed subscription.")]
        [Aliases("del", "d", "rm", "-", "unsub")]
        [UsageExamples("!rss unsubscribe 1")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task UnsubscribeAsync(CommandContext ctx,
                                          [Description("ID of the subscriptions to remove.")] params int[] ids)
        {
            if (!ids.Any())
                throw new CommandFailedException("Missing IDs of the subscriptions to remove!");

            await this.Database.RemoveSubscriptionByIdAsync(ctx.Channel.Id, ids);
            await this.InformAsync(ctx, $"Unsubscribed from feed with IDs {Formatter.Bold(string.Join(", ", ids))}", important: false);
        }

        [Command("unsubscribe"), Priority(0)]
        public async Task UnsubscribeAsync(CommandContext ctx,
                                          [RemainingText, Description("Name of the subscription.")] string name)
        {
            await this.Database.RemoveSubscriptionByNameAsync(ctx.Channel.Id, name);
            await this.InformAsync(ctx, $"Unsubscribed from feed with name {Formatter.Bold(name)}", important: false);
        }
        #endregion

        #region COMMAND_RSS_WM
        [Command("wm")]
        [Description("Get newest topics from WM forum.")]
        [UsageExamples("!rss wm")]
        public Task WmRssAsync(CommandContext ctx)
        {
            IReadOnlyList<SyndicationItem> res = RssService.GetFeedResults("http://worldmafia.net/forum/forums/-/index.rss");
            if (res == null)
                throw new CommandFailedException("An error occured while reaching WM forum. Possibly Pakistani didn't pay this month?");

            return RssService.SendFeedResultsAsync(ctx.Channel, res);
        }
        #endregion

        // TODO unsub all

        // TODO better sub/unsub system ... 
    }
}
