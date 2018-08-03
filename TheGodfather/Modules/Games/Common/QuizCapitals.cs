#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Common;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class QuizCapitals : ChannelEvent
    {
        private static Dictionary<string, string> _capitals = null;
        public ConcurrentDictionary<DiscordUser, int> Results = new ConcurrentDictionary<DiscordUser, int>();
        public int NumberOfQuestions { get; private set; }

        public static void LoadCapitals()
        {
            if (_capitals != null)
                return;

            var data = File.ReadAllText("Resources/quiz-capitals.json");
            var list = JsonConvert.DeserializeObject<List<CountryAndCapital>>(data);
            _capitals = new Dictionary<string, string>();
            foreach (var cc in list)
                _capitals.Add(cc.Country, cc.Capital);
        }


        public QuizCapitals(InteractivityExtension interactivity, DiscordChannel channel, int questions)
            : base(interactivity, channel)
        {
            NumberOfQuestions = questions;
        }


        public override async Task RunAsync()
        {
            var questions = _capitals.Keys.ToList();

            int timeouts = 0;
            for (int i = 1; i < NumberOfQuestions; i++) {
                string question = questions[GFRandom.Generator.Next(questions.Count)];

                await Channel.TriggerTypingAsync()
                    .ConfigureAwait(false);
                await Channel.EmbedAsync($"The capital of {Formatter.Bold(question)} is?", StaticDiscordEmoji.Question)
                    .ConfigureAwait(false);

                bool noresponse = true;
                Regex ansregex = new Regex($@"\b{_capitals[question]}\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
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
                    await Channel.SendMessageAsync($"Time is out! The correct answer was: {Formatter.Bold(_capitals[question])}")
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

    public class CountryAndCapital
    {
        public string Country { get; set; }
        public string Capital { get; set; }
    }
}


