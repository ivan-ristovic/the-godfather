#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Modules.Games.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games
{
    public class QuizCountries : Game
    {
        private static Dictionary<string, string> _countries = null;
        public IEnumerable<(ulong, int)> Results;

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


        public QuizCountries(InteractivityExtension interactivity, DiscordChannel channel)
            : base(interactivity, channel) { }


        public override async Task RunAsync()
        {
            var questions = _countries.Keys.ToList();
            var participants = new Dictionary<ulong, int>();

            var rnd = new Random();
            int timeouts = 0;
            for (int i = 1; i < 10; i++) {
                string question = questions[rnd.Next(questions.Count)];
                string answer = _countries[question].ToLowerInvariant();

                await _channel.TriggerTypingAsync()
                    .ConfigureAwait(false);
                await _channel.SendFileAsync(new FileStream(question, FileMode.Open), "flag.png", content: $"Question #{Formatter.Bold(i.ToString())}:")
                    .ConfigureAwait(false);

                bool norep = true;
                var msg = await _interactivity.WaitForMessageAsync(
                    xm => {
                        if (xm.ChannelId != _channel.Id) return false;
                        norep = false;
                        return !xm.Author.IsBot && xm.Content.ToLowerInvariant() == answer;
                    }
                ).ConfigureAwait(false);
                if (msg == null) {
                    if (norep)
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
                    await _channel.SendMessageAsync($"GG {msg.User.Mention}, you got it right!")
                        .ConfigureAwait(false);
                    if (participants.ContainsKey(msg.User.Id))
                        participants[msg.User.Id]++;
                    else
                        participants.Add(msg.User.Id, 1);
                }
                questions.Remove(question);

                await Task.Delay(TimeSpan.FromSeconds(2))
                    .ConfigureAwait(false);
            }

            Results = participants.OrderByDescending(kvp => kvp.Value).Select(kvp => (kvp.Key, kvp.Value));
        }
    }
}


