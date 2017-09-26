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
using Imgur.API.Models.Impl;
#endregion

namespace TheGodfatherBot.Modules.Search
{
    [Group("imgur", CanInvokeWithoutSubcommand = true)]
    [Description("Search imgur. Invoking without sub command searches top.")]
    [Aliases("img", "im", "i")]
    public class CommandsImgur
    {
        #region STATIC_FIELDS
        private static ImgurClient _imgurclient = new ImgurClient("5222972687f2120");
        private static GalleryEndpoint _endpoint = new GalleryEndpoint(_imgurclient);
        #endregion
        
        
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Query (optional).")] string sub = null,
                                           [Description("Number of images to print [1-10].")] int n = 1)
        {
            await ImgurTop(ctx, sub, n);
        }


        #region COMMAND_IMGUR_TOP
        [Command("top")]
        [Description("Return most rated images for query.")]
        [Aliases("t")]
        public async Task ImgurTop(CommandContext ctx,
                                  [Description("Query (optional).")] string sub = null,
                                  [Description("Number of images to print [1-10].")] int n = 1)
        {
            if (string.IsNullOrWhiteSpace(sub) || n < 1 || n > 10)
                throw new ArgumentException("Invalid sub or number of images (must be less than 10).");
            
            await GetImagesFromSub(ctx, sub.Trim(), n, SubredditGallerySortOrder.Top, TimeWindow.Day);
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task GetImagesFromSub(CommandContext ctx, 
                                            string sub, 
                                            int num, 
                                            SubredditGallerySortOrder order,
                                            TimeWindow time)
        {
            try {
                var images = await _endpoint.GetSubredditGalleryAsync(sub, order, time);
                
                int i = num;
                foreach (var im in images) {
                    if (i-- == 0)
                        break;
                    if (im.GetType().Name == "GalleryImage") {
                        var img = ((GalleryImage)im);
                        if (!ctx.Channel.IsNSFW && img.Nsfw != null && img.Nsfw == true)
                            throw new Exception("This is not a NSFW channel!");
                        await ctx.RespondAsync(img.Link);
                    } else if (im.GetType().Name == "GalleryAlbum") {
                        var img = ((GalleryAlbum)im);
                        if (!ctx.Channel.IsNSFW && img.Nsfw != null && img.Nsfw == true)
                            throw new Exception("This is not a NSFW channel!");
                        await ctx.RespondAsync(img.Link);
                    } else
                        throw new ImgurException("Imgur API error");
                    await Task.Delay(1000);
                }

                if (i == num)
                    await ctx.RespondAsync("No results...");

                if (i > 0) {
                    await ctx.RespondAsync("These are all of the results returned.");
                }
            } catch (ImgurException ie) {
                throw ie;
            } catch (Exception e) {
                throw e;
            }
        }
        #endregion
    }
}
