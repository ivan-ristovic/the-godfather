#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("youtube")]
    [Description("Youtube search commands. If invoked without subcommands, searches YouTube for given query.")]
    [Aliases("y", "yt", "ytube")]
    [UsageExample("!youtube never gonna give you up")]
    [Cooldown(2, 5, CooldownBucketType.User), Cooldown(4, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class YoutubeModule : TheGodfatherServiceModule<YoutubeService>
    {

        public YoutubeModule(YoutubeService yt) : base(yt) { }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Search query.")] string query)
            => await SearchAndSendResultsAsync(ctx, 5, query).ConfigureAwait(false);


        #region COMMAND_YOUTUBE_SEARCH
        [Command("search")]
        [Description("Advanced youtube search.")]
        [Aliases("s")]
        [UsageExample("!youtube search 5 rick astley")]
        public async Task AdvancedSearchAsync(CommandContext ctx,
                                             [Description("Amount of results. [1-10]")] int amount,
                                             [RemainingText, Description("Search query.")] string query)
            => await SearchAndSendResultsAsync(ctx, amount, query).ConfigureAwait(false);
        #endregion
        
        #region COMMAND_YOUTUBE_SEARCHVIDEO
        [Command("searchv")]
        [Description("Advanced youtube search for videos only.")]
        [Aliases("sv", "searchvideo")]
        public async Task SearchVideoAsync(CommandContext ctx,
                                          [RemainingText, Description("Search query.")] string query)
            => await SearchAndSendResultsAsync(ctx, 5, query, "video").ConfigureAwait(false);
        #endregion

        #region COMMAND_YOUTUBE_SEARCHCHANNEL
        [Command("searchc")]
        [Description("Advanced youtube search for channels only.")]
        [Aliases("sc", "searchchannel")]
        public async Task SearchChannelAsync(CommandContext ctx,
                                            [RemainingText, Description("Search query.")] string query)
            => await SearchAndSendResultsAsync(ctx, 5, query, "channel").ConfigureAwait(false);
        #endregion

        #region COMMAND_YOUTUBE_SEARCHPLAYLIST
        [Command("searchp")]
        [Description("Advanced youtube search for playlists only.")]
        [Aliases("sp", "searchplaylist")]
        public async Task SearchPlaylistAsync(CommandContext ctx,
                                             [RemainingText, Description("Search query.")] string query)
            => await SearchAndSendResultsAsync(ctx, 5, query, "playlist").ConfigureAwait(false);
        #endregion


        #region HELPER_FUNCTIONS
        private async Task SearchAndSendResultsAsync(CommandContext ctx, int amount, string query, string type = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("Search query missing.");

            if (amount < 1 || amount > 10)
                throw new CommandFailedException("Invalid amount (must be 1-10).");

            var pages = await Service.GetPaginatedResults(query, amount, type)
                .ConfigureAwait(false);
            if (pages == null) {
                await ReplyWithFailedEmbedAsync(ctx, "No results found!")
                    .ConfigureAwait(false);
                return;
            }

            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, pages)
                .ConfigureAwait(false);
        }
        #endregion
    }
}
