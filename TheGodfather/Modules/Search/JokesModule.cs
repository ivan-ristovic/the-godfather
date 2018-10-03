#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("joke"), Module(ModuleType.Searches), NotBlocked]
    [Description("Group for searching jokes. Group call returns a random joke.")]
    [Aliases("jokes", "j")]
    [UsageExamples("!joke")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class JokesModule : TheGodfatherModule
    {

        public JokesModule(SharedData shared, DBService db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.PhthaloBlue;
        }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            string joke = await JokesService.GetRandomJokeAsync();
            await this.InformAsync(ctx, joke, ":joy:");
        }


        #region COMMAND_JOKE_SEARCH
        [Command("search")]
        [Description("Search for the joke containing the given query.")]
        [Aliases("s")]
        [UsageExamples("!joke search blonde")]
        public async Task SearchAsync(CommandContext ctx,
                                     [RemainingText, Description("Query.")] string query)
        {
            IReadOnlyList<string> jokes = await JokesService.SearchForJokesAsync(query);
            if (!(jokes is null))
                await this.InformAsync(ctx, $"Results:\n\n{string.Join("\n", jokes.Take(5))}", ":joy:");
            else
                await this.InformFailureAsync(ctx, "No results...");
        }
        #endregion

        #region COMMAND_JOKE_YOURMOM
        [Command("yourmom")]
        [Description("Yo mama so...")]
        [Aliases("mama", "m", "yomomma", "yomom", "yomoma", "yomamma", "yomama")]
        [UsageExamples("!joke yourmom")]
        public async Task YomamaAsync(CommandContext ctx)
        {
            string joke = await JokesService.GetRandomYoMommaJokeAsync();
            await this.InformAsync(ctx, joke, ":joy:");
        }
        #endregion
    }
}
