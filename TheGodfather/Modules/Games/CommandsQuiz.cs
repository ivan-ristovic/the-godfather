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
        private enum QuizType { Countries };


        #region COMMAND_QUIZ_COUNTRIES
        [Command("countries")]
        [Description("Country flags quiz.")]
        [Aliases("flags")]
        public async Task CountriesQuiz(CommandContext ctx)
        {
            Dictionary<string, string> qna = LoadCountries();
            await StartQuiz(QuizType.Countries, ctx, qna);
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task StartQuiz(QuizType t, CommandContext ctx, Dictionary<string, string> qna)
        {
            var questions = new List<string>(qna.Keys);
            var participants = new SortedDictionary<string, int>();

            var rnd = new Random();
            for (int i = 0; i < 5; i++) {
                string question = questions[rnd.Next(questions.Count)];

                if (t == QuizType.Countries)
                    try {
                        await ctx.RespondWithFileAsync(new FileStream($"Resources/quiz-flags/{question}.png", FileMode.Open));
                    } catch (IOException e) {
                        throw e;
                    }
                else
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
        
        private Dictionary<string, string> LoadCountries()
        {
            var qna = new Dictionary<string, string>();
            #region COUNTRIES
            qna.Add("ad", "ad");
            qna.Add("ae", "ae");
            qna.Add("af", "af");
            qna.Add("ag", "ag");
            qna.Add("al", "al");
            qna.Add("am", "am");
            /*qna.Add("ao",
            qna.Add("ar",
            qna.Add("at",
            qna.Add("au",
            qna.Add("az",
            qna.Add("ba",
            qna.Add("bb",
            qna.Add("bd",
            qna.Add("be",
            qna.Add("bf",
            qna.Add("bg",
            qna.Add("bh",
            qna.Add("bi",
            qna.Add("bj",
            qna.Add("bn",
            qna.Add("bo",
            qna.Add("br",
            qna.Add("bs",
            qna.Add("bt",
            qna.Add("bw",
            qna.Add("by",
            qna.Add("bz",
            qna.Add("ca",
            qna.Add("cd",
            qna.Add("cf",
            qna.Add("cg",
            qna.Add("ch",
            qna.Add("ci",
            qna.Add("cl",
            qna.Add("cm",
            qna.Add("cn",
            qna.Add("co",
            qna.Add("cr",
            qna.Add("cu",
            qna.Add("cv",
            qna.Add("cy",
            qna.Add("cz",
            qna.Add("de",
            qna.Add("dj",
            qna.Add("dk",
            qna.Add("dm",
            qna.Add("do",
            qna.Add("dz",
            qna.Add("ec",
            qna.Add("ee",
            qna.Add("eg",
            qna.Add("eh",
            qna.Add("er",
            qna.Add("es",
            qna.Add("et",
            qna.Add("fi",
            qna.Add("fj",
            qna.Add("fm",
            qna.Add("fr",
            qna.Add("ga",
            qna.Add("gb",
            qna.Add("gd",
            qna.Add("ge",
            qna.Add("gh",
            qna.Add("gm",
            qna.Add("gn",
            qna.Add("gq",
            qna.Add("gr",
            qna.Add("gt",
            qna.Add("gw",
            qna.Add("gy",
            qna.Add("hn",
            qna.Add("hr",
            qna.Add("ht",
            qna.Add("hu",
            qna.Add("id",
            qna.Add("ie",
            qna.Add("il",
            qna.Add("in",
            qna.Add("iq",
            qna.Add("ir",
            qna.Add("is",
            qna.Add("it",
            qna.Add("jm",
            qna.Add("jo",
            qna.Add("jp",
            qna.Add("ke",
            qna.Add("kg",
            qna.Add("kh",
            qna.Add("ki",
            qna.Add("km",
            qna.Add("kn",
            qna.Add("kp",
            qna.Add("kr",
            qna.Add("ks",
            qna.Add("kw",
            qna.Add("kz",
            qna.Add("la",
            qna.Add("lb",
            qna.Add("lc",
            qna.Add("li",
            qna.Add("lk",
            qna.Add("lr",
            qna.Add("ls",
            qna.Add("lt",
            qna.Add("lu",
            qna.Add("lv",
            qna.Add("ly",
            qna.Add("ma",
            qna.Add("mc",
            qna.Add("md",
            qna.Add("me",
            qna.Add("mg",
            qna.Add("mh",
            qna.Add("mk",
            qna.Add("ml",
            qna.Add("mm",
            qna.Add("mn",
            qna.Add("mr",
            qna.Add("mt",
            qna.Add("mu",
            qna.Add("mv",
            qna.Add("mw",
            qna.Add("mx",
            qna.Add("my",
            qna.Add("mz",
            qna.Add("na",
            qna.Add("ne",
            qna.Add("ng",
            qna.Add("ni",
            qna.Add("nl",
            qna.Add("no",
            qna.Add("np",
            qna.Add("nr",
            qna.Add("nz",
            qna.Add("om",
            qna.Add("pa",
            qna.Add("pe",
            qna.Add("pg",
            qna.Add("ph",
            qna.Add("pk",
            qna.Add("pl",
            qna.Add("pt",
            qna.Add("pw",
            qna.Add("py",
            qna.Add("qa",
            qna.Add("ro",
            qna.Add("rs",
            qna.Add("ru",
            qna.Add("rw",
            qna.Add("sa",
            qna.Add("sb",
            qna.Add("sc",
            qna.Add("sd",
            qna.Add("se",
            qna.Add("sg",
            qna.Add("si",
            qna.Add("sk",
            qna.Add("sl",
            qna.Add("sm",
            qna.Add("sn",
            qna.Add("so",
            qna.Add("sr",
            qna.Add("st",
            qna.Add("sv",
            qna.Add("sy",
            qna.Add("sz",
            qna.Add("td",
            qna.Add("tg",
            qna.Add("th",
            qna.Add("tj",
            qna.Add("tl",
            qna.Add("tm",
            qna.Add("tn",
            qna.Add("to",
            qna.Add("tr",
            qna.Add("tt",
            qna.Add("tv",
            qna.Add("tw",
            qna.Add("tz",
            qna.Add("ua",
            qna.Add("ug",
            qna.Add("us",
            qna.Add("uy",
            qna.Add("uz",
            qna.Add("va",
            qna.Add("vc",
            qna.Add("ve",
            qna.Add("vn",
            qna.Add("vu",
            qna.Add("ws",
            qna.Add("ye",
            qna.Add("za",
            qna.Add("zm",
            qna.Add("zw",*/
            #endregion
            return qna;
        }
        #endregion
    }
}
