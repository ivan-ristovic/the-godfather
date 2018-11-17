#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Extensions;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("hangman")]
        [Description("Starts a hangman game.")]
        [Aliases("h", "hang")]
        [UsageExamples("!game hangman")]
        public class HangmanModule : TheGodfatherModule
        {

            public HangmanModule(SharedData shared, DatabaseContextBuilder db) 
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Teal;
            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another event is already running in the current channel!");

                DiscordDmChannel dm = await ctx.Client.CreateDmChannelAsync(ctx.User.Id);
                if (dm is null)
                    throw new CommandFailedException("Please enable direct messages, so I can ask you about the word to guess.");

                await dm.EmbedAsync("What is the secret word?", StaticDiscordEmoji.Question, this.ModuleColor);
                await this.InformAsync(ctx, StaticDiscordEmoji.Question, $"{ctx.User.Mention}, check your DM. When you give me the word, the game will start.");
                MessageContext mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(
                    xm => xm.Channel == dm && xm.Author.Id == ctx.User.Id,
                    TimeSpan.FromMinutes(1)
                );
                if (mctx is null) {
                    await this.InformFailureAsync(ctx, "I didn't get the word, so I will abort the game.");
                    return;
                } else {
                    await dm.EmbedAsync($"Alright! The word is: {Formatter.Bold(mctx.Message.Content)}", StaticDiscordEmoji.Information, this.ModuleColor);
                }

                var hangman = new HangmanGame(ctx.Client.GetInteractivity(), ctx.Channel, mctx.Message.Content, mctx.User);
                this.Shared.RegisterEventInChannel(hangman, ctx.Channel.Id);
                try {
                    await hangman.RunAsync();

                    if (!(hangman.Winner is null))
                        await this.Database.UpdateStatsAsync(hangman.Winner.Id, s => s.HangmanWon++);
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_HANGMAN_RULES
            [Command("rules")]
            [Description("Explain the Hangman game rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExamples("!game hangman rules")]
            public Task RulesAsync(CommandContext ctx)
            {
                return this.InformAsync(ctx, 
                    StaticDiscordEmoji.Information,
                    "\nI will ask a player for the word. Once he gives me the secret word, the other players try to guess by posting letters. For each failed guess you lose a \"life\"." +
                    " The game ends if the word is guessed or when all lives are spent."
                );
            }
            #endregion

            #region COMMAND_HANGMAN_STATS
            [Command("stats")]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            [UsageExamples("!game hangman stats")]
            public async Task StatsAsync(CommandContext ctx)
            {
                IReadOnlyList<DatabaseGameStats> topStats = await this.Database.GetTopHangmanStatsAsync();
                string top = await DatabaseGameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildHangmanStatsString());
                await this.InformAsync(ctx, StaticDiscordEmoji.Trophy, $"Top players in Hangman:\n\n{top}");
            }
            #endregion
        }
    }
}
