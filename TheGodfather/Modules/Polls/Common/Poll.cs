using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Common;
using TheGodfather.Modules.Polls.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Polls.Common
{
    public class Poll : IChannelEvent
    {
        public const int MinTimeSeconds = 10;
        public const int MaxTimeDays = 1;
        public const int MaxTimeSeconds = MaxTimeDays * 3600 * 24;
        public const int MaxPollOptions = 10;

        public string Question { get; }
        public bool IsRunning { get; protected set; }
        public List<string> Options { get; set; }
        public DateTimeOffset? EndTime { get; protected set; }
        public TimeSpan TimeUntilEnd => this.EndTime is null ? TimeSpan.Zero : this.EndTime.Value - DateTimeOffset.UtcNow;
        public TimeSpan TimeoutAfter { get; }
        public DiscordMember Initiator { get; }
        public DiscordChannel Channel { get; protected set; }
        public IReadOnlyDictionary<ulong, int> Results => this.votes;

        public InteractivityExtension Interactivity { get; protected set; }

        protected readonly ConcurrentDictionary<ulong, int> votes;
        protected readonly CancellationTokenSource cts;


        public Poll(InteractivityExtension interactivity, DiscordChannel channel, DiscordMember sender, string question, TimeSpan timeout)
        {
            this.Channel = channel;
            this.Interactivity = interactivity;
            this.Question = question;
            this.Options = new List<string>();
            this.Initiator = sender;
            this.TimeoutAfter = timeout;
            this.votes = new ConcurrentDictionary<ulong, int>();
            this.cts = new CancellationTokenSource();
        }


        public virtual async Task RunAsync(LocalizationService lcs)
        {
            this.EndTime = DateTimeOffset.UtcNow + this.TimeoutAfter;
            this.IsRunning = true;
            DiscordMessage msgHandle = await this.Channel.SendMessageAsync(embed: this.ToDiscordEmbed(lcs));

            while (!this.cts.IsCancellationRequested) {
                try {
                    IReadOnlyList<DiscordMessage> msgs = await this.Channel.GetMessagesAsync(1);
                    if (msgs.Any() && msgs.Single().Id != msgHandle.Id) {
                        await msgHandle.DeleteAsync();
                        msgHandle = await this.Channel.SendMessageAsync(embed: this.ToDiscordEmbed(lcs));
                    } else {
                        await msgHandle.ModifyAsync(embed: this.ToDiscordEmbed(lcs));
                    }
                } catch {
                    msgHandle = await this.Channel.SendMessageAsync(embed: this.ToDiscordEmbed(lcs));
                }

                if (this.TimeUntilEnd.TotalSeconds < 1)
                    break;

                await Task.Delay(
                    this.TimeUntilEnd <= TimeSpan.FromSeconds(MinTimeSeconds) ? this.TimeUntilEnd : TimeSpan.FromSeconds(MinTimeSeconds),
                    this.cts.Token
                );
            }

            await this.Channel.SendMessageAsync(embed: this.ResultsToDiscordEmbed(lcs));
            try {
                await msgHandle.DeleteAsync();
                await Task.Delay(100);
            } catch {
                // ignored
            }

            this.IsRunning = false;
        }

        public bool CancelVote(ulong uid)
            => !this.votes.ContainsKey(uid) || this.votes.TryRemove(uid, out _);

        public bool IsValidVote(int vote)
            => vote >= 0 && vote < this.Options.Count;

        public string? OptionWithId(int id)
            => (id >= 0 && id < this.Options.Count) ? this.Options[id] : null;

        public void Stop()
            => this.cts.Cancel();

        public bool UserVoted(ulong uid)
            => this.votes.ContainsKey(uid);

        public bool VoteFor(ulong uid, int vote)
            => !this.votes.ContainsKey(uid) && this.votes.TryAdd(uid, vote);
    }
}
