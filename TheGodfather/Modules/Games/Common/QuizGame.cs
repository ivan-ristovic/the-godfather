using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Extensions;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Games.Common
{
    public sealed class QuizGame : BaseChannelGame, IQuiz
    {
        public int NumberOfQuestions { get; }
        public IReadOnlyDictionary<DiscordUser, int> Results => this.results;

        private readonly ConcurrentDictionary<DiscordUser, int> results;
        private readonly IReadOnlyList<QuizQuestion> questions;


        public QuizGame(InteractivityExtension interactivity, DiscordChannel channel, IReadOnlyList<QuizQuestion> questions)
            : base(interactivity, channel)
        {
            this.questions = questions;
            this.results = new ConcurrentDictionary<DiscordUser, int>();
            this.NumberOfQuestions = this.questions.Count;
        }


        public override async Task RunAsync(LocalizationService lcs)
        {
            int timeouts = 0;

            foreach ((QuizQuestion question, int i) in this.questions.Select((q, i) => (q, i))) {
                var emb = new LocalizedEmbedBuilder(lcs, this.Channel.GuildId);
                emb.WithLocalizedTitle("fmt-game-quiz-q", i + 1);
                emb.WithDescription(Formatter.Bold(question.Content));
                emb.WithColor(DiscordColor.Teal);
                emb.AddLocalizedField("str-category", question.Category, inline: false);

                var answers = new List<string>(question.IncorrectAnswers) { question.CorrectAnswer }.Shuffle().ToList();

                foreach ((string answer, int index) in answers.Select((a, i) => (a, i)))
                    emb.AddLocalizedField("fmt-game-quiz-a", answer, inline: true, titleArgs: index + 1);

                var options = Emojis.Numbers.All.Skip(1).Take(4).ToList();
                DiscordMessage msg = await this.Channel.SendMessageAsync(embed: emb.Build());
                foreach (DiscordEmoji emoji in options)
                    await msg.CreateReactionAsync(emoji);

                bool timeout = true;
                var failed = new ConcurrentHashSet<ulong>();
                InteractivityResult<MessageReactionAddEventArgs> res = await this.Interactivity.WaitForReactionAsync(
                    e => {
                        if (e.User.IsBot || failed.Contains(e.User.Id) || e.Message != msg)
                            return false;
                        int opt = options.IndexOf(e.Emoji);
                        if (opt == -1)
                            return false;
                        if (answers[opt].Equals(question.CorrectAnswer))
                            return true;
                        else
                            failed.Add(e.User.Id);
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
                    await this.Channel.LocalizedEmbedAsync(lcs, Emojis.AlarmClock, DiscordColor.Teal, "fmt-game-quiz-timeout", question.CorrectAnswer);
                } else {
                    await this.Channel.LocalizedEmbedAsync(lcs, Emojis.CheckMarkSuccess, DiscordColor.Teal, "fmt-game-quiz-correct", res.Result.User.Mention);
                    this.results.AddOrUpdate(res.Result.User, u => 1, (u, v) => v + 1);
                }

                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}
