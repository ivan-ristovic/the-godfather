#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common.Collections;
using TheGodfather.Common.Converters;
using TheGodfather.Extensions;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class Quiz : Game
    {
        private static IReadOnlyList<QuizQuestion> _questions = null;
        public IEnumerable<(ulong, int)> Results;
        

        public Quiz(InteractivityExtension interactivity, DiscordChannel channel, IReadOnlyList<QuizQuestion> questions)
            : base(interactivity, channel)
        {
            _questions = questions;
        }


        public override async Task RunAsync()
        {
            var participants = new Dictionary<ulong, int>();

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
                if (question.Type == QuestionType.TrueFalse) {
                    bool? ans = CustomBoolConverter.TryConvert(question.CorrectAnswer);
                    mctx = await _interactivity.WaitForMessageAsync(
                        xm => {
                            if (xm.ChannelId != _channel.Id || xm.Author.IsBot || failed.Contains(xm.Author.Id))
                                return false;
                            bool? b = CustomBoolConverter.TryConvert(xm.Content);
                            if (b.HasValue && b.Value == ans.Value)
                                return true;
                            failed.Add(xm.Author.Id);
                            return false;
                        }
                    ).ConfigureAwait(false);
                } else {
                    Regex ansregex = new Regex($@"\b{question.CorrectAnswer}\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                    mctx = await _interactivity.WaitForMessageAsync(
                        xm => {
                            if (xm.ChannelId != _channel.Id || xm.Author.IsBot || failed.Contains(xm.Author.Id))
                                return false;
                            noresponse = false;
                            if (int.TryParse(xm.Content, out int index) && index > 0 && index <= answers.Count) {
                                if (answers[index - 1] == question.CorrectAnswer)
                                    return true;
                            } else {
                                if (ansregex.IsMatch(xm.Content))
                                    return true;
                            }
                            failed.Add(xm.Author.Id);
                            return false;
                        }
                    ).ConfigureAwait(false);
                }
                if (mctx == null) {
                    if (noresponse)
                        timeouts++;
                    else
                        timeouts = 0;
                    if (timeouts == 3) {
                        NoReply = true;
                        return;
                    }
                    await _channel.SendMessageAsync($"Time is out! The correct answer was: {Formatter.Bold(question.CorrectAnswer)}")
                        .ConfigureAwait(false);
                } else {
                    await _channel.SendMessageAsync($"GG {mctx.User.Mention}, you got it right!")
                        .ConfigureAwait(false);
                    if (participants.ContainsKey(mctx.User.Id))
                        participants[mctx.User.Id]++;
                    else
                        participants.Add(mctx.User.Id, 1);
                }

                await Task.Delay(TimeSpan.FromSeconds(2))
                    .ConfigureAwait(false);
            }

            Results = participants.OrderByDescending(kvp => kvp.Value).Select(kvp => (kvp.Key, kvp.Value));
        }
    }
}


