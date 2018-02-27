#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("reddit")]
    [Description("Reddit commands.")]
    [Aliases("r")]
    [UsageExample("!reddit aww")]
    [Cooldown(2, 5, CooldownBucketType.User), Cooldown(4, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class RedditModule : TheGodfatherBaseModule
    {

        public RedditModule(DBService db) : base(db: db) { }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Subreddit.")] string sub = "all")
        {
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidCommandUsageException("Subreddit missing.");

            var url = RSSService.GetRedditFeedURLForSubreddit(sub, out string rsub);
            if (url == null)
                throw new CommandFailedException("That subreddit doesn't exist.");

            var res = RSSService.GetFeedResults(url);
            if (res == null)
                throw new CommandFailedException($"Failed to get the data from that subreddit ({Formatter.Bold(rsub)}).");
            await RSSService.SendFeedResultsAsync(ctx.Channel, res)
                .ConfigureAwait(false);
        }


        #region COMMAND_RSS_REDDIT_SUBSCRIBE
        [Command("subscribe")]
        [Description("Add new feed for a subreddit.")]
        [Aliases("add", "a", "+", "sub")]
        [UsageExample("!reddit sub aww")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task SubscribeAsync(CommandContext ctx,
                                        [Description("Subreddit.")] string sub)
        {
            var url = RSSService.GetRedditFeedURLForSubreddit(sub, out string rsub);
            if (url == null)
                throw new CommandFailedException("That subreddit doesn't exist.");

            if (!await Database.AddFeedAsync(ctx.Channel.Id, url, sub).ConfigureAwait(false))
                throw new CommandFailedException("You are already subscribed to this subreddit!");

            await ReplyWithEmbedAsync(ctx, $"Subscribed to {Formatter.Bold(rsub)} !")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RSS_REDDIT_UNSUBSCRIBE
        [Command("unsubscribe"), Priority(1)]
        [Description("Remove a subreddit feed using subreddit name or subscription ID (use ``!feed list`` to see IDs).")]
        [Aliases("del", "d", "rm", "-", "unsub")]
        [UsageExample("!reddit unsub aww")]
        [UsageExample("!reddit unsub 12")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task UnsubscribeAsync(CommandContext ctx,
                                          [Description("Subreddit.")] string sub)
        {
            if (RSSService.GetRedditFeedURLForSubreddit(sub, out string rsub) == null)
                throw new CommandFailedException("That subreddit doesn't exist.");

            await Database.RemoveSubscriptionUsingNameAsync(ctx.Channel.Id, rsub)
                .ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx, $"Unsubscribed from {Formatter.Bold(rsub)} !")
                .ConfigureAwait(false);
        }

        [Command("unsubscribe"), Priority(0)]
        public async Task UnsubscribeAsync(CommandContext ctx,
                                          [Description("Subscription ID.")] int id)
        {
            await Database.RemoveSubscriptionAsync(ctx.Channel.Id, id)
                .ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion
    }
}
