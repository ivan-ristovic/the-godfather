#region USING_DIRECTIVES
using System;
using System.IO;
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
        #region PRIVATE_FIELDS
        private enum QuizType { Countries };
        private Dictionary<string, string> _countries = null;
        #endregion


        #region COMMAND_QUIZ_COUNTRIES
        [Command("countries")]
        [Description("Country flags quiz.")]
        [Aliases("flags")]
        public async Task CountriesQuiz(CommandContext ctx)
        {
            if (_countries == null)
                LoadCountries();
            await StartQuiz(QuizType.Countries, ctx);
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task StartQuiz(QuizType t, CommandContext ctx)
        {
            var questions = new List<string>(_countries.Keys);
            var participants = new SortedDictionary<string, int>();

            var rnd = new Random();
            for (int i = 0; i < 5; i++) {
                string question = questions[rnd.Next(questions.Count)];

                if (t == QuizType.Countries)
                    try {
                        await ctx.RespondWithFileAsync(new FileStream(question, FileMode.Open));
                    } catch (IOException e) {
                        throw e;
                    }
                else
                    await ctx.RespondAsync(question);

                var interactivity = ctx.Client.GetInteractivityModule();
                var msg = await interactivity.WaitForMessageAsync(
                    xm => xm.Content.ToLower() == _countries[question].ToLower(),
                    TimeSpan.FromSeconds(20)
                );
                if (msg == null) {
                    await ctx.RespondAsync($"Time is out! The correct answer was: **{_countries[question]}**");
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
        
        private void LoadCountries()
        {
            DirectoryInfo di = new DirectoryInfo("Resources/quiz-flags");
            FileInfo[] files = di.GetFiles("*.png");
            _countries = new Dictionary<string, string>();
            foreach (var f in files)
                _countries.Add(f.FullName, f.Name.Split('.')[0]);
        }
        #endregion
    }
}
