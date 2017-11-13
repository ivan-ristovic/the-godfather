#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
#endregion

namespace TheGodfather.Commands.Main
{
    public class CommandsPoll
    {
        #region PRIVATE_FIELDS
        private bool _eventset = false;
        private object _lock = new object();
        private ConcurrentDictionary<ulong, int[]> _options = new ConcurrentDictionary<ulong, int[]>();
        private ConcurrentDictionary<ulong, List<ulong>> _idsvoted = new ConcurrentDictionary<ulong, List<ulong>>();
        #endregion


        #region COMMAND_POLL
        [Command("poll")]
        [Description("Starts a poll in the channel.")]
        [Aliases("vote")]
        [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
        [PreExecutionCheck]
        public async Task Poll(CommandContext ctx, 
                              [RemainingText, Description("Question.")] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                throw new InvalidCommandUsageException("Poll requires a yes or no question.");

            if (_options.ContainsKey(ctx.Channel.Id))
                throw new CommandFailedException("Another poll is already running.");

            _options.TryAdd(ctx.Channel.Id, null);

            // Get poll options interactively
            await ctx.RespondAsync("And what will be the possible answers? (separate with semicolon ``;``)")
                .ConfigureAwait(false);
            var interactivity = ctx.Client.GetInteractivityModule();
            var msg = await interactivity.WaitForMessageAsync(
                xm => xm.Author.Id == ctx.User.Id && xm.Channel.Id == ctx.Channel.Id,
                TimeSpan.FromMinutes(1)
            ).ConfigureAwait(false);
            if (msg == null) {
                await ctx.RespondAsync("Nevermind...")
                    .ConfigureAwait(false);
                _options.TryRemove(ctx.Channel.Id, out _);
                return;
            }

            // Parse poll options
            List<string> poll_options = msg.Message.Content.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (poll_options.Count < 2)
                throw new InvalidCommandUsageException("Not enough poll options.");

            await ctx.RespondAsync(embed: EmbedPoll(q, poll_options))
                .ConfigureAwait(false);

            AddEntries(ctx.Channel.Id, poll_options.Count);

            TryToAddListenerEvent(ctx);

            await Task.Delay(TimeSpan.FromSeconds(30))
                .ConfigureAwait(false);

            await ctx.RespondAsync(embed: EmbedPollResults(ctx.Channel.Id, q, poll_options))
                .ConfigureAwait(false);

            RemoveEntries(ctx.Channel.Id);
        }
        #endregion


        #region HELPER_FUNCTIONS
        private void TryToAddListenerEvent(CommandContext ctx)
        {
            lock (_lock) {
                if (!_eventset) {
                    _eventset = true;
                    ctx.Client.MessageCreated += CheckForPollReply;
                }
            }
        }

        private async Task CheckForPollReply(MessageCreateEventArgs e)
        {
            if (e.Message.Author.IsBot || !_options.ContainsKey(e.Channel.Id))
                return;

            int vote;
            try {
                vote = int.Parse(e.Message.Content);
            } catch {
                return;
            }

            if (vote > 0 && vote <= _options[e.Channel.Id].Length) {
                if (_idsvoted[e.Channel.Id].Contains(e.Author.Id)) {
                    await e.Channel.SendMessageAsync("You have already voted.")
                        .ConfigureAwait(false);
                } else {
                    _idsvoted[e.Channel.Id].Add(e.Author.Id);
                    _options[e.Channel.Id][vote - 1]++;
                    await e.Channel.SendMessageAsync($"{e.Author.Mention} voted for: **{vote}**!")
                        .ConfigureAwait(false);
                }
            } else {
                await e.Channel.SendMessageAsync("Invalid poll option")
                    .ConfigureAwait(false);
            }
        }

        private void AddEntries(ulong id, int count)
        {
            _options[id] = new int[count];
            _idsvoted.TryAdd(id, new List<ulong>());
        }

        private void RemoveEntries(ulong id)
        {
            int[] r;
            _options.TryRemove(id, out r);
            List<ulong> l;
            _idsvoted.TryRemove(id, out l);
        }

        private DiscordEmbed EmbedPoll(string question, List<string> poll_options)
        {
            var embed = new DiscordEmbedBuilder() {
                Title = question,
                Color = DiscordColor.Orange
            };
            for (int i = 0; i < poll_options.Count; i++)
                if (!string.IsNullOrWhiteSpace(poll_options[i]))
                    embed.AddField($"{i + 1}", poll_options[i], inline: true);

            return embed;
        }

        private DiscordEmbed EmbedPollResults(ulong id, string question, List<string> poll_options)
        {
            var res = new DiscordEmbedBuilder() {
                Title = question + " (results)",
                Color = DiscordColor.Orange
            };
            for (int i = 0; i < poll_options.Count; i++)
                res.AddField(poll_options[i], _options[id][i].ToString(), inline: true);

            return res;
        }
        #endregion
    }
}
