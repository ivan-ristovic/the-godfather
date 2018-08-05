#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Imgur.API;
using Imgur.API.Enums;
using Imgur.API.Models;
using Imgur.API.Models.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("imgur"), Module(ModuleType.Searches), NotBlocked]
    [Description("Search imgur. Group call retrieves top ranked images from given subreddit.")]
    [Aliases("img", "im", "i")]
    [UsageExamples("!imgur aww",
                   "!imgur 10 aww",
                   "!imgur aww 10")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class ImgurModule : TheGodfatherServiceModule<ImgurService>
    {

        public ImgurModule(ImgurService imgur, SharedData shared, DBService db)
            : base(imgur, shared, db)
        {

        }


        [GroupCommand, Priority(1)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Number of images to print [1-10].")] int amount,
                                           [RemainingText, Description("Subreddit.")] string sub)
        {
            if (this.Service.IsDisabled())
                throw new ServiceDisabledException();

            var res = await this.Service.GetItemsFromSubAsync(
                sub,
                amount,
                SubredditGallerySortOrder.Top,
                TimeWindow.Day
            ).ConfigureAwait(false);

            await PrintImagesAsync(ctx.Channel, res, amount)
                .ConfigureAwait(false);
        }

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Subreddit.")] string sub,
                                     [Description("Number of images to print [1-10].")] int n = 1)
            => ExecuteGroupAsync(ctx, n, sub);


        #region COMMAND_IMGUR_LATEST
        [Command("latest"), Priority(1)]
        [Description("Return latest images from given subreddit.")]
        [Aliases("l", "new", "newest")]
        [UsageExamples("!imgur latest 5 aww",
                       "!imgur latest aww 5")]
        public async Task LatestAsync(CommandContext ctx,
                                     [Description("Number of images to print [1-10].")] int amount,
                                     [RemainingText, Description("Subreddit.")] string sub)
        {
            if (this.Service.IsDisabled())
                throw new ServiceDisabledException();

            var res = await this.Service.GetItemsFromSubAsync(sub, amount, SubredditGallerySortOrder.Time, TimeWindow.Day);
            await PrintImagesAsync(ctx.Channel, res, amount);
        }

        [Command("latest"), Priority(0)]
        public Task LatestAsync(CommandContext ctx,
                               [Description("Subreddit.")] string sub,
                               [Description("Number of images to print [1-10].")] int n)
            => LatestAsync(ctx, n, sub);
        #endregion

        #region COMMAND_IMGUR_TOP
        [Command("top"), Priority(3)]
        [Description("Return amount of top rated images in the given subreddit for given timespan.")]
        [Aliases("t")]
        [UsageExamples("!imgur top day 10 aww",
                       "!imgur top 10 day aww",
                       "!imgur top 5 aww",
                       "!imgur top day aww")]
        public async Task TopAsync(CommandContext ctx,
                                  [Description("Timespan in which to search (day/week/month/year/all).")] TimeWindow timespan,
                                  [Description("Number of images to print [1-10].")] int amount,
                                  [RemainingText, Description("Subreddit.")] string sub)
        {
            if (this.Service.IsDisabled())
                throw new ServiceDisabledException();

            var res = await this.Service.GetItemsFromSubAsync(sub, amount, SubredditGallerySortOrder.Time, timespan);
            await PrintImagesAsync(ctx.Channel, res, amount);
        }

        [Command("top"), Priority(2)]
        public Task TopAsync(CommandContext ctx,
                            [Description("Timespan in which to search (day/week/month/year/all).")] TimeWindow timespan,
                            [Description("Subreddit.")] string sub,
                            [Description("Number of images to print [1-10].")] int amount = 1)
            => TopAsync(ctx, timespan, amount, sub);

        [Command("top"), Priority(1)]
        public Task TopAsync(CommandContext ctx,
                            [Description("Number of images to print [1-10].")] int amount,
                            [Description("Timespan in which to search (day/week/month/year/all).")] TimeWindow timespan,
                            [RemainingText, Description("Subreddit.")] string sub)
            => TopAsync(ctx, timespan, amount, sub);

        [Command("top"), Priority(0)]
        public Task TopAsync(CommandContext ctx,
                            [Description("Number of images to print [1-10].")] int amount,
                            [RemainingText, Description("Subreddit.")] string sub)
            => TopAsync(ctx, TimeWindow.Day, amount, sub);

        #endregion


        #region HELPER_FUNCTIONS
        private async Task PrintImagesAsync(DiscordChannel channel, IEnumerable<IGalleryItem> results, int num)
        {
            if (!results.Any()) {
                await channel.InformFailureAsync("No results...");
                return;
            }

            try {
                foreach (var im in results) {
                    if (im.GetType().Name == "GalleryImage") {
                        var img = ((GalleryImage)im);
                        if (img.Nsfw != null && img.Nsfw == true && !channel.IsNSFW && !channel.Name.StartsWith("nsfw", StringComparison.InvariantCultureIgnoreCase))
                            throw new CommandFailedException("This is not a NSFW channel!");
                        await channel.SendMessageAsync(img.Link)
                            .ConfigureAwait(false);
                    } else if (im.GetType().Name == "GalleryAlbum") {
                        var img = ((GalleryAlbum)im);
                        if (img.Nsfw != null && img.Nsfw == true && !channel.IsNSFW && !channel.Name.StartsWith("nsfw", StringComparison.InvariantCultureIgnoreCase))
                            throw new CommandFailedException("This is not a NSFW channel!");
                        await channel.SendMessageAsync(img.Link)
                            .ConfigureAwait(false);
                    } else
                        throw new CommandFailedException("Imgur API error.");

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            } catch (ImgurException e) {
                throw new CommandFailedException("Imgur API error.", e);
            }

            if (results.Count() != num)
                await channel.InformFailureAsync("These are all of the results returned.");
        }
        #endregion
    }
}
