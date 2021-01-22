using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Modules.Games.Extensions;
using TheGodfather.Modules.Games.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("quiz")]
        [Aliases("trivia", "q")]
        [RequireGuild]
        public sealed class QuizModule : TheGodfatherServiceModule<ChannelEventService>
        {
            #region game quiz
            [GroupCommand, Priority(2)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("desc-game-quiz-cat-id")] int id,
                                               [Description("desc-game-quiz-amount")] int amount = 10,
                                               [Description("desc-game-quiz-diff")] int diff = 0)
            {
                if (amount is < 5 or > 20)
                    throw new CommandFailedException(ctx, "cmd-err-game-quiz-amount", 5, 20);

                if (diff is < 0 or > 2)
                    throw new CommandFailedException(ctx, "cmd-err-game-quiz-diff");

                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException(ctx, "cmd-err-evt-dup");

                IReadOnlyList<QuizQuestion>? questions = await QuizService.GetQuestionsAsync(id, amount, (QuestionDifficulty)diff);
                if (questions is null || !questions.Any())
                    throw new CommandFailedException(ctx, "cmd-err-game-quiz-cat");

                var quiz = new QuizGame(ctx.Client.GetInteractivity(), ctx.Channel, questions);
                await this.RunQuizAsync(ctx, quiz);
            }
            [GroupCommand, Priority(1)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("desc-game-quiz-cat-id")] string category,
                                               [Description("desc-game-quiz-diff")] int diff = 0,
                                               [Description("desc-game-quiz-amount")] int amount = 10)
            {
                int? id = await QuizService.GetCategoryIdAsync(category);
                if (id is null)
                    throw new CommandFailedException(ctx, "cmd-err-game-quiz-cat");
                await this.ExecuteGroupAsync(ctx, id.Value, amount, diff);
            }

            [GroupCommand, Priority(0)]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                IReadOnlyList<QuizCategory>? categories = await QuizService.GetCategoriesAsync();
                string catStr = categories?.Select(c => $"- {Formatter.Bold(c.Name)} (ID: {c.Id})").JoinWith() ?? "";
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Question, "fmt-game-quiz-cat", catStr);
            }
            #endregion

            #region game quiz capitals
            [Command("capitals")]
            [Aliases("capitaltowns")]
            public Task CapitalsQuizAsync(CommandContext ctx,
                                         [Description("desc-game-quiz-amount")] int amount = 10)
            {
                if (amount is < 5 or > 20)
                    throw new CommandFailedException(ctx, "cmd-err-game-quiz-amount", 5, 20);

                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException(ctx, "cmd-err-evt-dup");

                var quiz = new CapitalsQuizGame(ctx.Client.GetInteractivity(), ctx.Channel, amount);
                return this.RunQuizAsync(ctx, quiz);
            }
            #endregion 

            #region game quiz countries
            [Command("countries")]
            [Aliases("flags")]
            public Task CountriesQuizAsync(CommandContext ctx,
                                          [Description("desc-game-quiz-amount")] int amount = 10)
            {
                if (amount is < 5 or > 20)
                    throw new CommandFailedException(ctx, "cmd-err-game-quiz-amount", 5, 20);

                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException(ctx, "cmd-err-evt-dup");

                var quiz = new CountriesQuizGame(ctx.Client.GetInteractivity(), ctx.Channel, amount);
                return this.RunQuizAsync(ctx, quiz);
            }
            #endregion

            #region game quiz stats
            [Command("stats"), Priority(1)]
            [Aliases("s")]
            public Task StatsAsync(CommandContext ctx,
                                  [Description("desc-member")] DiscordMember? member = null)
                => this.StatsAsync(ctx, member as DiscordUser);

            [Command("stats"), Priority(0)]
            public async Task StatsAsync(CommandContext ctx,
                                        [Description("desc-user")] DiscordUser? user = null)
            {
                user ??= ctx.User;
                GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();

                GameStats? stats = await gss.GetAsync(user.Id);
                await ctx.RespondWithLocalizedEmbedAsync(emb => {
                    emb.WithLocalizedTitle("fmt-game-stats", user.ToDiscriminatorString());
                    emb.WithColor(this.ModuleColor);
                    emb.WithThumbnail(user.AvatarUrl);
                    if (stats is null)
                        emb.WithLocalizedDescription("str-game-stats-none");
                    else
                        emb.WithDescription(stats.BuildQuizStatsString());
                });
            }
            #endregion

            #region game quiz top
            [Command("top")]
            [Aliases("t", "leaderboard")]
            public async Task TopAsync(CommandContext ctx)
            {
                GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                IReadOnlyList<GameStats> topStats = await gss.GetTopQuizStatsAsync();
                string top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildQuizStatsString());
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, "fmt-game-quiz-top", top);
            }
            #endregion


            #region internals
            private async Task RunQuizAsync(CommandContext ctx, IQuiz quiz)
            {
                this.Service.RegisterEventInChannel(quiz, ctx.Channel.Id);
                try {
                    await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, "str-game-quiz-start");
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    await quiz.RunAsync(this.Localization);

                    if (quiz.IsTimeoutReached)
                        await ctx.FailAsync("cmd-err-game-timeout");
                    else
                        await this.HandleQuizResultsAsync(ctx, quiz.Results);
                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }

            private async Task HandleQuizResultsAsync(CommandContext ctx, IReadOnlyDictionary<DiscordUser, int> results)
            {
                if (results.Any()) {
                    var ordered = results.OrderByDescending(kvp => kvp.Value).ToList();

                    if (results.Count > 0) {
                        await ctx.RespondWithLocalizedEmbedAsync(emb => {
                            emb.WithLocalizedTitle("str-results");
                            emb.WithDescription(ordered.Take(10).Select(kvp => $"{kvp.Key.Mention} : {kvp.Value}").JoinWith());
                            emb.WithColor(this.ModuleColor);
                        });
                        if (results.Count > 1) {
                            GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                            await gss.UpdateStatsAsync(ordered.First().Key.Id, s => s.QuizWon++);
                        }
                    }
                }
            }
            #endregion
        }
    }
}
