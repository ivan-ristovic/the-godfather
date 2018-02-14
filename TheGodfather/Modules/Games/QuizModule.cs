#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Services;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Games.Common;

using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("quiz")]
        [Description("List all available quiz categories.")]
        [Aliases("trivia", "q")]
        [UsageExample("!game quiz ")]
        [Cooldown(2, 5, CooldownBucketType.User), Cooldown(3, 5, CooldownBucketType.Channel)]
        [ListeningCheck]
        public class QuizModule : GodfatherBaseModule
        {

            public QuizModule(DatabaseService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                await ReplyWithEmbedAsync(
                    ctx,
                    "Available quiz categories:" +
                    $"- {Formatter.Bold("countries")}",
                    ":information_source:"
                ).ConfigureAwait(false);
            }


            #region COMMAND_QUIZ_COUNTRIES
            [Command("countries")]
            [Description("Country flags guessing quiz.")]
            [Aliases("flags", "c")]
            public async Task CountriesQuizAsync(CommandContext ctx)
            {
                QuizCountries.LoadCountries();

                if (Game.RunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another game is already running in the current channel.");


                var quiz = new QuizCountries(ctx.Client.GetInteractivity(), ctx.Channel);
                Game.RegisterGameInChannel(quiz, ctx.Channel.Id);
                try {
                    await ReplyWithEmbedAsync(ctx, "Quiz will start in 10s! Get ready!", ":clock1:")
                        .ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromSeconds(10))
                        .ConfigureAwait(false);
                    await quiz.RunAsync()
                        .ConfigureAwait(false);

                    if (quiz.NoReply) {
                        await ReplyWithEmbedAsync(ctx, "Aborting quiz due to no replies...", ":alarm_clock:")
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
                        await DatabaseService.UpdateUserStatsAsync(results.First().Item1.Id, "quizes_won")
                            .ConfigureAwait(false);
                    } else {
                        await ReplyWithEmbedAsync(ctx, "Trying to improve stats by playing alone? Won't work...", ":joy:")
                            .ConfigureAwait(false);
                    }
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }
            #endregion
        }
    }
}
