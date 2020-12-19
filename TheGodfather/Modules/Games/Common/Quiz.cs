#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Common.Collections;
using TheGodfather.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class Quiz : BaseChannelGame
    {
        public ConcurrentDictionary<DiscordUser, int> Results { get; }

        private readonly IReadOnlyList<QuizQuestion> questions;


        public Quiz(InteractivityExtension interactivity, DiscordChannel channel, IReadOnlyList<QuizQuestion> questions)
            : base(interactivity, channel)
        {
            this.questions = questions;
            this.Results = new ConcurrentDictionary<DiscordUser, int>();
        }


        public override async Task RunAsync(LocalizationService lcs)
        {
            int timeouts = 0;
            int currentQuestionIndex = 1;
            foreach (QuizQuestion question in this.questions) {
                var emb = new DiscordEmbedBuilder {
                    Title = $"Question #{currentQuestionIndex}",
                    Description = Formatter.Bold(question.Content),
                    Color = DiscordColor.Teal
                };
                emb.AddField("Category", question.Category, inline: false);

                var answers = new List<string>(question.IncorrectAnswers) {
                    question.CorrectAnswer
                };
                answers.Shuffle();

                for (int index = 0; index < answers.Count; index++)
                    emb.AddField($"Answer #{index + 1}:", answers[index], inline: true);

                await this.Channel.TriggerTypingAsync();
                await this.Channel.SendMessageAsync(embed: emb.Build());

                bool timeout = true;
                var failed = new ConcurrentHashSet<ulong>();
                var answerRegex = new Regex($@"\b{question.CorrectAnswer}\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                InteractivityResult<DiscordMessage> mctx = await this.Interactivity.WaitForMessageAsync(
                    xm => {
                        if (xm.ChannelId != this.Channel.Id || xm.Author.IsBot || failed.Contains(xm.Author.Id))
                            return false;
                        if (int.TryParse(xm.Content, out int index) && index > 0 && index <= answers.Count) {
                            timeout = false;
                            if (answers[index - 1] == question.CorrectAnswer)
                                return true;
                            else
                                failed.Add(xm.Author.Id);
                        }
                        return false;
                    },
                    TimeSpan.FromSeconds(10)
                ); ;
                if (mctx.TimedOut) {
                    if (timeout)
                        timeouts++;
                    else
                        timeouts = 0;

                    if (timeouts == 3) {
                        this.IsTimeoutReached = true;
                        return;
                    }

                    await this.Channel.SendMessageAsync($"Time is out! The correct answer was: {Formatter.Bold(question.CorrectAnswer)}");
                } else {
                    await this.Channel.SendMessageAsync($"GG {mctx.Result.Author.Mention}, you got it right!");
                    this.Results.AddOrUpdate(mctx.Result.Author, u => 1, (u, v) => v + 1);
                }

                await Task.Delay(TimeSpan.FromSeconds(2));

                currentQuestionIndex++;
            }
        }
    }
}


