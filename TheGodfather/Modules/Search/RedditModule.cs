#region USING_DIRECTIVES
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("reddit"), Module(ModuleType.Searches), NotBlocked]
    [Description("Reddit commands. Group call prints hottest posts from given sub.")]
    [Aliases("r")]
    
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class RedditModule : TheGodfatherModule
    {

        public RedditModule(DbContextBuilder db)
            : base(db)
        {
            
        }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Subreddit.")] string sub = "all")
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.Hot);


        #region COMMAND_RSS_REDDIT_CONTROVERSIAL
        [Command("controversial")]
        [Description("Get newest controversial posts for a subreddit.")]
        
        public Task ControversialAsync(CommandContext ctx,
                                      [Description("Subreddit.")] string sub)
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.Controversial);
        #endregion

        #region COMMAND_RSS_REDDIT_GILDED
        [Command("gilded")]
        [Description("Get newest gilded posts for a subreddit.")]
        
        public Task GildedAsync(CommandContext ctx,
                               [Description("Subreddit.")] string sub)
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.Gilded);
        #endregion

        #region COMMAND_RSS_REDDIT_HOT
        [Command("hot")]
        [Description("Get newest hot posts for a subreddit.")]
        
        public Task HotAsync(CommandContext ctx,
                            [Description("Subreddit.")] string sub)
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.Hot);
        #endregion

        #region COMMAND_RSS_REDDIT_NEW
        [Command("new")]
        [Description("Get newest posts for a subreddit.")]
        [Aliases("newest", "latest")]
        
        public Task NewAsync(CommandContext ctx,
                            [Description("Subreddit.")] string sub)
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.New);
        #endregion

        #region COMMAND_RSS_REDDIT_RISING
        [Command("rising")]
        [Description("Get newest rising posts for a subreddit.")]
        
        public Task RisingAsync(CommandContext ctx,
                               [Description("Subreddit.")] string sub)
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.Rising);
        #endregion

        #region COMMAND_RSS_REDDIT_TOP
        [Command("top")]
        [Description("Get top posts for a subreddit.")]
        
        public Task TopAsync(CommandContext ctx,
                            [Description("Subreddit.")] string sub)
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.Top);
        #endregion

        #region COMMAND_RSS_REDDIT_SUBSCRIBE
        [Command("subscribe")]
        [Description("Add new feed for a subreddit.")]
        [Aliases("add", "a", "+", "sub")]
        
        [RequireUserPermissions(Permissions.ManageGuild)]
        public Task SubscribeAsync(CommandContext ctx,
                                  [Description("Subreddit.")] string sub)
        {
            string command = $"sub r {sub}";
            Command cmd = ctx.CommandsNext.FindCommand(command, out string args);
            CommandContext fctx = ctx.CommandsNext.CreateFakeContext(ctx.Member, ctx.Channel, command, ctx.Prefix, cmd, args);
            return ctx.CommandsNext.ExecuteCommandAsync(fctx);
        }
        #endregion

        #region COMMAND_RSS_REDDIT_UNSUBSCRIBE
        [Command("unsubscribe"), Priority(1)]
        [Description("Remove a subreddit feed using subreddit name or subscription ID (use command ``feed list`` to see IDs).")]
        [Aliases("del", "d", "rm", "-", "unsub")]
        
        [RequireUserPermissions(Permissions.ManageGuild)]
        public Task UnsubscribeAsync(CommandContext ctx,
                                    [Description("Subreddit.")] string sub)
        {
            string command = $"unsub r {sub}";
            Command cmd = ctx.CommandsNext.FindCommand(command, out string args);
            CommandContext fctx = ctx.CommandsNext.CreateFakeContext(ctx.Member, ctx.Channel, command, ctx.Prefix, cmd, args);
            return ctx.CommandsNext.ExecuteCommandAsync(fctx);
        }

        [Command("unsubscribe"), Priority(0)]
        public async Task UnsubscribeAsync(CommandContext ctx,
                                          [Description("Subscription ID.")] int id)
        {
            using (TheGodfatherDbContext db = this.Database.CreateDbContext()) {
                db.RssSubscriptions.Remove(new RssSubscription { ChannelId = ctx.Channel.Id, Id = id });
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, $"Removed subscription with ID {Formatter.Bold(id.ToString())}", important: false);
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task SearchAndSendResultsAsync(CommandContext ctx, string sub, RedditCategory category)
        {
            string url = RedditService.GetFeedURLForSubreddit(sub, category, out string rsub);
            if (url is null)
                throw new CommandFailedException("That subreddit doesn't exist.");

            IReadOnlyList<SyndicationItem> res = RssService.GetFeedResults(url);
            if (res is null)
                throw new CommandFailedException($"Failed to get the data from that subreddit ({Formatter.Bold(rsub)}).");

            await RssService.SendFeedResultsAsync(ctx.Channel, res);
        }
        #endregion
    }
}
