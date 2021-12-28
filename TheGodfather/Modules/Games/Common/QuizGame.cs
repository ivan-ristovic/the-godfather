using System.Collections.Concurrent;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using TheGodfather.Common.Collections;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Games.Common;

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
            emb.WithLocalizedTitle(TranslationKey.fmt_game_quiz_q(i + 1));
            emb.WithDescription(Formatter.Bold(question.Content));
            emb.WithColor(DiscordColor.Teal);
            emb.AddLocalizedField(TranslationKey.str_category, question.Category);

            var answers = new List<string>(question.IncorrectAnswers) { question.CorrectAnswer }.Shuffle().ToList();

            foreach ((string answer, int index) in answers.Select((a, i) => (a, i)))
                emb.AddLocalizedField(TranslationKey.fmt_game_quiz_a(index + 1), answer, true);

            var options = Emojis.Numbers.All.Skip(1).Take(4).ToList();
            DiscordMessage msg = await this.Channel.SendMessageAsync(emb.Build());
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
                await this.Channel.LocalizedEmbedAsync(lcs, Emojis.AlarmClock, DiscordColor.Teal, TranslationKey.fmt_game_quiz_timeout(question.CorrectAnswer));
            } else {
                await this.Channel.LocalizedEmbedAsync(lcs, Emojis.CheckMarkSuccess, DiscordColor.Teal, TranslationKey.fmt_game_quiz_correct(res.Result.User.Mention));
                this.results.AddOrUpdate(res.Result.User, _ => 1, (_, v) => v + 1);
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}