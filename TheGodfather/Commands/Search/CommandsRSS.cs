#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;

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
    [Cooldown(2, 5, CooldownBucketType.User), Cooldown(4, 5, CooldownBucketType.Channel)]
    public class CommandsRSS
    {
        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                            [RemainingText, Description("URL")] string url = null)
        {
            if (string.IsNullOrWhiteSpace(url)) {
                await WMRSS(ctx);
            } else {
                await SendFeedResults(ctx, ctx.Dependencies.GetDependency<FeedManager>().GetFeedResults(url));
            }
        }


        #region COMMAND_WM
        [Command("wm"), Description("Get newest topics from WM forum.")]
        public async Task WMRSS(CommandContext ctx)
        {
            await SendFeedResults(ctx, ctx.Dependencies.GetDependency<FeedManager>().GetFeedResults("http://worldmafia.net/forum/forums/-/index.rss"));
        }
        #endregion

        #region COMMAND_NEWS
        [Command("news"), Description("Get newest world news.")]
        public async Task NewsRSS(CommandContext ctx)
        {
            await SendFeedResults(ctx, ctx.Dependencies.GetDependency<FeedManager>().GetFeedResults("https://news.google.com/news/rss/headlines/section/topic/WORLD?ned=us&hl=en"));
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task SendFeedResults(CommandContext ctx, IEnumerable<SyndicationItem> results)
        {
            if (results == null)
                throw new CommandFailedException("Error getting RSS feed.");

            var embed = new DiscordEmbedBuilder() {
                Title = "Topics active recently",
                Color = DiscordColor.Green
            };

            foreach (var res in results.Take(5))
                embed.AddField(res.Title.Text, res.Links[0].Uri.ToString());

            await ctx.RespondAsync(embed: embed);
        }
        #endregion
    }
}
