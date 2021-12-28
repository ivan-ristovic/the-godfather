using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Common;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;
using Serilog;
using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Games.Common
{
    public sealed class CapitalsQuizGame : BaseChannelGame, IQuiz
    {
        private static readonly Dictionary<string, string>? _capitals;

        static CapitalsQuizGame()
        {
            try {
                string data = File.ReadAllText("Resources/quiz-capitals.json");
                _capitals = JsonConvert.DeserializeObject<List<CapitalInfo>>(data)
                    ?.ToDictionary(ci => ci.Country, ci => ci.Capital);
            } catch (Exception e) {
                Log.Error(e, "Failed to load capitals data from Resources folder");
            }
        }


        public IReadOnlyDictionary<DiscordUser, int> Results => this.results;
        public int NumberOfQuestions { get; }

        private readonly ConcurrentDictionary<DiscordUser, int> results;


        public CapitalsQuizGame(InteractivityExtension interactivity, DiscordChannel channel, int questions)
            : base(interactivity, channel)
        {
            if (_capitals is null)
                throw new InvalidOperationException("The capitals data has not been loaded.");
            this.NumberOfQuestions = questions;
            this.results = new ConcurrentDictionary<DiscordUser, int>();
        }


        public override async Task RunAsync(LocalizationService lcs)
        {
            if (_capitals is null)
                throw new InvalidOperationException("The capitals data has not been loaded.");

            var questions = new Queue<string>(_capitals.Keys.Shuffle().Take(this.NumberOfQuestions));

            int timeouts = 0;
            for (int i = 0; i < this.NumberOfQuestions; i++) {
                string question = questions.Dequeue();
                await this.Channel.LocalizedEmbedAsync(lcs, Emojis.Question, DiscordColor.Teal, TranslationKey.fmt_game_quiz_qc(question));

                bool timeout = true;
                var failed = new ConcurrentHashSet<ulong>();
                var answerRegex = new Regex($@"\b{_capitals[question]}\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
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
                    await this.Channel.LocalizedEmbedAsync(lcs, Emojis.AlarmClock, DiscordColor.Teal, TranslationKey.fmt_game_quiz_timeout(_capitals[question]));
                } else {
                    await this.Channel.LocalizedEmbedAsync(lcs, Emojis.CheckMarkSuccess, DiscordColor.Teal, TranslationKey.fmt_game_quiz_correct(res.Result.Author.Mention));
                    this.results.AddOrUpdate(res.Result.Author, u => 1, (u, v) => v + 1);
                }

                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}
