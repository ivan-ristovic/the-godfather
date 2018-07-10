#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("joke"), Module(ModuleType.Searches)]
    [Description("Group for searching jokes. If invoked without a subcommand, returns a random joke.")]
    [Aliases("jokes", "j")]
    [UsageExamples("!joke")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class JokesModule : TheGodfatherBaseModule
    {

        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            string joke = await JokesService.GetRandomJokeAsync()
                .ConfigureAwait(false);
            await ctx.InformSuccessAsync(joke, ":joy:")
                .ConfigureAwait(false);
        }


        #region COMMAND_JOKE_SEARCH
        [Command("search"), Module(ModuleType.Searches)]
        [Description("Search for the joke containing the given query.")]
        [Aliases("s")]
        [UsageExamples("!joke search blonde")]
        public async Task SearchAsync(CommandContext ctx,
                                     [RemainingText, Description("Query.")] string query)
        {
            var jokes = await JokesService.SearchForJokesAsync(query)
                .ConfigureAwait(false);
            if (jokes != null) {
                await ctx.InformSuccessAsync($"Results:\n\n{string.Join("\n", jokes.Take(5))}", ":joy:")
                    .ConfigureAwait(false);
            } else {
                await ctx.InformFailureAsync("No results...")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_JOKE_YOURMOM
        [Command("yourmom"), Module(ModuleType.Searches)]
        [Description("Yo mama so...")]
        [Aliases("mama", "m", "yomomma", "yomom", "yomoma", "yomamma", "yomama")]
        [UsageExamples("!joke yourmom")]
        public async Task YomamaAsync(CommandContext ctx)
        {
            string joke = await JokesService.GetRandomYoMommaJokeAsync()
                .ConfigureAwait(false);
            await ctx.InformSuccessAsync(joke, ":joy:")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
