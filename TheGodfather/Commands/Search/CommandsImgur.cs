#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfatherBot.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Imgur.API;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Enums;
using Imgur.API.Models.Impl;
#endregion

namespace TheGodfatherBot.Commands.Search
{
    [Group("imgur", CanInvokeWithoutSubcommand = true)]
    [Description("Search imgur. Invoking without sub command searches top.")]
    [Aliases("img", "im", "i")]
    public class CommandsImgur
    {
        #region STATIC_FIELDS
        private static ImgurClient _imgurclient = new ImgurClient(TheGodfather.Config.ImgurKey);
        private static GalleryEndpoint _endpoint = new GalleryEndpoint(_imgurclient);
        #endregion


        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Number of images to print [1-10].")] int n = 1,
                                           [Description("Query.")] string sub = null)
        {
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidCommandUsageException("Missing search query.");
            if (n < 1 || n > 10)
                throw new CommandFailedException("Invalid ammount (must be 1-10).", new ArgumentOutOfRangeException());

            await PrintImagesFromSub(ctx, sub.Trim(), n, SubredditGallerySortOrder.Top, TimeWindow.Day);
        }


        #region COMMAND_IMGUR_LATEST
        [Command("latest")]
        [Description("Return latest images for query.")]
        [Aliases("l", "new", "newest")]
        public async Task ImgurTop(CommandContext ctx,
                                  [Description("Number of images to print [1-10].")] int n = 1,
                                  [Description("Query.")] string sub = null)
        {
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidCommandUsageException("Missing search query.");
            if (n < 1 || n > 10)
                throw new CommandFailedException("Invalid ammount (must be 1-10).", new ArgumentOutOfRangeException());

            await PrintImagesFromSub(ctx, sub.Trim(), n, SubredditGallerySortOrder.Time, TimeWindow.Day);
        }
        #endregion

        #region COMMAND_IMGUR_TOP
        [Command("top")]
        [Description("Return most rated images for query.")]
        [Aliases("t")]
        public async Task ImgurTop(CommandContext ctx,
                                  [Description("Time window (day/month/week/year/all).")] string time = "day",
                                  [Description("Number of images to print [1-10].")] int n = 1,
                                  [Description("Query.")] string sub = null)
        {
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidCommandUsageException("Missing search query.");
            if (n < 1 || n > 10)
                throw new CommandFailedException("Invalid ammount (must be 1-10).", new ArgumentOutOfRangeException());

            TimeWindow t = TimeWindow.Day;
            if (time == "day" || time == "d")
                t = TimeWindow.Day;
            else if (time == "week" || time == "w")
                t = TimeWindow.Week;
            else if (time == "month" || time == "m")
                t = TimeWindow.Month;
            else if(time == "year" || time == "y")
                t = TimeWindow.Year;
            else if (time == "all" || time == "a")
                t = TimeWindow.All;

            await PrintImagesFromSub(ctx, sub.Trim(), n, SubredditGallerySortOrder.Top, t);
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task PrintImagesFromSub(CommandContext ctx, 
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

                if (i == num) {
                    await ctx.RespondAsync("No results...");
                    return;
                }

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
