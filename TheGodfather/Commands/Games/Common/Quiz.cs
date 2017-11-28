#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Helpers.Collections;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Commands.Games
{
    public enum QuizType { Countries };

    public class Quiz
    {
        #region PUBLIC_FIELDS
        public DiscordUser Winner { get; private set; }
        #endregion

        #region STATIC_FIELDS
        public static bool QuizExistsInChannel(ulong cid) => _channels.Contains(cid);
        private static ConcurrentHashSet<ulong> _channels = new ConcurrentHashSet<ulong>();
        private static Dictionary<string, string> _countries = null;
        #endregion

        #region PRIVATE_FIELDS
        private DiscordClient _client;
        private ulong _cid;
        #endregion


        public Quiz(DiscordClient client, ulong cid)
        {
            _channels.Add(_cid);
            _client = client;
            _cid = cid;
        }


        public async Task StartAsync(QuizType t)
        {
            var chn = await _client.GetChannelAsync(_cid)
                .ConfigureAwait(false);

            var questions = new List<string>(_countries.Keys);
            var participants = new SortedDictionary<ulong, int>();

            await chn.SendMessageAsync("Quiz will start in 10s! Get ready!")
                .ConfigureAwait(false);
            await Task.Delay(10000)
                .ConfigureAwait(false);

            var rnd = new Random();
            for (int i = 1; i < 10; i++) {
                string question = questions[rnd.Next(questions.Count)];

                await chn.TriggerTypingAsync()
                    .ConfigureAwait(false);

                if (t == QuizType.Countries) {
                    try {
                        await chn.SendFileAsync(new FileStream(question, FileMode.Open), content: $"Question {Formatter.Bold(i.ToString())}:")
                            .ConfigureAwait(false); ;
                    } catch (IOException e) {
                        throw e;
                    }
                } else {
                    await chn.SendMessageAsync(question)
                        .ConfigureAwait(false);
                }

                var interactivity = _client.GetInteractivityModule();
                var msg = await interactivity.WaitForMessageAsync(
                    // TODO check enum when you add more quiz commands
                    xm => xm.ChannelId == _cid && xm.Content.ToLower() == _countries[question].ToLower()
                ).ConfigureAwait(false);
                if (msg == null) {
                    await chn.SendMessageAsync($"Time is out! The correct answer was: {Formatter.Bold(_countries[question])}")
                        .ConfigureAwait(false);
                } else {
                    await chn.SendMessageAsync($"GG {msg.User.Mention}, you got it right!")
                        .ConfigureAwait(false);
                    if (participants.ContainsKey(msg.User.Id))
                        participants[msg.User.Id]++;
                    else
                        participants.Add(msg.User.Id, 1);
                }
                questions.Remove(question);

                await Task.Delay(2000)
                    .ConfigureAwait(false);
            }

            var em = new DiscordEmbedBuilder() { Title = "Results", Color = DiscordColor.Azure };
            foreach (var participant in participants) {
                var m = await _client.GetUserAsync(participant.Key)
                    .ConfigureAwait(false);
                em.AddField(m.Username, participant.Value.ToString(), inline: true);
            }
            await chn.SendMessageAsync(embed: em.Build())
                .ConfigureAwait(false);

            _channels.TryRemove(_cid);
        }

        public static void LoadCountries()
        {
            if (_countries != null)
                return;

            DirectoryInfo di = new DirectoryInfo("Resources/quiz-flags");
            FileInfo[] files = di.GetFiles("*.png");
            _countries = new Dictionary<string, string>();
            foreach (var f in files)
                _countries.Add(f.FullName, f.Name.Split('.')[0]);
        }
    }
}


