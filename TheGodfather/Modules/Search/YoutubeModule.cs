#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;

using DSharpPlus;
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
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("Search query missing.");

            var pages = await Service.GetPaginatedResults(query, 5)
                .ConfigureAwait(false);
            if (pages == null) {
                await ReplyWithFailedEmbedAsync(ctx, "No results found!")
                    .ConfigureAwait(false);
                return;
            }

            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, pages)
                .ConfigureAwait(false);
        }

        /*
        #region COMMAND_YOUTUBE_SEARCH
        [Command("search")]
        [Description("Advanced youtube search.")]
        [Aliases("s")]
        public async Task AdvancedSearchAsync(CommandContext ctx,
                                             [Description("Amount of results. [1-10]")] int amount,
                                             [RemainingText, Description("Search query.")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("Search query missing.");
            if (amount < 1 || amount > 10)
                throw new CommandFailedException("Invalid amount (must be 1-10).");

            await SendYouTubeResults(ctx, query, amount)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_YOUTUBE_SEARCHVIDEO
        [Command("searchv")]
        [Description("Advanced youtube search for videos only.")]
        [Aliases("sv", "searchvideo")]
        public async Task SearchVideoAsync(CommandContext ctx,
                                          [RemainingText, Description("Search query.")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("Search query missing.");

            await SendYouTubeResults(ctx, query, 5, "video")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_YOUTUBE_SEARCHCHANNEL
        [Command("searchc")]
        [Description("Advanced youtube search for channels only.")]
        [Aliases("sc", "searchchannel")]
        public async Task SearchChannelAsync(CommandContext ctx,
                                            [RemainingText, Description("Search query.")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("Search query missing.");

            await SendYouTubeResults(ctx, query, 5, "channel")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_YOUTUBE_SEARCHPLAYLIST
        [Command("searchp")]
        [Description("Advanced youtube search for playlists only.")]
        [Aliases("sp", "searchplaylist")]
        public async Task SearchPlaylistAsync(CommandContext ctx,
                                             [RemainingText, Description("Search query.")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("Search query missing.");

            await SendYouTubeResults(ctx, query, 5, "playlist")
                .ConfigureAwait(false);
        }
        #endregion
        */
    }
}
