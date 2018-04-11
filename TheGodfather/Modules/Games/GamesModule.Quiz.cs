#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("quiz"), Module(ModuleType.Games)]
        [Description("List all available quiz categories.")]
        [Aliases("trivia", "q")]
        [UsageExample("!game quiz")]
        [ListeningCheck]
        public partial class QuizModule : TheGodfatherBaseModule
        {

            public QuizModule(DBService db) : base(db: db) { }


            [GroupCommand, Priority(1)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("ID of the quiz category")] int id)
            {
                if (Game.RunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another game is already running in the current channel.");

                var questions = await QuizService.GetQuizQuestionsAsync(id)
                    .ConfigureAwait(false);

                if (questions == null || !questions.Any())
                    throw new CommandFailedException("Either the ID is not correct or the category does not yet have enough questions for the quiz :(");

                var quiz = new Quiz(ctx.Client.GetInteractivity(), ctx.Channel, questions);
                Game.RegisterGameInChannel(quiz, ctx.Channel.Id);
                try {
                    await ctx.RespondWithIconEmbedAsync("Quiz will start in 10s! Get ready!", ":clock1:")
                        .ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromSeconds(10))
                        .ConfigureAwait(false);
                    await quiz.RunAsync()
                        .ConfigureAwait(false);

                    if (quiz.NoReply) {
                        await ctx.RespondWithIconEmbedAsync("Aborting quiz due to no replies...", ":alarm_clock:")
                            .ConfigureAwait(false);
                        return;
                    }

                    int n = quiz.Results.Count();
                    if (n == 0)
                        return;
                    if (n > 1) {
                        List<(DiscordUser, int)> results = new List<(DiscordUser, int)>();
                        foreach (var res in quiz.Results) {
                            var user = await ctx.Client.GetUserAsync(res.Item1)
                                .ConfigureAwait(false);
                            results.Add((user, res.Item2));
                        }
                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                            Title = "Results",
                            Description = string.Join("\n", results.Select(t => $"{t.Item1.Mention} : {t.Item2}")),
                            Color = DiscordColor.Azure
                        }.Build()).ConfigureAwait(false);
                        await Database.UpdateUserStatsAsync(results.First().Item1.Id, GameStatsType.QuizesWon)
                            .ConfigureAwait(false);
                    } else {
                        await ctx.RespondWithIconEmbedAsync("Trying to improve stats by playing alone? Won't work...", ":joy:")
                            .ConfigureAwait(false);
                    }
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }

            [GroupCommand, Priority(0)]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                var categories = await QuizService.GetQuizCategoriesAsync()
                    .ConfigureAwait(false);

                await ctx.RespondWithIconEmbedAsync(
                    $"You need to specify a quiz type!\n\n{Formatter.Bold("Available quiz categories:")}\n\n" +
                    $"- Custom quiz type (command): {Formatter.Bold("Capitals")}\n" +
                    $"- Custom quiz type (command): {Formatter.Bold("Countries")}\n" + 
                    string.Join("\n", categories.Select(c => $"{Formatter.Bold(c.Name)} (ID: {c.Id})")),
                    ":information_source:"
                ).ConfigureAwait(false);
            }


            #region COMMAND_QUIZ_STATS
            [Command("stats"), Module(ModuleType.Games)]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            [UsageExample("!game quiz stats")]
            public async Task StatsAsync(CommandContext ctx)
            {
                var top = await Database.GetTopQuizPlayersStringAsync(ctx.Client)
                    .ConfigureAwait(false);

                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Trophy, $"Top players in Quiz:\n\n{top}")
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
