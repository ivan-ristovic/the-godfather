#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
#endregion

namespace TheGodfatherBot.Modules.Messages
{
    [Group("poll", CanInvokeWithoutSubcommand = true)]
    [Description("Starts a poll in the channel.")]
    [Aliases("vote")]
    public class CommandsPoll
    {
        #region PRIVATE_FIELDS
        private bool _eventset = false;
        private ConcurrentDictionary<ulong, int[]> _options = new ConcurrentDictionary<ulong, int[]>();
        private ConcurrentDictionary<ulong, List<ulong>> _idsvoted = new ConcurrentDictionary<ulong, List<ulong>>();
        #endregion


        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                           [RemainingText, Description("Question")] string s = null)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException("Poll requires a yes or no question.");

            if (_options.ContainsKey(ctx.Channel.Id))
                throw new Exception("Another poll is already running.");

            _options.TryAdd(ctx.Channel.Id, null);

            // Get poll options interactively
            await ctx.RespondAsync("And what will be the possible answers? (separate with comma)");
            var interactivity = ctx.Client.GetInteractivityModule();
            var msg = await interactivity.WaitForMessageAsync(
                xm => xm.Author.Id == ctx.User.Id && xm.Channel.Id == ctx.Channel.Id,
                TimeSpan.FromMinutes(1)
            );
            if (msg == null) {
                await ctx.RespondAsync("Nevermind...");
                return;
            }

            // Parse poll options
            var poll_options = msg.Message.Content.Split(',').ToList();
            poll_options.RemoveAll(str => string.IsNullOrWhiteSpace(str));
            if (poll_options.Count < 2)
                throw new ArgumentException("Not enough poll options.");

            // Write embed field representing the poll
            var embed = new DiscordEmbedBuilder() {
                Title = s,
                Color = DiscordColor.Orange
            };
            for (int i = 0; i < poll_options.Count; i++)
                if (!string.IsNullOrWhiteSpace(poll_options[i]))
                    embed.AddField($"{i + 1}", poll_options[i], inline: true);
            await ctx.RespondAsync("", embed: embed);

            // Setup poll settings
            _options[ctx.Channel.Id] = new int[poll_options.Count];
            _idsvoted.TryAdd(ctx.Channel.Id, new List<ulong>());
            if (!_eventset) {
                _eventset = true;
                ctx.Client.MessageCreated += CheckForPollReply;
            }

            // Poll expiration time, 30s
            await Task.Delay(30000);

            // Write embedded result
            var res = new DiscordEmbedBuilder() {
                Title = "Poll results",
                Color = DiscordColor.Orange
            };
            for (int i = 0; i < poll_options.Count; i++)
                res.AddField(poll_options[i], _options[ctx.Channel.Id][i].ToString(), inline: true);
            await ctx.RespondAsync("", embed: res);
        }
        

        #region HELPER_FUNCTIONS
        private Task CheckForPollReply(MessageCreateEventArgs e)
        {
            if (e.Message.Author.IsBot || !_options.ContainsKey(e.Channel.Id))
                return Task.CompletedTask;

            int vote;
            try {
                vote = int.Parse(e.Message.Content);
            } catch {
                return Task.CompletedTask;
            }

            if (vote > 0 && vote <= _options[e.Channel.Id].Length) {
                if (_idsvoted[e.Channel.Id].Contains(e.Author.Id)) {
                    e.Channel.SendMessageAsync("You have already voted.");
                } else {
                    _idsvoted[e.Channel.Id].Add(e.Author.Id);
                    _options[e.Channel.Id][vote - 1]++;
                    e.Channel.SendMessageAsync($"{e.Author.Mention} voted for: **{vote}**!");
                }
            } else {
                e.Channel.SendMessageAsync("Invalid poll option");
            }

            return Task.CompletedTask;
        }
        #endregion
    }
}
