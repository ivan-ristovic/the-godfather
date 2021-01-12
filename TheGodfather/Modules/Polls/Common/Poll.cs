using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        public DateTimeOffset? EndTime { get; }
        public TimeSpan TimeUntilEnd => this.EndTime is null ? TimeSpan.Zero : this.EndTime.Value - DateTimeOffset.UtcNow;
        public DiscordMember Initiator { get; set; }
        public DiscordChannel Channel { get; protected set; }
        public IReadOnlyDictionary<ulong, int> Results => this.votes;

        public InteractivityExtension Interactivity { get; protected set; }

        protected readonly ConcurrentDictionary<ulong, int> votes;
        protected readonly CancellationTokenSource cts;


        public Poll(InteractivityExtension interactivity, DiscordChannel channel, DiscordMember sender, string question, TimeSpan runFor)
        {
            this.EndTime = DateTimeOffset.UtcNow + runFor;
            this.Channel = channel;
            this.Interactivity = interactivity;
            this.Question = question;
            this.Options = new List<string>();
            this.Initiator = sender;
            this.votes = new ConcurrentDictionary<ulong, int>();
            this.cts = new CancellationTokenSource();
        }


        public virtual async Task RunAsync(LocalizationService lcs)
        {
            this.IsRunning = true;
            DiscordMessage msgHandle = await this.Channel.SendMessageAsync(embed: this.ToDiscordEmbed(lcs));

            while (!this.cts.IsCancellationRequested) {
                try {
                    if (this.Channel.LastMessageId != msgHandle.Id) {
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

            this.IsRunning = false;

            await this.Channel.SendMessageAsync(embed: this.ResultsToDiscordEmbed(lcs));
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
