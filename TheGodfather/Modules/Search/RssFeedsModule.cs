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
    [Module(ModuleType.Searches), NotBlocked]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class RssFeedsModule : TheGodfatherModule
    {

        public RssFeedsModule(SharedData shared, DBService db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Orange;
        }


        #region COMMAND_RSS
        [Command("rss")]
        [Description("Get the latest topics from the given RSS feed URL.")]
        [Aliases("feed")]
        [UsageExamples("!rss https://news.google.com/news/rss/")]
        public Task RssAsync(CommandContext ctx,
                            [Description("RSS feed URL.")] Uri url)
        {
            if (!RssService.IsValidFeedURL(url.AbsoluteUri))
                throw new InvalidCommandUsageException("No results found for given URL (maybe forbidden?).");

            IReadOnlyList<SyndicationItem> res = RssService.GetFeedResults(url.AbsoluteUri);
            if (res == null)
                throw new CommandFailedException("Error getting feed from given URL.");

            return RssService.SendFeedResultsAsync(ctx.Channel, res);
        }
        #endregion


        #region GROUP_SUBSCRIBE
        [Group("subscribe")]
        [Description("Commands for managing feed subscriptions. The bot will send a message when the latest topic " +
                     "is changed. Group call subscribes the bot to the given RSS feed URL or lists active subs.")]
        [Aliases("sub", "add", "+", "subscriptions", "subscription")]
        [UsageExamples("!subscribe https://news.google.com/news/rss/",
                       "!subscribe https://news.google.com/news/rss/ news")]
        [RequireOwnerOrPermissions(Permissions.ManageGuild)]
        public class SubscribeModule : TheGodfatherModule
        {

            public SubscribeModule(SharedData shared, DBService db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Orange;
            }


            [GroupCommand, Priority(1)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("URL.")] Uri url,
                                               [RemainingText, Description("Friendly name.")] string name = null)
            {
                if (!RssService.IsValidFeedURL(url.AbsoluteUri))
                    throw new InvalidCommandUsageException("Given URL isn't a valid RSS feed URL.");

                if (!await this.Database.TryAddSubscriptionAsync(ctx.Channel.Id, url.AbsoluteUri, name ?? url.AbsoluteUri))
                    throw new CommandFailedException("You are already subscribed to this RSS feed URL!");

                await this.InformAsync(ctx, $"Subscribed to {url}!", important: false);
            }

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => this.ListAsync(ctx);


            #region COMMAND_SUBSCRIBE_LIST
            [Command("list")]
            [Description("Get feed list for the current channel.")]
            [Aliases("ls", "listsubs", "listfeeds")]
            [UsageExamples("!subscribe list")]
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
        }
        #endregion

        #region GROUP_UNSUBSCRIBE
        [Group("unsubscribe")]
        [Description("Remove an existing feed subscription.")]
        [Aliases("del", "d", "rm", "-", "unsub")]
        [UsageExamples("!unsubscribe 1")]
        [RequireOwnerOrPermissions(Permissions.ManageGuild)]
        public class UnsubscribeModule : TheGodfatherModule
        {
            public UnsubscribeModule(SharedData shared, DBService db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Orange;
            }


            [GroupCommand, Priority(1)]
            public async Task UnsubscribeAsync(CommandContext ctx,
                                              [Description("ID of the subscriptions to remove.")] params int[] ids)
            {
                if (!ids.Any())
                    throw new CommandFailedException("Missing IDs of the subscriptions to remove!");

                await this.Database.RemoveSubscriptionByIdAsync(ctx.Channel.Id, ids);
                await this.InformAsync(ctx, $"Unsubscribed from feed with IDs {Formatter.Bold(string.Join(", ", ids))}", important: false);
            }

            [GroupCommand, Priority(0)]
            public async Task UnsubscribeAsync(CommandContext ctx,
                                              [RemainingText, Description("Name of the subscription.")] string name)
            {
                await this.Database.RemoveSubscriptionByNameAsync(ctx.Channel.Id, name);
                await this.InformAsync(ctx, $"Unsubscribed from feed with name {Formatter.Bold(name)}", important: false);
            }
        }
        #endregion
    }
}
