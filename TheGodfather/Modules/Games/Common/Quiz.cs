#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common.Collections;
using TheGodfather.Extensions;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class Quiz : ChannelEvent
    {
        private static IReadOnlyList<QuizQuestion> _questions = null;
        public ConcurrentDictionary<DiscordUser, int> Results = new ConcurrentDictionary<DiscordUser, int>();


        public Quiz(InteractivityExtension interactivity, DiscordChannel channel, IReadOnlyList<QuizQuestion> questions)
            : base(interactivity, channel)
        {
            _questions = questions;
        }


        public override async Task RunAsync()
        {
            int timeouts = 0;
            int i = 0;
            foreach (var question in _questions) {
                i++;
                var emb = new DiscordEmbedBuilder {
                    Title = $"Question #{i}",
                    Description = Formatter.Bold(question.Content),
                    Color = DiscordColor.SapGreen
                };
                emb.AddField("Category", question.Category, inline: false);

                var answers = new List<string>(question.IncorrectAnswers) {
                    question.CorrectAnswer
                };
                answers.Shuffle();

                for (int index = 0; index < answers.Count; index++)
                    emb.AddField($"Answer #{index + 1}:", answers[index], inline: true);

                await _channel.TriggerTypingAsync()
                    .ConfigureAwait(false);
                await _channel.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);

                bool noresponse = true;
                MessageContext mctx;
                ConcurrentHashSet<ulong> failed = new ConcurrentHashSet<ulong>();
                Regex ansregex = new Regex($@"\b{question.CorrectAnswer}\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                mctx = await _interactivity.WaitForMessageAsync(
                    xm => {
                        if (xm.ChannelId != _channel.Id || xm.Author.IsBot || failed.Contains(xm.Author.Id))
                            return false;
                        if (int.TryParse(xm.Content, out int index) && index > 0 && index <= answers.Count) {
                            noresponse = false;
                            if (answers[index - 1] == question.CorrectAnswer)
                                return true;
                            else
                                failed.Add(xm.Author.Id);
                        }
                        return false;
                    }, TimeSpan.FromSeconds(10)
                ).ConfigureAwait(false);
                if (mctx == null) {
                    if (noresponse)
                        timeouts++;
                    else
                        timeouts = 0;
                    if (timeouts == 3) {
                        TimedOut = true;
                        return;
                    }
                    await _channel.SendMessageAsync($"Time is out! The correct answer was: {Formatter.Bold(question.CorrectAnswer)}")
                        .ConfigureAwait(false);
                } else {
                    await _channel.SendMessageAsync($"GG {mctx.User.Mention}, you got it right!")
                        .ConfigureAwait(false);
                    Results.AddOrUpdate(mctx.User, u => 1, (u, v) => v + 1);
                }

                await Task.Delay(TimeSpan.FromSeconds(2))
                    .ConfigureAwait(false);
            }
        }
    }
}


