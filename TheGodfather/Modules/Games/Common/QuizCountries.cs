#region USING_DIRECTIVES
using System;
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
    public class QuizCountries : Game
    {
        private static Dictionary<string, string> _countries = null;
        public IEnumerable<(ulong, int)> Results;
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
            var participants = new Dictionary<ulong, int>();
            
            int timeouts = 0;
            for (int i = 1; i < NumberOfQuestions; i++) {
                string question = questions[GFRandom.Generator.Next(questions.Count)];

                await _channel.TriggerTypingAsync()
                    .ConfigureAwait(false);
                await _channel.SendFileAsync(new FileStream(question, FileMode.Open), "flag.png", content: $"Question #{Formatter.Bold(i.ToString())}:")
                    .ConfigureAwait(false);

                bool noresponse = true;
                Regex ansregex = new Regex($@"\b{_countries[question]}\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                var mctx = await _interactivity.WaitForMessageAsync(
                    xm => {
                        if (xm.ChannelId != _channel.Id || xm.Author.IsBot) return false;
                        noresponse = false;
                        return ansregex.IsMatch(xm.Content);
                    }
                ).ConfigureAwait(false);
                if (mctx == null) {
                    if (noresponse)
                        timeouts++;
                    else
                        timeouts = 0;
                    if (timeouts == 3) {
                        NoReply = true;
                        return;
                    }
                    await _channel.SendMessageAsync($"Time is out! The correct answer was: {Formatter.Bold(_countries[question])}")
                        .ConfigureAwait(false);
                } else {
                    await _channel.SendMessageAsync($"GG {mctx.User.Mention}, you got it right!")
                        .ConfigureAwait(false);
                    if (participants.ContainsKey(mctx.User.Id))
                        participants[mctx.User.Id]++;
                    else
                        participants.Add(mctx.User.Id, 1);
                }
                questions.Remove(question);

                await Task.Delay(TimeSpan.FromSeconds(2))
                    .ConfigureAwait(false);
            }

            Results = participants.OrderByDescending(kvp => kvp.Value).Select(kvp => (kvp.Key, kvp.Value));
        }
    }
}


