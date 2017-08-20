#region USING_DIRECTIVES
using System;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfatherBot
{
    [Group("rss", CanInvokeWithoutSubcommand = true)]
    [Description("RSS feed operations.")]
    public class CommandsRSS
    {
        public async Task ExecuteGroup(CommandContext ctx)
        {
            await RSSFeedRead(ctx, "http://worldmafia.net/forum/forums/-/index.rss");
        }

        private async Task RSSFeedRead(CommandContext ctx, string url)
        {
            XmlReader reader = XmlReader.Create(url);
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            reader.Close();
            foreach (SyndicationItem item in feed.Items) {
                await ctx.RespondAsync(item.Title.Text);
            }
        }
    }
}
