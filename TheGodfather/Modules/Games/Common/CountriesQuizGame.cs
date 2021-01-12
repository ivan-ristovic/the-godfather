using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Serilog;
using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Extensions;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Games.Common
{
    public sealed class CountriesQuizGame : BaseChannelGame, IQuiz
    {
        private static readonly Dictionary<string, string>? _countries;

        static CountriesQuizGame()
        {
            try {
                var di = new DirectoryInfo("Resources/quiz-flags");
                _countries = di
                    .GetFiles("*.png")
                    .ToDictionary(fi => fi.FullName, fi => Path.GetFileNameWithoutExtension(fi.FullName))
                    ;
            } catch (IOException e) {
                Log.Error(e, "Failed to scan quiz flags directory!");
            }
        }


        public IReadOnlyDictionary<DiscordUser, int> Results => this.results;
        public int NumberOfQuestions { get; }

        private readonly ConcurrentDictionary<DiscordUser, int> results;


        public CountriesQuizGame(InteractivityExtension interactivity, DiscordChannel channel, int questions)
            : base(interactivity, channel)
        {
            if (_countries is null)
                throw new InvalidOperationException("The quiz flag paths have not been loaded.");
            this.NumberOfQuestions = questions;
            this.results = new ConcurrentDictionary<DiscordUser, int>();
        }


        public override async Task RunAsync(LocalizationService lcs)
        {
            if (_countries is null)
                throw new InvalidOperationException("The quiz flag paths have not been loaded.");

            var questions = new Queue<string>(_countries.Keys.Shuffle().Take(this.NumberOfQuestions));

            int timeouts = 0;
            for (int i = 0; i < this.NumberOfQuestions; i++) {
                string question = questions.Dequeue();

                var emb = new LocalizedEmbedBuilder(lcs, this.Channel.GuildId);
                emb.WithLocalizedDescription("fmt-game-quiz-q", i + 1);
                await this.Channel.SendFileAsync("flag.png", new FileStream(question, FileMode.Open), embed: emb.Build());

                bool timeout = true;
                var failed = new ConcurrentHashSet<ulong>();
                var answerRegex = new Regex($@"\b{_countries[question]}\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                InteractivityResult<DiscordMessage> res = await this.Interactivity.WaitForMessageAsync(
                    xm => {
                        if (xm.ChannelId != this.Channel.Id || xm.Author.IsBot || failed.Contains(xm.Author.Id))
                            return false;
                        timeout = false;
                        if (answerRegex.IsMatch(xm.Content))
                            return true;
                        else
                            failed.Add(xm.Author.Id);
                        return false;
                    },
                    TimeSpan.FromSeconds(10)
                );

                if (res.TimedOut) {
                    if (!failed.Any()) {
                        timeouts = timeout ? timeouts + 1 : 0;
                        if (timeouts == 3) {
                            this.IsTimeoutReached = true;
                            return;
                        }
                    } else {
                        timeouts = 0;
                    }
                    await this.Channel.LocalizedEmbedAsync(lcs, Emojis.AlarmClock, DiscordColor.Teal, "fmt-game-quiz-timeout", _countries[question]);
                } else {
                    await this.Channel.LocalizedEmbedAsync(lcs, Emojis.CheckMarkSuccess, DiscordColor.Teal, "fmt-game-quiz-correct", res.Result.Author.Mention);
                    this.results.AddOrUpdate(res.Result.Author, u => 1, (u, v) => v + 1);
                }

                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}
