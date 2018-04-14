#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
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
        [UsageExample("!game quiz countries")]
        [UsageExample("!game quiz 9")]
        [UsageExample("!game quiz 9 hard")]
        [UsageExample("!game quiz 9 hard 15")]
        [ListeningCheck]
        public class QuizModule : TheGodfatherBaseModule
        {

            public QuizModule(DBService db) : base(db: db) { }


            [GroupCommand, Priority(2)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("ID of the quiz category.")] int id,
                                               [Description("Amount of questions.")] int amount = 10,
                                               [Description("Difficulty. (easy/medium/hard)")] string diff = "easy")
            {
                if (Game.RunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another game is already running in the current channel.");

                if (amount < 5 || amount > 50)
                    throw new CommandFailedException("Invalid amount of questions specified. Amount has to be in range [5-50]!");

                QuestionDifficulty difficulty = QuestionDifficulty.Easy;
                switch (diff.ToLowerInvariant()) {
                    case "medium": difficulty = QuestionDifficulty.Medium; break;
                    case "hard": difficulty = QuestionDifficulty.Hard; break;
                }

                var questions = await QuizService.GetQuizQuestionsAsync(id, amount, difficulty)
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

                    await HandleQuizResultsAsync(ctx, quiz.Results)
                        .ConfigureAwait(false);
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }
            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("ID of the quiz category.")] int id,
                                         [Description("Difficulty. (easy/medium/hard)")] string diff = "easy",
                                         [Description("Amount of questions.")] int amount = 10)
                => ExecuteGroupAsync(ctx, id, amount, diff);

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


            #region COMMAND_QUIZ_CAPITALS

            [Command("capitals"), Module(ModuleType.Games)]
            [Description("Country capitals guessing quiz. You can also specify how many questions there will be in the quiz.")]
            [Aliases("capitaltowns")]
            [UsageExample("!game quiz capitals")]
            [UsageExample("!game quiz capitals 15")]
            public async Task CapitalsQuizAsync(CommandContext ctx,
                                               [Description("Number of questions.")] int qnum = 10)
            {
                if (qnum < 5 || qnum > 50)
                    throw new InvalidCommandUsageException("Number of questions must be in range [5-50]");

                if (Game.RunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another game is already running in the current channel.");

                try {
                    QuizCapitals.LoadCapitals();
                } catch {
                    throw new CommandFailedException("Failed to load country capitals!");
                }

                var quiz = new QuizCapitals(ctx.Client.GetInteractivity(), ctx.Channel, qnum);
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

                    await HandleQuizResultsAsync(ctx, quiz.Results)
                        .ConfigureAwait(false);
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }
            #endregion

            #region COMMAND_QUIZ_COUNTRIES
            [Command("countries"), Module(ModuleType.Games)]
            [Description("Country flags guessing quiz. You can also specify how many questions there will be in the quiz.")]
            [Aliases("flags")]
            [UsageExample("!game quiz countries")]
            [UsageExample("!game quiz countries 15")]
            public async Task CountriesQuizAsync(CommandContext ctx,
                                                [Description("Number of questions.")] int qnum = 10)
            {
                if (qnum < 5 || qnum > 50)
                    throw new InvalidCommandUsageException("Number of questions must be in range [5-50]");

                if (Game.RunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another game is already running in the current channel.");

                try {
                    QuizCountries.LoadCountries();
                } catch {
                    throw new CommandFailedException("Failed to load country flags!");
                }

                var quiz = new QuizCountries(ctx.Client.GetInteractivity(), ctx.Channel, qnum);
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

                    await HandleQuizResultsAsync(ctx, quiz.Results)
                        .ConfigureAwait(false);
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }
            #endregion

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


            #region HELPER_FUNCTIONS
            private async Task HandleQuizResultsAsync(CommandContext ctx, ConcurrentDictionary<DiscordUser, int> results)
            {
                if (results.Any()) {
                    var ordered = results.OrderByDescending(kvp => kvp.Value);
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                        Title = "Results",
                        Description = string.Join("\n", ordered.Select(kvp => $"{kvp.Key.Mention} : {kvp.Value}")),
                        Color = DiscordColor.Azure
                    }.Build()).ConfigureAwait(false);

                    if (results.Count > 1)
                        await Database.UpdateUserStatsAsync(ordered.First().Key.Id, GameStatsType.QuizesWon)
                            .ConfigureAwait(false);
                }
            }
            #endregion
        }
    }
}
