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
    [UsageExample("!joke")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class JokesModule : TheGodfatherBaseModule
    {

        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            string joke = null;
            try {
                joke = await JokesService.GetRandomJokeAsync()
                    .ConfigureAwait(false);
            } catch (Exception e) {
                TheGodfather.LogProvider.LogException(LogLevel.Warning, e);
                throw new CommandFailedException("Failed to retrieve a joke. Please report this.");
            }

            await ctx.RespondWithIconEmbedAsync(joke, ":joy:")
                .ConfigureAwait(false);
        }


        #region COMMAND_JOKE_SEARCH
        [Command("search"), Module(ModuleType.Searches)]
        [Description("Search for the joke containing the given query.")]
        [Aliases("s")]
        [UsageExample("!joke search blonde")]
        public async Task SearchAsync(CommandContext ctx,
                                     [RemainingText, Description("Query.")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("Query missing.");

            var jokes = await JokesService.SearchForJokesAsync(query)
                .ConfigureAwait(false);
            if (jokes == null)
                throw new CommandFailedException("Failed to retrieve joke. Please report this.");

            if (!jokes.Any()) {
                await ctx.RespondWithIconEmbedAsync("No results...", ":frowning:")
                    .ConfigureAwait(false);
                return;
            }
            await ctx.RespondWithIconEmbedAsync($"Results:\n\n{string.Join("\n", jokes.Take(5))}", ":joy:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_JOKE_YOURMOM
        [Command("yourmom"), Module(ModuleType.Searches)]
        [Description("Yo mama so...")]
        [Aliases("mama", "m", "yomomma", "yomom", "yomoma", "yomamma", "yomama")]
        [UsageExample("!joke yourmom")]
        public async Task YomamaAsync(CommandContext ctx)
        {
            var joke = await JokesService.GetYoMommaJokeAsync()
                .ConfigureAwait(false);
            if (joke == null)
                throw new CommandFailedException("Failed to retrieve joke. Please report this.");

            await ctx.RespondWithIconEmbedAsync(joke, ":joy:")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
