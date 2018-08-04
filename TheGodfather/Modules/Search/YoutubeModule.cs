#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;
using TheGodfather.Services.Database;
using TheGodfather.Services.Database.Feeds;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("youtube"), Module(ModuleType.Searches)]
    [Description("Youtube search commands. If invoked without subcommands, searches YouTube for given query.")]
    [Aliases("y", "yt", "ytube")]
    [UsageExamples("!youtube never gonna give you up")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class YoutubeModule : TheGodfatherServiceModule<YtService>
    {

        public YoutubeModule(YtService yt, DBService db) : base(yt, db: db) { }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("Search query.")] string query)
            => SearchAndSendResultsAsync(ctx, 5, query);


        #region COMMAND_YOUTUBE_SEARCH
        [Command("search"), Module(ModuleType.Searches)]
        [Description("Advanced youtube search.")]
        [Aliases("s")]
        [UsageExamples("!youtube search 5 rick astley")]
        public Task AdvancedSearchAsync(CommandContext ctx,
                                       [Description("Amount of results. [1-10]")] int amount,
                                       [RemainingText, Description("Search query.")] string query)
            => SearchAndSendResultsAsync(ctx, amount, query);
        #endregion

        #region COMMAND_YOUTUBE_SEARCHVIDEO
        [Command("searchvideo"), Module(ModuleType.Searches)]
        [Description("Advanced youtube search for videos only.")]
        [Aliases("sv", "searchv")]
        [UsageExamples("!youtube searchvideo 5 rick astley")]
        public Task SearchVideoAsync(CommandContext ctx,
                                    [RemainingText, Description("Search query.")] string query)
            => SearchAndSendResultsAsync(ctx, 5, query, "video");
        #endregion

        #region COMMAND_YOUTUBE_SEARCHCHANNEL
        [Command("searchchannel"), Module(ModuleType.Searches)]
        [Description("Advanced youtube search for channels only.")]
        [Aliases("sc", "searchc")]
        [UsageExamples("!youtube searchchannel 5 rick astley")]
        public Task SearchChannelAsync(CommandContext ctx,
                                      [RemainingText, Description("Search query.")] string query)
            => SearchAndSendResultsAsync(ctx, 5, query, "channel");
        #endregion

        #region COMMAND_YOUTUBE_SEARCHPLAYLIST
        [Command("searchp"), Module(ModuleType.Searches)]
        [Description("Advanced youtube search for playlists only.")]
        [Aliases("sp", "searchplaylist")]
        [UsageExamples("!youtube searchplaylist 5 rick astley")]
        public Task SearchPlaylistAsync(CommandContext ctx,
                                       [RemainingText, Description("Search query.")] string query)
            => SearchAndSendResultsAsync(ctx, 5, query, "playlist");
        #endregion

        #region COMMAND_YOUTUBE_SUBSCRIBE
        [Command("subscribe"), Module(ModuleType.Searches)]
        [Description("Add a new subscription for a YouTube channel.")]
        [Aliases("add", "a", "+", "sub")]
        [UsageExamples("!youtube subscribe https://www.youtube.com/user/RickAstleyVEVO",
                       "!youtube subscribe https://www.youtube.com/user/RickAstleyVEVO rick")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task SubscribeAsync(CommandContext ctx,
                                        [Description("Channel URL.")] string url,
                                        [Description("Friendly name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidCommandUsageException("Channel URL missing.");

            var chid = await Service.ExtractChannelIdAsync(url)
                .ConfigureAwait(false);

            if (chid == null)
                throw new CommandFailedException("Failed retrieving channel ID for that URL.");

            var feedurl = YtService.GetRssUrlForChannel(chid);
            if (await Database.TryAddSubscriptionAsync(ctx.Channel.Id, feedurl, string.IsNullOrWhiteSpace(name) ? url : name).ConfigureAwait(false))
                await InformAsync(ctx, "Subscribed!").ConfigureAwait(false);
            else
                await InformFailureAsync(ctx, "Either the channel URL you is invalid or you are already subscribed to it!").ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_YOUTUBE_UNSUBSCRIBE
        [Command("unsubscribe"), Module(ModuleType.Searches)]
        [Description("Remove a YouTube channel subscription.")]
        [Aliases("del", "d", "rm", "-", "unsub")]
        [UsageExamples("!youtube unsubscribe https://www.youtube.com/user/RickAstleyVEVO",
                       "!youtube unsubscribe rick")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task UnsubscribeAsync(CommandContext ctx,
                                          [Description("Channel URL or subscription name.")] string name_url)
        {
            if (string.IsNullOrWhiteSpace(name_url))
                throw new InvalidCommandUsageException("Channel URL missing.");

            await Database.RemoveSubscriptionByNameAsync(ctx.Channel.Id, name_url)
                .ConfigureAwait(false);

            var chid = await Service.ExtractChannelIdAsync(name_url)
                .ConfigureAwait(false);
            if (chid != null) {
                var feedurl = YtService.GetRssUrlForChannel(chid);
                await Database.RemoveSubscriptionByUrlAsync(ctx.Channel.Id, feedurl)
                    .ConfigureAwait(false);
            }

            await InformAsync(ctx, "Unsubscribed!")
                .ConfigureAwait(false);
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task SearchAndSendResultsAsync(CommandContext ctx, int amount, string query, string type = null)
        {
            if (this.Service.IsDisabled())
                throw new ServiceDisabledException();

            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("Search query missing.");

            if (amount < 1 || amount > 10)
                throw new CommandFailedException("Invalid amount (must be 1-10).");

            var pages = await Service.GetPaginatedResultsAsync(query, amount, type)
                .ConfigureAwait(false);
            if (pages == null) {
                await InformFailureAsync(ctx, "No results found!")
                    .ConfigureAwait(false);
                return;
            }

            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, pages)
                .ConfigureAwait(false);
        }
        #endregion
    }
}
