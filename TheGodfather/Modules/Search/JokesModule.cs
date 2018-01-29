#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("joke", CanInvokeWithoutSubcommand = true)]
    [Description("Send a joke.")]
    [Aliases("jokes", "j")]
    [Cooldown(2, 5, CooldownBucketType.User), Cooldown(4, 5, CooldownBucketType.Channel)]
    [PreExecutionCheck]
    public class JokesModule
    {

        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            string joke = null;
            try {
                joke = await JokesService.GetRandomJokeAsync()
                    .ConfigureAwait(false);
            } catch (WebException e) {
                throw new CommandFailedException("Connection to remote site failed!", e);
            } catch (Exception e) {
                throw new CommandFailedException("Exception occured!", e);
            }

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() { Description = joke }.Build())
                .ConfigureAwait(false);
        }


        #region COMMAND_JOKE_SEARCH
        [Command("search")]
        [Description("Search for the joke containing the query.")]
        [Aliases("s")]
        public async Task SearchAsync(CommandContext ctx,
                                     [RemainingText, Description("Query.")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("Query missing.");

            string jokes = null;
            try {
                jokes = await JokesService.SearchForJokesAsync(query)
                    .ConfigureAwait(false);
            } catch (WebException e) {
                throw new CommandFailedException("Connection to remote site failed!", e);
            } catch (Exception e) {
                throw new CommandFailedException("Exception occured!", e);
            }

            if (string.IsNullOrWhiteSpace(jokes))
                jokes = "No results...";

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Description = string.Join("\n\n", jokes.Split('\n').Take(10))
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_JOKE_YOURMOM
        [Command("yourmom")]
        [Description("Yo mama so...")]
        [Aliases("mama", "m", "yomomma", "yomom", "yomoma", "yomamma", "yomama")]
        public async Task YomamaAsync(CommandContext ctx)
        {
            try {
                var joke = await JokesService.GetYoMommaJokeAsync();
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                    Description = joke
                }.Build()).ConfigureAwait(false);
            } catch (WebException e) {
                throw new CommandFailedException("Connection to remote site failed!", e);
            } catch (Exception e) {
                throw new CommandFailedException("Exception occured!", e);
            }
        }
        #endregion
    }
}
