#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Polls.Common
{
    public class Poll
    {
        public string Question { get; }
        public bool IsRunning { get; protected set; }
        public List<string> Options { get; set; }
        public TimeSpan TimeUntilEnd => this.endTime != null ? this.endTime - DateTime.Now : TimeSpan.Zero;

        protected DateTime endTime;
        protected readonly ConcurrentDictionary<ulong, int> votes;
        protected readonly DiscordChannel channel;
        protected readonly InteractivityExtension interactivity;
        protected readonly CancellationTokenSource cts;


        public Poll(InteractivityExtension interactivity, DiscordChannel channel, string question)
        {
            this.Question = question;
            this.channel = channel;
            this.interactivity = interactivity;
            this.Options = new List<string>();
            this.votes = new ConcurrentDictionary<ulong, int>();
            this.cts = new CancellationTokenSource();
        }


        public virtual async Task RunAsync(TimeSpan timespan)
        {
            this.IsRunning = true;
            var msgHandle = await this.channel.SendMessageAsync(embed: ToDiscordEmbed());

            this.endTime = DateTime.Now + timespan;
            while (!this.cts.IsCancellationRequested) {
                try {
                    if (this.channel.LastMessageId != msgHandle.Id) {
                        await msgHandle.DeleteAsync();
                        msgHandle = await this.channel.SendMessageAsync(embed: ToDiscordEmbed());
                    } else {
                        await msgHandle.ModifyAsync(embed: ToDiscordEmbed());
                    }
                } catch {
                    msgHandle = await this.channel.SendMessageAsync(embed: ToDiscordEmbed());
                }

                if (this.TimeUntilEnd.TotalSeconds < 1)
                    break;

                try {
                    await Task.Delay(this.TimeUntilEnd <= TimeSpan.FromSeconds(5) ? this.TimeUntilEnd : TimeSpan.FromSeconds(5), this.cts.Token);
                } catch (TaskCanceledException) {
                    await this.channel.InformFailureAsync("The poll has been cancelled!");
                }
            }

            this.IsRunning = false;

            await this.channel.SendMessageAsync(embed: ResultsToDiscordEmbed());
        }

        public virtual DiscordEmbed ToDiscordEmbed()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = Question,
                Description = $"Vote by typing {Formatter.InlineCode("!vote <number>")}",
                Color = DiscordColor.Orange
            };

            for (int i = 0; i < this.Options.Count; i++)
                if (!string.IsNullOrWhiteSpace(this.Options[i]))
                    emb.AddField($"{i + 1} : {this.Options[i]}", $"{this.votes.Count(kvp => kvp.Value == i)} vote(s)");

            if (this.endTime != null) {
                if (this.TimeUntilEnd.TotalSeconds > 1)
                    emb.WithFooter($"Poll ends in: {this.TimeUntilEnd:hh\\:mm\\:ss}");
                else
                    emb.WithFooter($"Poll ended.");
            }

            return emb.Build();
        }

        public virtual DiscordEmbed ResultsToDiscordEmbed()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = this.Question + " (results)",
                Color = DiscordColor.Orange
            };

            for (int i = 0; i < this.Options.Count; i++)
                emb.AddField(this.Options[i], this.votes.Count(kvp => kvp.Value == i).ToString(), inline: true);

            return emb.Build();
        }

        public bool CancelVote(ulong uid)
        {
            if (!this.votes.ContainsKey(uid))
                return true;
            return this.votes.TryRemove(uid, out _);
        }

        public bool IsValidVote(int vote)
            => vote >= 0 && vote < this.Options.Count;

        public string OptionWithId(int id)
            => (id >= 0 && id < this.Options.Count) ? this.Options[id] : null;

        public void Stop()
            => this.cts.Cancel();

        public bool UserVoted(ulong uid)
            => this.votes.ContainsKey(uid);

        public bool VoteFor(ulong uid, int vote)
        {
            if (this.votes.ContainsKey(uid))
                return false;
            return this.votes.TryAdd(uid, vote);
        }
    }
}
