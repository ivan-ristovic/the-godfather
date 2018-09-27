#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Search.Extensions;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("youtube"), Module(ModuleType.Searches), NotBlocked]
    [Description("Youtube search commands. Group call searches YouTube for given query.")]
    [Aliases("y", "yt", "ytube")]
    [UsageExamples("!youtube never gonna give you up")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class YoutubeModule : TheGodfatherServiceModule<YtService>
    {

        public YoutubeModule(YtService yt, SharedData shared, DBService db) 
            : base(yt, shared, db)
        {
            this.ModuleColor = DiscordColor.Red;
        }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("Search query.")] string query)
            => this.SearchAndSendResultsAsync(ctx, 5, query);


        #region COMMAND_YOUTUBE_SEARCH
        [Command("search")]
        [Description("Advanced youtube search.")]
        [Aliases("s")]
        [UsageExamples("!youtube search 5 rick astley")]
        public Task AdvancedSearchAsync(CommandContext ctx,
                                       [Description("Amount of results. [1-20]")] int amount,
                                       [RemainingText, Description("Search query.")] string query)
            => this.SearchAndSendResultsAsync(ctx, amount, query);
        #endregion

        #region COMMAND_YOUTUBE_SEARCHVIDEO
        [Command("searchvideo")]
        [Description("Advanced youtube search for videos only.")]
        [Aliases("sv", "searchv")]
        [UsageExamples("!youtube searchvideo 5 rick astley")]
        public Task SearchVideoAsync(CommandContext ctx,
                                    [RemainingText, Description("Search query.")] string query)
            => this.SearchAndSendResultsAsync(ctx, 5, query, "video");
        #endregion

        #region COMMAND_YOUTUBE_SEARCHCHANNEL
        [Command("searchchannel")]
        [Description("Advanced youtube search for channels only.")]
        [Aliases("sc", "searchc")]
        [UsageExamples("!youtube searchchannel 5 rick astley")]
        public Task SearchChannelAsync(CommandContext ctx,
                                      [RemainingText, Description("Search query.")] string query)
            => this.SearchAndSendResultsAsync(ctx, 5, query, "channel");
        #endregion

        #region COMMAND_YOUTUBE_SEARCHPLAYLIST
        [Command("searchp")]
        [Description("Advanced youtube search for playlists only.")]
        [Aliases("sp", "searchplaylist")]
        [UsageExamples("!youtube searchplaylist 5 rick astley")]
        public Task SearchPlaylistAsync(CommandContext ctx,
                                       [RemainingText, Description("Search query.")] string query)
            => this.SearchAndSendResultsAsync(ctx, 5, query, "playlist");
        #endregion

        #region COMMAND_YOUTUBE_SUBSCRIBE
        [Command("subscribe")]
        [Description("Add a new subscription for a YouTube channel.")]
        [Aliases("add", "a", "+", "sub")]
        [UsageExamples("!youtube subscribe https://www.youtube.com/user/RickAstleyVEVO",
                       "!youtube subscribe https://www.youtube.com/user/RickAstleyVEVO rick")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public Task SubscribeAsync(CommandContext ctx,
                                  [Description("Channel URL.")] string url,
                                  [Description("Friendly name.")] string name = null)
        {
            string command = $"sub yt {url} {name}";
            Command cmd = ctx.CommandsNext.FindCommand(command, out string args);
            CommandContext fctx = ctx.CommandsNext.CreateFakeContext(ctx.Member, ctx.Channel, command, ctx.Prefix, cmd, args);
            return ctx.CommandsNext.ExecuteCommandAsync(fctx);
        }
        #endregion

        #region COMMAND_YOUTUBE_UNSUBSCRIBE
        [Command("unsubscribe")]
        [Description("Remove a YouTube channel subscription.")]
        [Aliases("del", "d", "rm", "-", "unsub")]
        [UsageExamples("!youtube unsubscribe https://www.youtube.com/user/RickAstleyVEVO",
                       "!youtube unsubscribe rick")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public Task UnsubscribeAsync(CommandContext ctx,
                                    [Description("Channel URL or subscription name.")] string name_url)
        {
            string command = $"unsub yt {name_url}";
            Command cmd = ctx.CommandsNext.FindCommand(command, out string args);
            CommandContext fctx = ctx.CommandsNext.CreateFakeContext(ctx.Member, ctx.Channel, command, ctx.Prefix, cmd, args);
            return ctx.CommandsNext.ExecuteCommandAsync(fctx);
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task SearchAndSendResultsAsync(CommandContext ctx, int amount, string query, string type = null)
        {
            if (this.Service.IsDisabled())
                throw new ServiceDisabledException();

            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("Search query missing.");

            IReadOnlyList<Page> pages = await this.Service.GetPaginatedResultsAsync(query, amount, type);
            if (pages == null) {
                await this.InformFailureAsync(ctx, "No results found!");
                return;
            }

            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, pages);
        }
        #endregion
    }
}
