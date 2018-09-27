#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Search.Extensions;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("reddit"), Module(ModuleType.Searches), NotBlocked]
    [Description("Reddit commands. Group call prints latest posts from given sub.")]
    [Aliases("r")]
    [UsageExamples("!reddit aww")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class RedditModule : TheGodfatherModule
    {

        public RedditModule(SharedData shared, DBService db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Orange;
        }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Subreddit.")] string sub = "all")
        {
            string url = RssService.GetFeedURLForSubreddit(sub, out string rsub);
            if (url == null)
                throw new CommandFailedException("That subreddit doesn't exist.");

            IReadOnlyList<SyndicationItem> res = RssService.GetFeedResults(url);
            if (res == null)
                throw new CommandFailedException($"Failed to get the data from that subreddit ({Formatter.Bold(rsub)}).");

            return RssService.SendFeedResultsAsync(ctx.Channel, res);
        }




        #region COMMAND_RSS_REDDIT_SUBSCRIBE
        [Command("subscribe")]
        [Description("Add new feed for a subreddit.")]
        [Aliases("add", "a", "+", "sub")]
        [UsageExamples("!reddit sub aww")]
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
        [UsageExamples("!reddit unsub aww",
                       "!reddit unsub 12")]
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
            await this.Database.RemoveSubscriptionByIdAsync(ctx.Channel.Id, id);
            await this.InformAsync(ctx, $"Removed subscription with ID {Formatter.Bold(id.ToString())}", important: false);
        }
        #endregion

        // TODO reddit top, all...
    }
}
