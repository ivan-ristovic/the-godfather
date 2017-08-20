#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Enums;
#endregion

namespace TheGodfatherBot
{
    [Description("Imgur commands.")]
    public class CommandsImgur
    {
        #region STATIC_FIELDS
        static ImgurClient _imgurclient = new ImgurClient("5222972687f2120");
        static GalleryEndpoint _endpoint = new GalleryEndpoint(_imgurclient);
        #endregion

        #region COMMAND_IMGUR
        [Command("imgur"), Description("Search imgur.")]
        [Aliases("img", "im")]
        public async Task Imgur(CommandContext ctx, [Description("Query (optional)")] string sub = null)
        {
            if (sub == null || sub.Trim() == "")
                await GetImagesFromSub(ctx, "pics");
            else
                await GetImagesFromSub(ctx, sub.Trim());
        }
        #endregion

        #region HELPER_FUNCTIONS
        private async Task GetImagesFromSub(CommandContext ctx, string sub)
        {
            var images = await _endpoint.GetSubredditGalleryAsync(sub, SubredditGallerySortOrder.Top, TimeWindow.Day);

            int i = 3;
            foreach (var im in images) {
                if (i-- == 0)
                    break;
                await ctx.RespondAsync(im.ToString());
            }

            if (i == 3) {
                await ctx.RespondAsync("Subreddit not found...");
                return;
            }
        }
        #endregion
    }
}
