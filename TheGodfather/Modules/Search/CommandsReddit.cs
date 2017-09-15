#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using RedditSharp;
using static RedditSharp.Things.VotableThing;
#endregion


namespace TheGodfatherBot.Modules.Search
{
    [Group("reddit", CanInvokeWithoutSubcommand = true)]
    [Description("Reddit commands.")]
    [Aliases("r")]
    public class CommandsReddit
    {
        /*
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            var wa = new BotWebAgent();
            var reddit = new Reddit(wa, false);
            var subreddit = reddit.GetSubreddit("/r/all");
            subreddit.Subscribe();
            foreach (var post in subreddit.New.Take(5)) {
                await ctx.RespondAsync(post.Title);
            }
        }
        */
    }
}
