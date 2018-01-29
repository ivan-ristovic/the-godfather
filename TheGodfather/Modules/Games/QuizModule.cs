#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Attributes;
using TheGodfather.Services;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("quiz")]
        [Description("Start a quiz!")]
        [Aliases("trivia", "q")]
        [Cooldown(2, 5, CooldownBucketType.User), Cooldown(3, 5, CooldownBucketType.Channel)]
        [PreExecutionCheck]
        public class QuizModule
        {
            #region COMMAND_QUIZ_COUNTRIES
            [Command("countries")]
            [Description("Country flags quiz.")]
            [Aliases("flags")]
            public async Task CountriesQuizAsync(CommandContext ctx)
            {
                Quiz.LoadCountries();

                if (Quiz.QuizExistsInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Quiz is already running in this channel!");

                var quiz = new Quiz(ctx.Client, ctx.Channel.Id);
                await quiz.StartAsync(QuizType.Countries)
                    .ConfigureAwait(false);

                if (quiz.Winner != null)
                    await ctx.Services.GetService<DatabaseService>().UpdateUserStatsAsync(quiz.Winner.Id, "quizes_won")
                        .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
