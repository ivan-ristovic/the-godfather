#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Imgur.API;
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
        [Aliases("img", "im", "i")]
        public async Task Imgur(CommandContext ctx,
                                [Description("Query (optional).")] string sub = null,
                                [Description("Number of images to print [1-10].")] int n = 1)
        {
            if (string.IsNullOrWhiteSpace(sub) || n < 1 || n > 10)
                await GetImagesFromSub(ctx, "pics", 1);
            else
                await GetImagesFromSub(ctx, sub.Trim(), n);
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task GetImagesFromSub(CommandContext ctx, string sub, int num)
        {
            try {
                var images = await _endpoint.GetSubredditGalleryAsync(sub, SubredditGallerySortOrder.Top, TimeWindow.Day);

                int i = num;
                foreach (var im in images) {
                    if (i-- == 0)
                        break;
                    await ctx.RespondAsync(im.Link);
                    await Task.Delay(1000);
                }

                if (i == num)
                    throw new Exception("No results...");

                if (i != 0) {
                    await ctx.RespondAsync("These are all of the results returned.");
                }
            } catch {
                throw new ImgurException("Imgur API returned album which I can't show...");
            }
        }
        #endregion
    }
}
