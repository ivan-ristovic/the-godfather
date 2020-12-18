#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Common;
using TheGodfather.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class CountriesQuiz : BaseChannelGame
    {
        private static readonly Dictionary<string, string> _countries;

        static CountriesQuiz()
        {
            var di = new DirectoryInfo("Resources/quiz-flags");
            FileInfo[] files = di.GetFiles("*.png");
            _countries = new Dictionary<string, string>();
            foreach (FileInfo fi in files)
                _countries.Add(fi.FullName, fi.Name.Split('.').First());
        }


        public ConcurrentDictionary<DiscordUser, int> Results { get; }
        public int NumberOfQuestions { get; private set; }


        public CountriesQuiz(InteractivityExtension interactivity, DiscordChannel channel, int questions)
            : base(interactivity, channel)
        {
            this.NumberOfQuestions = questions;
            this.Results = new ConcurrentDictionary<DiscordUser, int>();
        }


        public override async Task RunAsync(LocalizationService lcs)
        {
            var questions = new Queue<string>(_countries.Keys.Shuffle());

            int timeouts = 0;
            for (int i = 1; i < this.NumberOfQuestions; i++) {
                string question = questions.Dequeue();

                await this.Channel.TriggerTypingAsync();
                await this.Channel.SendFileAsync("flag.png", new FileStream(question, FileMode.Open), content: $"Question #{Formatter.Bold(i.ToString())}:");

                bool timeout = true;
                var answerRegex = new Regex($@"\b{_countries[question]}\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                InteractivityResult<DiscordMessage> mctx = await this.Interactivity.WaitForMessageAsync(
                    xm => {
                        if (xm.ChannelId != this.Channel.Id || xm.Author.IsBot) return false;
                        timeout = false;
                        return answerRegex.IsMatch(xm.Content);
                    },
                    TimeSpan.FromSeconds(10)
                );
                if (mctx.TimedOut) {
                    if (timeout)
                        timeouts++;
                    else
                        timeouts = 0;

                    if (timeouts == 3) {
                        this.IsTimeoutReached = true;
                        return;
                    }

                    await this.Channel.SendMessageAsync($"Time is out! The correct answer was: {Formatter.Bold(_countries[question])}");
                } else {
                    await this.Channel.SendMessageAsync($"GG {mctx.Result.Author.Mention}, you got it right!");
                    this.Results.AddOrUpdate(mctx.Result.Author, u => 1, (u, v) => v + 1);
                }

                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}


