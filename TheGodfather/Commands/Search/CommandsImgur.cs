#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Services;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Imgur.API.Enums;
using Imgur.API.Models;
using Imgur.API.Models.Impl;
using Imgur.API;
#endregion

namespace TheGodfather.Commands.Search
{
    [Group("imgur", CanInvokeWithoutSubcommand = true)]
    [Description("Search imgur. Invoking without sub command searches top.")]
    [Aliases("img", "im", "i")]
    [Cooldown(2, 5, CooldownBucketType.User), Cooldown(4, 5, CooldownBucketType.Channel)]
    [PreExecutionCheck]
    public class CommandsImgur
    {

        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Number of images to print [1-10].")] int n,
                                           [RemainingText, Description("Query.")] string sub)
        {
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidCommandUsageException("Missing search query.");
            if (n < 1 || n > 10)
                throw new CommandFailedException("Invalid amount (must be 1-10).", new ArgumentOutOfRangeException());

            var res = await ctx.Dependencies.GetDependency<ImgurService>().GetItemsFromSubAsync(
                sub,
                n,
                SubredditGallerySortOrder.Top,
                TimeWindow.Day
            ).ConfigureAwait(false);

            await PrintImagesAsync(ctx, res, n)
                .ConfigureAwait(false);
        }


        #region COMMAND_IMGUR_LATEST
        [Command("latest")]
        [Description("Return latest images for query.")]
        [Aliases("l", "new", "newest")]
        public async Task LatestAsync(CommandContext ctx,
                                     [Description("Number of images to print [1-10].")] int n,
                                     [Description("Query.")] string sub)
        {
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidCommandUsageException("Missing search query.");
            if (n < 1 || n > 10)
                throw new CommandFailedException("Invalid amount (must be 1-10).", new ArgumentOutOfRangeException());


            var res = await ctx.Dependencies.GetDependency<ImgurService>().GetItemsFromSubAsync(
                sub, 
                n, 
                SubredditGallerySortOrder.Time, 
                TimeWindow.Day
            ).ConfigureAwait(false);

            await PrintImagesAsync(ctx, res, n)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_IMGUR_TOP
        [Command("top")]
        [Description("Return most rated images for query.")]
        [Aliases("t")]
        public async Task TopAsync(CommandContext ctx,
                                  [Description("Time window (day/month/week/year/all).")] string time = "day",
                                  [Description("Number of images to print [1-10].")] int n = 1,
                                  [RemainingText, Description("Query.")] string sub = null)
        {
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidCommandUsageException("Missing search query.");
            if (n < 1 || n > 10)
                throw new CommandFailedException("Invalid amount (must be 1-10).", new ArgumentOutOfRangeException());

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

            var res = await ctx.Dependencies.GetDependency<ImgurService>().GetItemsFromSubAsync(
                sub,
                n,
                SubredditGallerySortOrder.Time,
                t
            ).ConfigureAwait(false);

            await PrintImagesAsync(ctx, res, n)
                .ConfigureAwait(false);
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task PrintImagesAsync(CommandContext ctx, IEnumerable<IGalleryItem> results, int num)
        {
            if (!results.Any()) {
                await ctx.RespondAsync("No results...")
                    .ConfigureAwait(false);
                return;
            }

            try {
                foreach (var im in results) {
                    if (im.GetType().Name == "GalleryImage") {
                        var img = ((GalleryImage)im);
                        if (!ctx.Channel.IsNSFW && img.Nsfw != null && img.Nsfw == true)
                            throw new CommandFailedException("This is not a NSFW channel!");
                        await ctx.RespondAsync(img.Link)
                            .ConfigureAwait(false);
                    } else if (im.GetType().Name == "GalleryAlbum") {
                        var img = ((GalleryAlbum)im);
                        if (!ctx.Channel.IsNSFW && img.Nsfw != null && img.Nsfw == true)
                            throw new CommandFailedException("This is not a NSFW channel!");
                        await ctx.RespondAsync(img.Link)
                            .ConfigureAwait(false);
                    } else
                        throw new CommandFailedException("Imgur API error.");

                    await Task.Delay(1000)
                        .ConfigureAwait(false);
                }
            } catch (ImgurException ie) {
                throw new CommandFailedException("Imgur API error.", ie);
            }

            if (results.Count() != num) {
                await ctx.RespondAsync("These are all of the results returned.")
                    .ConfigureAwait(false);
            }
        }
        #endregion
    }
}
