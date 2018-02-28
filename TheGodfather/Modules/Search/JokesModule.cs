#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("joke")]
    [Description("Group for searching jokes. If invoked without a subcommand, returns a random joke.")]
    [Aliases("jokes", "j")]
    [UsageExample("!joke")]
    [Cooldown(2, 5, CooldownBucketType.User), Cooldown(4, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
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
                Logger.LogException(LogLevel.Warning, e);
                throw new CommandFailedException("Failed to retrieve a joke. Please report this.");
            }

            await ReplyWithEmbedAsync(ctx, joke, ":joy:")
                .ConfigureAwait(false);
        }


        #region COMMAND_JOKE_SEARCH
        [Command("search")]
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
                await ReplyWithEmbedAsync(ctx, "No results...", ":frowning:")
                    .ConfigureAwait(false);
                return;
            }
            await ReplyWithEmbedAsync(ctx, $"Results:\n\n{string.Join("\n", jokes.Take(5))}", ":joy:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_JOKE_YOURMOM
        [Command("yourmom")]
        [Description("Yo mama so...")]
        [Aliases("mama", "m", "yomomma", "yomom", "yomoma", "yomamma", "yomama")]
        [UsageExample("!joke yourmom")]
        public async Task YomamaAsync(CommandContext ctx)
        {
            var joke = await JokesService.GetYoMommaJokeAsync()
                .ConfigureAwait(false);
            if (joke == null)
                throw new CommandFailedException("Failed to retrieve joke. Please report this.");

            await ReplyWithEmbedAsync(ctx, joke, ":joy:")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
