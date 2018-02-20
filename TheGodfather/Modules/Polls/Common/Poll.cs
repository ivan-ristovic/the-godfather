#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Extensions.Collections;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Polls.Common
{
    public class Poll
    {
        #region STATIC_FIELDS
        private static ConcurrentDictionary<ulong, Poll> _polls = new ConcurrentDictionary<ulong, Poll>();

        public static Poll GetPollInChannel(ulong cid)
            => _polls.ContainsKey(cid) ? _polls[cid] : null;

        public static bool RunningInChannel(ulong cid)
            => _polls.ContainsKey(cid) && _polls[cid] != null;

        public static void RegisterPollInChannel(Poll poll, ulong cid)
            => _polls.AddOrUpdate(cid, poll, (c, g) => poll);

        public static void UnregisterPollInChannel(ulong cid)
        {
            if (!_polls.ContainsKey(cid))
                return;
            if (!_polls.TryRemove(cid, out _))
                _polls[cid] = null;
        }
        #endregion

        public string Question { get; }
        public bool Running { get; private set; }
        private List<PollOption> _options = new List<PollOption>();
        public int OptionCount => _options.Count;
        private List<ulong> _voted = new List<ulong>();
        private DiscordChannel _channel;
        private InteractivityExtension _interactivity;


        public Poll(InteractivityExtension interactivity, DiscordChannel channel, string question)
        {
            _channel = channel;
            _interactivity = interactivity;
            Question = question;
        }


        public async Task RunAsync(TimeSpan timespan)
        {
            Running = true;
            await Task.Delay(timespan)
                .ConfigureAwait(false);
            Running = false;
        }

        public void SetOptions(List<string> options)
        {
            foreach (var option in options)
                _options.Add(new PollOption { Option = option, Votes = 0 });
        }

        public bool UserVoted(ulong uid)
            => _voted.Contains(uid);

        public bool IsValidVote(int vote)
            => vote > 0 && vote <= _options.Count;

        public void VoteFor(ulong uid, int vote)
        {
            _voted.Add(uid);
            _options[vote - 1].Votes++;
        }

        public string OptionWithId(int id)
            => _options[id].Option;

        public DiscordEmbed EmbedPoll()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = Question,
                Description = $"Vote by typing {Formatter.InlineCode("!vote <number>")}",
                Color = DiscordColor.Orange
            };
            for (int i = 0; i < _options.Count; i++)
                if (!string.IsNullOrWhiteSpace(_options[i].Option))
                    emb.AddField($"{i + 1}", _options[i].Option, inline: true);

            return emb.Build();
        }

        public DiscordEmbed EmbedPollResults()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = Question + " (results)",
                Color = DiscordColor.Orange
            };
            foreach (var option in _options)
                emb.AddField(option.Option, option.Votes.ToString(), inline: true);

            return emb.Build();
        }


        private sealed class PollOption
        {
            public string Option;
            public int Votes;
        }
    }
}
