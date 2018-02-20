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
        private List<PollOption> _options = new List<PollOption>();
        private List<ulong> _voted = new List<ulong>();
        private bool _listening = false;
        private object _lock = new object();
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
            var mctx = await _interactivity.WaitForMessageAsync(
                m => HandlePosiblePollReply(m).GetAwaiter().GetResult()
                , timespan
            ).ConfigureAwait(false);
        }

        public void SetOptions(List<string> options)
        {
            foreach (var option in options)
                _options.Add(new PollOption { Option = option, Votes = 0 });
        }

        public DiscordEmbed EmbedPoll()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = Question,
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

        private async Task<bool> HandlePosiblePollReply(DiscordMessage m)
        {
            if (m.Author.IsBot || m.Channel.Id != _channel.Id)
                return false;

            if (!int.TryParse(m.Content, out int vote))
                return false;

            if (vote > 0 && vote <= _options.Count) {
                if (_voted.Contains(m.Author.Id)) {
                    await _channel.SendMessageAsync("You have already voted!")
                        .ConfigureAwait(false);
                    return false;
                }

                _voted.Add(m.Author.Id);
                _options[vote - 1].Votes++;
                await _channel.SendMessageAsync($"{m.Author.Mention} voted for: **{vote}**!")
                    .ConfigureAwait(false);
            } else {
                await _channel.SendMessageAsync($"Invalid poll option. Valid range: [1, {_options.Count}].")
                    .ConfigureAwait(false);
            }

            return false;
        }


        private sealed class PollOption
        {
            public string Option;
            public int Votes;
        }
    }
}
