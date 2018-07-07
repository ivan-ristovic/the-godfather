#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class QuizCountries : ChannelEvent
    {
        private static Dictionary<string, string> _countries = null;
        public ConcurrentDictionary<DiscordUser, int> Results = new ConcurrentDictionary<DiscordUser, int>();
        public int NumberOfQuestions { get; private set; }

        public static void LoadCountries()
        {
            if (_countries != null)
                return;

            var di = new DirectoryInfo("Resources/quiz-flags");
            var files = di.GetFiles("*.png");
            _countries = new Dictionary<string, string>();
            foreach (var f in files)
                _countries.Add(f.FullName, f.Name.Split('.')[0]);
        }


        public QuizCountries(InteractivityExtension interactivity, DiscordChannel channel, int questions)
            : base(interactivity, channel)
        {
            NumberOfQuestions = questions;
        }


        public override async Task RunAsync()
        {
            var questions = _countries.Keys.ToList();
            
            int timeouts = 0;
            for (int i = 1; i < NumberOfQuestions; i++) {
                string question = questions[GFRandom.Generator.Next(questions.Count)];

                await Channel.TriggerTypingAsync()
                    .ConfigureAwait(false);
                await Channel.SendFileAsync(new FileStream(question, FileMode.Open), "flag.png", content: $"Question #{Formatter.Bold(i.ToString())}:")
                    .ConfigureAwait(false);

                bool noresponse = true;
                Regex ansregex = new Regex($@"\b{_countries[question]}\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                var mctx = await Interactivity.WaitForMessageAsync(
                    xm => {
                        if (xm.ChannelId != Channel.Id || xm.Author.IsBot) return false;
                        noresponse = false;
                        return ansregex.IsMatch(xm.Content);
                    }, TimeSpan.FromSeconds(10)
                ).ConfigureAwait(false);
                if (mctx == null) {
                    if (noresponse)
                        timeouts++;
                    else
                        timeouts = 0;
                    if (timeouts == 3) {
                        IsTimeoutReached = true;
                        return;
                    }
                    await Channel.SendMessageAsync($"Time is out! The correct answer was: {Formatter.Bold(_countries[question])}")
                        .ConfigureAwait(false);
                } else {
                    await Channel.SendMessageAsync($"GG {mctx.User.Mention}, you got it right!")
                        .ConfigureAwait(false);
                    Results.AddOrUpdate(mctx.User, u => 1, (u, v) => v + 1);
                }
                questions.Remove(question);

                await Task.Delay(TimeSpan.FromSeconds(2))
                    .ConfigureAwait(false);
            }
        }
    }
}


