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

        #region PUBLIC_FIELDS
        public string Question { get; }
        public bool Running { get; protected set; }
        public TimeSpan UntilEnd => this._endTime != null ? this._endTime - DateTime.Now : TimeSpan.Zero;
        public int OptionCount => this._options.Count;
        #endregion

        #region PROTECTED_FIELDS
        protected List<string> _options = new List<string>();
        protected ConcurrentDictionary<ulong, int> _votes = new ConcurrentDictionary<ulong, int>();
        protected DiscordChannel _channel;
        protected InteractivityExtension _interactivity;
        protected DateTime _endTime;
        protected CancellationTokenSource _cts = new CancellationTokenSource();
        #endregion


        public Poll(InteractivityExtension interactivity, DiscordChannel channel, string question)
        {
            this._channel = channel;
            this._interactivity = interactivity;
            this.Question = question;
        }


        public virtual async Task RunAsync(TimeSpan timespan)
        {
            this.Running = true;
            var msg = await this._channel.SendMessageAsync(embed: EmbedPoll())
                .ConfigureAwait(false);

            this._endTime = DateTime.Now + timespan;
            while (!this._cts.IsCancellationRequested) {
                try {
                    if (this._channel.LastMessageId != msg.Id) {
                        await msg.DeleteAsync()
                            .ConfigureAwait(false);
                        msg = await this._channel.SendMessageAsync(embed: EmbedPoll())
                            .ConfigureAwait(false);
                    } else {
                        await msg.ModifyAsync(embed: EmbedPoll())
                            .ConfigureAwait(false);
                    }
                } catch {
                    msg = await this._channel.SendMessageAsync(embed: EmbedPoll())
                        .ConfigureAwait(false);
                }

                if (this.UntilEnd.TotalSeconds < 1)
                    break;

                try {
                    await Task.Delay(this.UntilEnd <= TimeSpan.FromSeconds(5) ? this.UntilEnd : TimeSpan.FromSeconds(5), this._cts.Token)
                        .ConfigureAwait(false);
                } catch (TaskCanceledException) {
                    await this._channel.InformFailureAsync("The poll has been cancelled!")
                        .ConfigureAwait(false);
                }
            }
            this.Running = false;

            await this._channel.SendMessageAsync(embed: EmbedPollResults())
                .ConfigureAwait(false);
        }

        public virtual DiscordEmbed EmbedPoll()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = Question,
                Description = $"Vote by typing {Formatter.InlineCode("!vote <number>")}",
                Color = DiscordColor.Orange
            };
            for (int i = 0; i < this._options.Count; i++)
                if (!string.IsNullOrWhiteSpace(this._options[i]))
                    emb.AddField($"{i + 1} : {this._options[i]}", $"{this._votes.Count(kvp => kvp.Value == i)} vote(s)");

            if (this._endTime != null) {
                if (this.UntilEnd.TotalSeconds > 1)
                    emb.WithFooter($"Poll ends in: {this.UntilEnd:hh\\:mm\\:ss}");
                else
                    emb.WithFooter($"Poll ended.");
            }

            return emb.Build();
        }

        public virtual DiscordEmbed EmbedPollResults()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = this.Question + " (results)",
                Color = DiscordColor.Orange
            };
            for (int i = 0; i < this._options.Count; i++)
                emb.AddField(this._options[i], this._votes.Count(kvp => kvp.Value == i).ToString(), inline: true);

            return emb.Build();
        }

        public bool CancelVote(ulong uid)
        {
            if (!this._votes.ContainsKey(uid))
                return true;
            return this._votes.TryRemove(uid, out _);
        }

        public bool IsValidVote(int vote)
            => vote >= 0 && vote < this._options.Count;

        public string OptionWithId(int id)
            => (id >= 0 && id < this._options.Count) ? this._options[id] : null;

        public void SetOptions(List<string> options)
        {
            this._options = options;
        }

        public void Stop()
            => this._cts.Cancel();

        public bool UserVoted(ulong uid)
            => this._votes.ContainsKey(uid);

        public bool VoteFor(ulong uid, int vote)
        {
            if (this._votes.ContainsKey(uid))
                return false;
            return this._votes.TryAdd(uid, vote);
        }
    }
}
