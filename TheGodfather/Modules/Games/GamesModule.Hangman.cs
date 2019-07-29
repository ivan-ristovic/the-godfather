#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using Microsoft.Extensions.DependencyInjection;

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
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("hangman"), UsesInteractivity]
        [Description("Starts a hangman game.")]
        [Aliases("h", "hang")]
        public class HangmanModule : TheGodfatherServiceModule<ChannelEventService>
        {

            public HangmanModule(ChannelEventService service, SharedData shared, DatabaseContextBuilder db)
                : base(service, shared, db)
            {
                
            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another event is already running in the current channel!");

                DiscordDmChannel dm = await ctx.Client.CreateDmChannelAsync(ctx.User.Id);
                if (dm is null)
                    throw new CommandFailedException("Please enable direct messages, so I can ask you about the word to guess.");

                await dm.EmbedAsync("What is the secret word?", StaticDiscordEmoji.Question, this.ModuleColor);
                await this.InformAsync(ctx, StaticDiscordEmoji.Question, $"{ctx.User.Mention}, check your DM. When you give me the word, the game will start.");
                InteractivityResult<DiscordMessage> mctx = await ctx.Client.GetInteractivity().WaitForDmReplyAsync(
                    dm,
                    ctx.Channel.Id,
                    ctx.User.Id,
                    ctx.Services.GetService<InteractivityService>()
                );
                if (mctx.TimedOut) {
                    await this.InformFailureAsync(ctx, "I didn't get the word, so I will abort the game.");
                    return;
                } else {
                    await dm.EmbedAsync($"Alright! The word is: {Formatter.Bold(mctx.Result.Content)}", StaticDiscordEmoji.Information, this.ModuleColor);
                }

                var hangman = new HangmanGame(ctx.Client.GetInteractivity(), ctx.Channel, mctx.Result.Content, mctx.Result.Author);
                this.Service.RegisterEventInChannel(hangman, ctx.Channel.Id);
                try {
                    await hangman.RunAsync();

                    if (!(hangman.Winner is null))
                        await this.Database.UpdateStatsAsync(hangman.Winner.Id, s => s.HangmanWon++);
                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_HANGMAN_RULES
            [Command("rules")]
            [Description("Explain the Hangman game rules.")]
            [Aliases("help", "h", "ruling", "rule")]
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
