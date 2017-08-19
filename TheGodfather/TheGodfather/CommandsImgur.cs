using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models.Impl;
using Imgur.API.Enums;

namespace TheGodfatherBot
{
    [Description("Imgur commands.")]
    public class CommandsImgur
    {
        static ImgurClient client = new ImgurClient("5222972687f2120");
        static GalleryEndpoint endpoint = new GalleryEndpoint(client);

        [Command("imgur")]
        [Description("Search imgur.")]
        [Aliases("img", "im")]
        public async Task Imgur(CommandContext ctx, [Description("Query (optional)")] string sub = null)
        {
            if (sub == null || sub.Trim() == "")
                await GetImagesFromSub(ctx, "pics");
            else
                await GetImagesFromSub(ctx, sub.Trim());
        }

        private async Task GetImagesFromSub(CommandContext ctx, string sub)
        {
            var images = await endpoint.GetSubredditGalleryAsync(sub, SubredditGallerySortOrder.Top, TimeWindow.Day);

            int i = 3;
            foreach (var im in images)
            {
                if (i-- == 0)
                    break;
                await ctx.RespondAsync(im.ToString());
            }

            if (i == 3)
            {
                await ctx.RespondAsync("Subreddit not found...");
                return;
            }
        }
    }
}
