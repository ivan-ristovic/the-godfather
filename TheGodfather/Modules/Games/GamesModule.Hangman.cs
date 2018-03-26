#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule : TheGodfatherBaseModule
    {
        [Group("hangman")]
        [Description("Starts a hangman game.")]
        [Aliases("h", "hang")]
        [UsageExample("!game hangman")]
        public class HangmanModule : TheGodfatherBaseModule
        {

            public HangmanModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (Game.RunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another game is already running in the current channel!");

                var dm = await ctx.Client.CreateDmChannelAsync(ctx.User.Id)
                    .ConfigureAwait(false);
                if (dm == null)
                    throw new CommandFailedException("Please enable direct messages, so I can ask you about the word to guess.");
                await dm.SendMessageAsync("What is the secret word?")
                    .ConfigureAwait(false);
                await ctx.RespondAsync(ctx.User.Mention + ", check your DM. When you give me the word, the game will start.")
                    .ConfigureAwait(false);
                var mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(
                    xm => xm.Channel == dm && xm.Author.Id == ctx.User.Id,
                    TimeSpan.FromMinutes(1)
                ).ConfigureAwait(false);
                if (mctx == null) {
                    await ctx.RespondAsync("I didn't get the word, so I will abort the game.")
                        .ConfigureAwait(false);
                    return;
                } else {
                    await dm.SendMessageAsync("Alright! The word is: " + Formatter.Bold(mctx.Message.Content))
                        .ConfigureAwait(false);
                }

                var hangman = new Hangman(ctx.Client.GetInteractivity(), ctx.Channel, mctx.Message.Content, mctx.User);
                Game.RegisterGameInChannel(hangman, ctx.Channel.Id);
                try {
                    await hangman.RunAsync()
                        .ConfigureAwait(false);
                    if (hangman.Winner != null)
                        await Database.UpdateUserStatsAsync(hangman.Winner.Id, GameStatsType.HangmansWon)
                            .ConfigureAwait(false);
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_HANGMAN_RULES
            [Command("rules")]
            [Description("Explain the Hangman game rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExample("!game hangman rules")]
            public async Task RulesAsync(CommandContext ctx)
            {
                await ctx.RespondWithIconEmbedAsync(
                    "\nI will ask a player for the word. Once he gives me the secret word, the other players try to guess by posting letters. For each failed guess you lose a \"life\"." +
                    " The game ends if the word is guessed or when all lives are spent.",
                    ":book:"
                ).ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_HANGMAN_STATS
            [Command("stats")]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            [UsageExample("!game hangman stats")]
            public async Task StatsAsync(CommandContext ctx)
            {
                var top = await Database.GetTopHangmanPlayersStringAsync(ctx.Client)
                    .ConfigureAwait(false);

                await ctx.RespondWithIconEmbedAsync(EmojiUtil.Trophy, $"Top players in Hangman:\n\n{top}")
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
