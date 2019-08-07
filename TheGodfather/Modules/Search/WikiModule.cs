#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("wikipedia"), Module(ModuleType.Searches), NotBlocked]
    [Description("Wikipedia search. If invoked without a subcommand, searches Wikipedia with given query.")]
    [Aliases("wiki")]
    
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class WikiModule : TheGodfatherModule
    {

        public WikiModule(DatabaseContextBuilder db)
            : base(db)
        {
            
        }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("Query.")] string query)
            => this.SearchAsync(ctx, query);


        #region COMMAND_WIKI_SEARCH
        [Command("search")]
        [Description("Search Wikipedia for a given query.")]
        [Aliases("s", "find")]
        
        public async Task SearchAsync(CommandContext ctx,
                                     [RemainingText, Description("Query.")] string query)
        {
            WikiSearchResponse res = await WikiService.SearchAsync(query);
            if (res is null || !res.Any()) {
                await this.InformFailureAsync(ctx, "No results...");
                return;
            }

            await ctx.Client.GetInteractivity().SendPaginatedMessageAsync(ctx.Channel, ctx.User, res.Select(r => new Page(embed:
                new DiscordEmbedBuilder {
                    Title = r.Title,
                    Description = string.IsNullOrWhiteSpace(r.Snippet) ? "No description provided" : r.Snippet,
                    Url = r.Url,
                    Color = this.ModuleColor
                }.WithFooter("Powered by Wikipedia API", WikiService.WikipediaIconUrl)
            )));
        }
        #endregion
    }
}
