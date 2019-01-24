#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("wikipedia"), Module(ModuleType.Searches), NotBlocked]
    [Description("Wikipedia search. If invoked without a subcommand, searches Wikipedia with given query.")]
    [Aliases("wiki")]
    [UsageExamples("!wikipedia wat")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class WikiModule : TheGodfatherModule
    {

        public WikiModule(SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Violet;
        }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Query.")] string query)
        {
            var res = await WikiService.SearchAsync(query);
            if (res is null){ // TODO || !res.Any()) {
                await this.InformFailureAsync(ctx, "No results...");
                return;
            }

            // TODO
            var r = res[0];

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = r.Title,
                Description = r.Snippet,
                Color = this.ModuleColor,
                Url = r.Url
            }.Build());
        }
    }
}
