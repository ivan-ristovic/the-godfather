#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Extensions;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfather.Modules.Search
{
    [Module(ModuleType.Searches), NotBlocked]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class RssFeedsModule : TheGodfatherModule
    {

        public RssFeedsModule(DbContextBuilder db)
            : base(db)
        {

        }


        #region COMMAND_RSS
        [Command("rss")]
        [Description("Get the latest topics from the given RSS feed URL.")]
        [Aliases("feed")]

        public Task RssAsync(CommandContext ctx,
                            [Description("RSS feed URL.")] Uri url)
        {
            if (!RssService.IsValidFeedURL(url.AbsoluteUri))
                throw new InvalidCommandUsageException("No results found for given URL (maybe forbidden?).");

            IReadOnlyList<SyndicationItem> res = RssService.GetFeedResults(url.AbsoluteUri);
            if (res is null)
                throw new CommandFailedException("Error getting feed from given URL.");

            return RssService.SendFeedResultsAsync(ctx.Channel, res);
        }
        #endregion


        #region GROUP_SUBSCRIBE
        [Group("subscribe")]
        [Description("Commands for managing feed subscriptions. The bot will send a message when the latest topic " +
                     "is changed. Group call subscribes the bot to the given RSS feed URL or lists active subs.")]
        [Aliases("sub", "subscriptions", "subscription")]

        [RequireOwnerOrPermissions(Permissions.ManageGuild)]
        public class SubscribeModule : TheGodfatherModule
        {

            public SubscribeModule(DbContextBuilder db)
                : base(db)
            {

            }


            [GroupCommand, Priority(1)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("URL.")] Uri url,
                                               [RemainingText, Description("Friendly name.")] string name = null)
            {
                if (!RssService.IsValidFeedURL(url.AbsoluteUri))
                    throw new InvalidCommandUsageException("Given URL isn't a valid RSS feed URL.");

                await this.Database.SubscribeAsync(ctx.Guild.Id, ctx.Channel.Id, url.AbsoluteUri, name);
                await this.InformAsync(ctx, $"Subscribed to {url}!", important: false);
            }

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => this.ListAsync(ctx);


            #region COMMAND_SUBSCRIBE_LIST
            [Command("list")]
            [Description("Get feed list for the current channel.")]
            [Aliases("ls", "listsubs", "listfeeds")]
            public async Task ListAsync(CommandContext ctx)
            {
                IReadOnlyList<RssSubscription> subs;
                using (TheGodfatherDbContext db = this.Database.CreateContext())
                    subs = await db.RssSubscriptions.Where(s => s.ChannelId == ctx.Channel.Id).ToListAsync();

                if (!subs.Any())
                    throw new CommandFailedException("No subscriptions found in this channel");

                await ctx.SendCollectionInPagesAsync(
                    "Subscriptions for this channel",
                    subs,
                    sub => {
                        string qname = sub.Name;
                        return $"{Formatter.InlineCode($"{sub.Id:D4}")} | {(string.IsNullOrWhiteSpace(qname) ? sub.Feed.Url : qname)}";
                    },
                    this.ModuleColor
                );
            }
            #endregion

            #region COMMAND_SUBSCRIBE_REDDIT
            [Command("reddit")]
            [Description("Add new subscription for a subreddit.")]
            [Aliases("r")]

            public async Task RedditAsync(CommandContext ctx,
                                         [Description("Subreddit.")] string sub)
            {
                string url = RedditService.GetFeedURLForSubreddit(sub, RedditCategory.New, out string rsub);
                if (url is null)
                    throw new CommandFailedException("That subreddit doesn't exist.");

                await this.Database.SubscribeAsync(ctx.Guild.Id, ctx.Channel.Id, url, rsub);
                await this.InformAsync(ctx, $"Subscribed to {Formatter.Bold(rsub)}", important: false);
            }
            #endregion

            #region COMMAND_SUBSCRIBE_YOUTUBE
            [Command("youtube")]
            [Description("Add a new subscription for a YouTube channel.")]
            [Aliases("y", "yt", "ytube")]

            public async Task SubscribeAsync(CommandContext ctx,
                                            [Description("Channel URL.")] string url,
                                            [Description("Friendly name.")] string name = null)
            {
                string chid = await ctx.Services.GetService<YtService>().ExtractChannelIdAsync(url);
                if (chid is null)
                    throw new CommandFailedException("Failed retrieving channel ID for that URL.");

                string feedurl = YtService.GetRssUrlForChannel(chid);
                await this.Database.SubscribeAsync(ctx.Guild.Id, ctx.Channel.Id, feedurl, string.IsNullOrWhiteSpace(name) ? url : name);
                await this.InformAsync(ctx, "Subscribed!", important: false);
            }
            #endregion
        }
        #endregion

        #region GROUP_UNSUBSCRIBE
        [Group("unsubscribe")]
        [Description("Remove an existing feed subscription.")]
        [Aliases("unsub")]

        [RequireOwnerOrPermissions(Permissions.ManageGuild)]
        public class UnsubscribeModule : TheGodfatherModule
        {

            public UnsubscribeModule(DbContextBuilder db)
                : base(db)
            {

            }


            [GroupCommand, Priority(1)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("ID of the subscriptions to remove.")] params int[] ids)
            {
                if (ids is null || !ids.Any())
                    throw new CommandFailedException("Missing IDs of the subscriptions to remove!");

                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    db.RssSubscriptions.RemoveRange(db.RssSubscriptions.Where(s => s.GuildId == ctx.Guild.Id && s.ChannelId == ctx.Channel.Id && ids.Contains(s.Id)));
                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, $"Unsubscribed from feed with IDs {Formatter.Bold(string.Join(", ", ids))}", important: false);
            }

            [GroupCommand, Priority(0)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [RemainingText, Description("Name of the subscription.")] string name)
            {
                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    RssSubscription sub = db.RssSubscriptions.SingleOrDefault(s => s.GuildId == ctx.Guild.Id && s.ChannelId == ctx.Channel.Id && s.Name == name);
                    if (sub == null)
                        throw new CommandFailedException("Not subscribed to a feed with that name!");
                    db.RssSubscriptions.Remove(sub);
                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, $"Unsubscribed from feed with name {Formatter.Bold(name)}", important: false);
            }


            #region COMMAND_UNSUBSCRIBE_ALL
            [Command("all"), UsesInteractivity]
            [Description("Remove all subscriptions for the given channel.")]
            [Aliases("a")]
            public async Task AllAsync(CommandContext ctx,
                                      [Description("Channel.")] DiscordChannel channel = null)
            {
                channel = channel ?? ctx.Channel;

                if (!await ctx.WaitForBoolReplyAsync($"Are you sure you want to remove all subscriptions for channel {channel.Mention}?"))
                    return;

                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    db.RssSubscriptions.RemoveRange(db.RssSubscriptions.Where(s => s.ChannelId == ctx.Guild.Id));
                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, $"Removed all subscriptions for channel {channel.Mention}!", important: false);
            }
            #endregion

            #region COMMAND_UNSUBSCRIBE_REDDIT
            [Command("reddit")]
            [Description("Remove a subscription using subreddit name or subscription ID (use command ``subscriptions list`` to see IDs).")]
            [Aliases("r")]

            public async Task RedditAsync(CommandContext ctx,
                                         [Description("Subreddit.")] string sub)
            {
                if (RedditService.GetFeedURLForSubreddit(sub, RedditCategory.New, out string rsub) is null)
                    throw new CommandFailedException("That subreddit doesn't exist.");

                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    db.RssSubscriptions.RemoveRange(db.RssSubscriptions.Where(s => s.GuildId == ctx.Guild.Id && s.ChannelId == ctx.Channel.Id && s.Name == rsub));
                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, $"Unsubscribed from {Formatter.Bold(rsub)}", important: false);
            }
            #endregion

            #region COMMAND_UNSUBSCRIBE_YOUTUBE
            [Command("youtube")]
            [Description("Remove a YouTube channel subscription.")]
            [Aliases("y", "yt", "ytube")]

            public async Task UnsubscribeAsync(CommandContext ctx,
                                              [Description("Channel URL or subscription name.")] string name_url)
            {
                if (string.IsNullOrWhiteSpace(name_url))
                    throw new InvalidCommandUsageException("Channel URL missing.");

                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    db.RssSubscriptions.RemoveRange(db.RssSubscriptions.Where(s => s.GuildId == ctx.Guild.Id && s.ChannelId == ctx.Channel.Id && s.Name == name_url));
                    await db.SaveChangesAsync();
                }

                string chid = await ctx.Services.GetService<YtService>().ExtractChannelIdAsync(name_url);
                if (!(chid is null)) {
                    string feedurl = YtService.GetRssUrlForChannel(chid);
                    using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                        RssSubscription sub = db.RssSubscriptions
                            .SingleOrDefault(s => s.ChannelId == ctx.Channel.Id && s.Feed.Url == feedurl);
                        if (!(sub is null)) {
                            db.RssSubscriptions.Remove(sub);
                            await db.SaveChangesAsync();
                        }
                    }
                }

                await this.InformAsync(ctx, "Unsubscribed!", important: false);
            }
            #endregion
        }
        #endregion
    }
}
