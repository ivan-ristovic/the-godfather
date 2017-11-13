#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Exceptions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion


namespace TheGodfather.Commands.Search
{
    [Group("youtube", CanInvokeWithoutSubcommand = true)]
    [Description("Youtube search commands.")]
    [Aliases("y", "yt")]
    [Cooldown(2, 5, CooldownBucketType.User), Cooldown(4, 5, CooldownBucketType.Channel)]
    [PreExecutionCheck]
    public class CommandsYoutube
    {

        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Search query.")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("Search query missing.");

            await SendYouTubeResults(ctx, query, 1)
                .ConfigureAwait(false);
        }


        #region COMMAND_YOUTUBE_SEARCH
        [Command("search")]
        [Description("Advanced youtube search.")]
        [Aliases("s")]
        public async Task AdvancedSearchAsync(CommandContext ctx,
                                             [Description("Ammount of results. [1-10]")] int ammount,
                                             [RemainingText, Description("Search query.")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("Search query missing.");
            if (ammount < 1 || ammount > 10)
                throw new CommandFailedException("Invalid ammount (must be 1-10).");

            await SendYouTubeResults(ctx, query, ammount)
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


        #region HELPER_FUNCTIONS
        private async Task SendYouTubeResults(CommandContext ctx, string query, int ammount, string type = null)
        {
            var em = await ctx.Dependencies.GetDependency<YoutubeService>().GetEmbeddedResults(query, ammount, type)
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Search result for {Formatter.Bold(query)}", embed: em)
                .ConfigureAwait(false);
        }
        #endregion
    }
}
