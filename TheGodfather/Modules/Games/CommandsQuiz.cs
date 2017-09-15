#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot.Modules.Games
{
    [Group("quiz", CanInvokeWithoutSubcommand = false)]
    [Description("Start a quiz!")]
    [Aliases("trivia")]
    class CommandsQuiz
    {
        #region COMMAND_QUIZ_COUNTRIES
        [Command("countries")]
        [Description("Country flags quiz.")]
        [Aliases("flags")]
        public async Task CountriesQuiz(CommandContext ctx)
        {
            Dictionary<string, string> qna = LoadQuestionsAndAnswers();
            await StartQuiz(ctx, qna);
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task StartQuiz(CommandContext ctx, Dictionary<string, string> qna)
        {
            var questions = new List<string>(qna.Keys);
            var participants = new SortedDictionary<string, int>();

            var rnd = new Random();
            for (int i = 0; i < 5; i++) {
                string question = questions[rnd.Next(questions.Count)];
                await ctx.RespondAsync(question);

                var interactivity = ctx.Client.GetInteractivityModule();
                var msg = await interactivity.WaitForMessageAsync(
                    xm => xm.Content.ToLower() == qna[question],
                    TimeSpan.FromSeconds(10)
                );
                if (msg == null) {
                    await ctx.RespondAsync($"Time is out! The correct answer was: {qna[question]}");
                } else {
                    await ctx.RespondAsync($"GG {msg.User.Mention}, you got it right!");
                    if (participants.ContainsKey(msg.User.Username))
                        participants[msg.User.Username]++;
                    else
                        participants.Add(msg.User.Username, 1);
                }
                questions.Remove(question);
                await Task.Delay(2000);
            }

            var em = new DiscordEmbedBuilder() { Title = "Results" };
            foreach (var participant in participants)
                em.AddField(participant.Key, participant.Value.ToString(), inline: true);
            await ctx.RespondAsync("", embed: em);
        }
        
        private Dictionary<string, string> LoadQuestionsAndAnswers()
        {
            var qna = new Dictionary<string, string>();
            qna.Add("test 1?", "1");
            qna.Add("test 2?", "2");
            qna.Add("test 3?", "3");
            qna.Add("test 4?", "4");
            qna.Add("test 5?", "5");
            return qna;
        }
        #endregion
    }
}
