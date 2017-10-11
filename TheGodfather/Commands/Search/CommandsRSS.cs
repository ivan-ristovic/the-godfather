#region USING_DIRECTIVES
using System;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;

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
            if (string.IsNullOrWhiteSpace(url))
                await WMRSS(ctx);
            else
                await RSSFeedRead(ctx, url);
        }

        #region COMMAND_WM
        [Command("wm"), Description("Get newest topics from WM forum.")]
        public async Task WMRSS(CommandContext ctx)
        {
            await RSSFeedRead(ctx, "http://worldmafia.net/forum/forums/-/index.rss");
        }
        #endregion

        #region COMMAND_NEWS
        [Command("news"), Description("Get newest world news.")]
        public async Task NewsRSS(CommandContext ctx)
        {
            await RSSFeedRead(ctx, "https://news.google.com/news/rss/headlines/section/topic/WORLD?ned=us&hl=en");
        }
        #endregion

        #region HELPER_FUNCTIONS
        private async Task RSSFeedRead(CommandContext ctx, string url)
        {
            SyndicationFeed feed = null;
            try {
                XmlReader reader = XmlReader.Create(url);
                feed = SyndicationFeed.Load(reader);
                reader.Close();
            } catch (Exception) {
                await ctx.RespondAsync("Error getting RSS feed from " + url);
                return;
            }

            var embed = new DiscordEmbedBuilder() {
                Title = "Topics active recently",
                Color = DiscordColor.Green
            };

            int count = 5;
            foreach (SyndicationItem item in feed.Items) {
                if (count-- == 0)
                    break;
                embed.AddField(item.Title.Text, item.Links[0].Uri.ToString());
            }

            await ctx.RespondAsync(embed: embed);
        }
        #endregion
    }
}
