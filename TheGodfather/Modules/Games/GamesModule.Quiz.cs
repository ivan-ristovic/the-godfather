#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Modules.Games.Extensions;
using TheGodfather.Modules.Games.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("quiz")]
        [Description("Play a quiz! Group call lists all available quiz categories.")]
        [Aliases("trivia", "q")]
        [UsageExamples("!game quiz",
                       "!game quiz countries",
                       "!game quiz 9",
                       "!game quiz history",
                       "!game quiz history hard",
                       "!game quiz history hard 15",
                       "!game quiz 9 hard",
                       "!game quiz 9 hard 15")]
        public class QuizModule : TheGodfatherModule
        {

            public QuizModule(SharedData shared, DBService db) 
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Teal;
            }


            [GroupCommand, Priority(4)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("ID of the quiz category.")] int id,
                                               [Description("Amount of questions.")] int amount = 10,
                                               [Description("Difficulty. (easy/medium/hard)")] string diff = "easy")
            {
                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another event is already running in the current channel.");

                if (amount < 5 || amount > 50)
                    throw new CommandFailedException("Invalid amount of questions specified. Amount has to be in range [5, 50]!");

                QuestionDifficulty difficulty = QuestionDifficulty.Easy;
                switch (diff.ToLowerInvariant()) {
                    case "medium": difficulty = QuestionDifficulty.Medium; break;
                    case "hard": difficulty = QuestionDifficulty.Hard; break;
                }

                IReadOnlyList<QuizQuestion> questions = await QuizService.GetQuestionsAsync(id, amount, difficulty);
                if (questions is null || !questions.Any())
                    throw new CommandFailedException("Either the ID is not correct or the category does not yet have enough questions for the quiz :(");

                var quiz = new Quiz(ctx.Client.GetInteractivity(), ctx.Channel, questions);
                this.Shared.RegisterEventInChannel(quiz, ctx.Channel.Id);
                try {
                    await this.InformAsync(ctx, StaticDiscordEmoji.Clock1, "Quiz will start in 10s! Get ready!");
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    await quiz.RunAsync();

                    if (quiz.IsTimeoutReached) {
                        await this.InformAsync(ctx, StaticDiscordEmoji.AlarmClock, "Aborting quiz due to no replies...");
                        return;
                    }

                    await this.HandleQuizResultsAsync(ctx, quiz.Results);
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }

            [GroupCommand, Priority(3)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("ID of the quiz category.")] int id,
                                         [Description("Difficulty. (easy/medium/hard)")] string diff = "easy",
                                         [Description("Amount of questions.")] int amount = 10)
                => this.ExecuteGroupAsync(ctx, id, amount, diff);

            [GroupCommand, Priority(2)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Quiz category.")] string category,
                                               [Description("Difficulty. (easy/medium/hard)")] string diff = "easy",
                                               [Description("Amount of questions.")] int amount = 10)
            {
                int? id = await QuizService.GetCategoryIdAsync(category);
                if (!id.HasValue)
                    throw new CommandFailedException("Category with that name doesn't exist!");

                await this.ExecuteGroupAsync(ctx, id.Value, amount, diff);
            }

            [GroupCommand, Priority(1)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [RemainingText, Description("Quiz category.")] string category)
            {
                int? id = await QuizService.GetCategoryIdAsync(category);
                if (!id.HasValue)
                    throw new CommandFailedException("Category with that name doesn't exist!");

                await this.ExecuteGroupAsync(ctx, id.Value, 10);
            }

            [GroupCommand, Priority(0)]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                IReadOnlyList<QuizCategory> categories = await QuizService.GetCategoriesAsync();

                await this.InformAsync(ctx,
                    StaticDiscordEmoji.Information,
                    $"You need to specify a quiz type!\n\n{Formatter.Bold("Available quiz categories:")}\n\n" +
                    $"- Custom quiz type (command): {Formatter.Bold("Capitals")}\n" +
                    $"- Custom quiz type (command): {Formatter.Bold("Countries")}\n" +
                    string.Join("\n", categories.Select(c => $"{Formatter.Bold(c.Name)} (ID: {c.Id})"))
                );
            }


            #region COMMAND_QUIZ_CAPITALS
            [Command("capitals")]
            [Description("Country capitals guessing quiz. You can also specify how many questions there will be in the quiz.")]
            [Aliases("capitaltowns")]
            [UsageExamples("!game quiz capitals",
                           "!game quiz capitals 15")]
            public async Task CapitalsQuizAsync(CommandContext ctx,
                                               [Description("Number of questions.")] int qnum = 10)
            {
                if (qnum < 5 || qnum > 50)
                    throw new InvalidCommandUsageException("Number of questions must be in range [5, 50]");

                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another event is already running in the current channel.");
                
                var quiz = new CapitalsQuiz(ctx.Client.GetInteractivity(), ctx.Channel, qnum);
                this.Shared.RegisterEventInChannel(quiz, ctx.Channel.Id);
                try {
                    await this.InformAsync(ctx, StaticDiscordEmoji.Clock1, "Quiz will start in 10s! Get ready!");
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    await quiz.RunAsync();

                    if (quiz.IsTimeoutReached) {
                        await this.InformAsync(ctx, StaticDiscordEmoji.AlarmClock, "Aborting quiz due to no replies...");
                        return;
                    }

                    await this.HandleQuizResultsAsync(ctx, quiz.Results);
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }
            #endregion

            #region COMMAND_QUIZ_COUNTRIES
            [Command("countries")]
            [Description("Country flags guessing quiz. You can also specify how many questions there will be in the quiz.")]
            [Aliases("flags")]
            [UsageExamples("!game quiz countries",
                           "!game quiz countries 15")]
            public async Task CountriesQuizAsync(CommandContext ctx,
                                                [Description("Number of questions.")] int qnum = 10)
            {
                if (qnum < 5 || qnum > 50)
                    throw new InvalidCommandUsageException("Number of questions must be in range [5-50]");

                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another event is already running in the current channel.");

                var quiz = new CountriesQuiz(ctx.Client.GetInteractivity(), ctx.Channel, qnum);
                this.Shared.RegisterEventInChannel(quiz, ctx.Channel.Id);
                try {
                    await this.InformAsync(ctx, StaticDiscordEmoji.Clock1, "Quiz will start in 10s! Get ready!");
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    await quiz.RunAsync();

                    if (quiz.IsTimeoutReached) {
                        await this.InformAsync(ctx, StaticDiscordEmoji.AlarmClock, "Aborting quiz due to no replies...");
                        return;
                    }

                    await this.HandleQuizResultsAsync(ctx, quiz.Results);
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }
            #endregion

            #region COMMAND_QUIZ_STATS
            [Command("stats")]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            [UsageExamples("!game quiz stats")]
            public async Task StatsAsync(CommandContext ctx)
            {
                IReadOnlyList<DatabaseGameStats> topStats = await this.DatabaseBuilder.GetTopQuizStatsAsync();
                string top = await DatabaseGameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildQuizStatsString());
                await this.InformAsync(ctx, StaticDiscordEmoji.Trophy, $"Top players in Quiz:\n\n{top}");
            }
            #endregion


            #region HELPER_FUNCTIONS
            private async Task HandleQuizResultsAsync(CommandContext ctx, ConcurrentDictionary<DiscordUser, int> results)
            {
                if (results.Any()) {
                    IOrderedEnumerable<KeyValuePair<DiscordUser, int>> ordered = results.OrderByDescending(kvp => kvp.Value);
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                        Title = "Results",
                        Description = string.Join("\n", ordered.Select(kvp => $"{kvp.Key.Mention} : {kvp.Value}")),
                        Color = this.ModuleColor
                    }.Build());

                    if (results.Count > 1)
                        await this.DatabaseBuilder.UpdateStatsAsync(ordered.First().Key.Id, s => s.QuizesWon++);
                }
            }
            #endregion
        }
    }
}
