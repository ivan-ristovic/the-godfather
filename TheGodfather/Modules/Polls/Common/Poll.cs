#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.Entities;
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

        public static bool RegisterPollInChannel(Poll poll, ulong cid)
        {
            if (_polls.ContainsKey(cid)) {
                _polls[cid] = poll;
                return true;
            }
            
            return _polls.TryAdd(cid, poll);
        }

        public static void UnregisterPollInChannel(ulong cid)
        {
            if (!_polls.ContainsKey(cid))
                return;
            if (!_polls.TryRemove(cid, out _))
                _polls[cid] = null;
        }
        #endregion

        public string Question { get; }
        public bool Running { get; protected set; }
        public TimeSpan UntilEnd => _endTime != null ? _endTime - DateTime.Now : TimeSpan.Zero;
        protected List<string> _options = new List<string>();
        public int OptionCount => _options.Count;
        protected ConcurrentDictionary<ulong, int> _votes = new ConcurrentDictionary<ulong, int>();
        protected DiscordChannel _channel;
        protected InteractivityExtension _interactivity;
        protected DateTime _endTime;
        protected CancellationTokenSource _cts = new CancellationTokenSource();


        public Poll(InteractivityExtension interactivity, DiscordChannel channel, string question)
        {
            _channel = channel;
            _interactivity = interactivity;
            Question = question;
        }


        public virtual async Task RunAsync(TimeSpan timespan)
        {
            Running = true;
            var msg = await _channel.SendMessageAsync(embed: EmbedPoll())
                .ConfigureAwait(false);
            
            _endTime = DateTime.Now + timespan;
            while (!_cts.IsCancellationRequested) {
                try {
                    if (_channel.LastMessageId != msg.Id) {
                        await msg.DeleteAsync()
                            .ConfigureAwait(false);
                        msg = await _channel.SendMessageAsync(embed: EmbedPoll())
                            .ConfigureAwait(false);
                    } else {
                        await msg.ModifyAsync(embed: EmbedPoll())
                            .ConfigureAwait(false);
                    }
                } catch {
                    msg = await _channel.SendMessageAsync(embed: EmbedPoll())
                        .ConfigureAwait(false);
                }

                if (UntilEnd.TotalSeconds < 1)
                    break;

                try {
                    await Task.Delay(UntilEnd <= TimeSpan.FromSeconds(5) ? UntilEnd : TimeSpan.FromSeconds(5), _cts.Token)
                        .ConfigureAwait(false);
                } catch (TaskCanceledException) {
                    await _channel.SendFailedEmbedAsync("The poll has been cancelled!")
                        .ConfigureAwait(false);
                }
            }
            Running = false;

            await _channel.SendMessageAsync(embed: EmbedPollResults())
                .ConfigureAwait(false);
        }

        public void SetOptions(List<string> options)
        {
            _options = options;
        }

        public bool UserVoted(ulong uid)
            => _votes.ContainsKey(uid);

        public bool IsValidVote(int vote)
            => vote >= 0 && vote < _options.Count;

        public bool VoteFor(ulong uid, int vote)
        {
            if (_votes.ContainsKey(uid))
                return false;
            return _votes.TryAdd(uid, vote);
        }

        public bool CancelVote(ulong uid)
        {
            if (!_votes.ContainsKey(uid))
                return true;
            return _votes.TryRemove(uid, out _);
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        public string OptionWithId(int id)
            => (id >= 0 && id < _options.Count) ? _options[id] : null;

        public virtual DiscordEmbed EmbedPoll()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = Question,
                Description = $"Vote by typing {Formatter.InlineCode("!vote <number>")}",
                Color = DiscordColor.Orange
            };
            for (int i = 0; i < _options.Count; i++)
                if (!string.IsNullOrWhiteSpace(_options[i]))
                    emb.AddField($"{i + 1} : {_options[i]}", $"{_votes.Count(kvp => kvp.Value == i)} vote(s)");

            if (_endTime != null) {
                if (UntilEnd.TotalSeconds > 1)
                    emb.WithFooter($"Poll ends in: {UntilEnd:hh\\:mm\\:ss}");
                else
                    emb.WithFooter($"Poll ended.");
            }

            return emb.Build();
        }

        public virtual DiscordEmbed EmbedPollResults()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = Question + " (results)",
                Color = DiscordColor.Orange
            };
            for (int i = 0; i < _options.Count; i++)
                emb.AddField(_options[i], _votes.Count(kvp => kvp.Value == i).ToString(), inline: true);

            return emb.Build();
        }
    }
}
