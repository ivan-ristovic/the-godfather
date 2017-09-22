#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
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
        private int _opnum = 0;
        private int[] _votes;
        private HashSet<ulong> _idsvoted;
        private DiscordChannel _pollchannel;
        #endregion
        

        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                           [RemainingText, Description("Question")] string s = null)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException("Poll requires a yes or no question.");

            if (_opnum != 0)
                throw new Exception("Another poll is already running.");

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
            var poll_options = msg.Message.Content.Split(',');
            if (poll_options.Length < 2)
                throw new ArgumentException("Not enough poll options.");

            // Write embed field representing the poll
            var embed = new DiscordEmbedBuilder() {
                Title = s,
                Color = DiscordColor.Orange
            };
            for (int i = 0; i < poll_options.Length; i++)
                embed.AddField($"{i + 1}", poll_options[i], inline: true);
            await ctx.RespondAsync("", embed: embed);

            // Setup poll settings
            _opnum = poll_options.Length;
            _votes = new int[_opnum];
            _idsvoted = new HashSet<ulong>();
            _pollchannel = ctx.Channel;
            ctx.Client.MessageCreated += CheckForPollReply;

            // Poll expiration time, 30s
            await Task.Delay(30000);

            // Allow another poll and stop listening for votes
            _opnum = 0;
            ctx.Client.MessageCreated -= CheckForPollReply;

            // Write embedded result
            var res = new DiscordEmbedBuilder() {
                Title = "Poll results",
                Color = DiscordColor.Orange
            };
            for (int i = 0; i < poll_options.Length; i++)
                res.AddField(poll_options[i], _votes[i].ToString(), inline: true);
            await ctx.RespondAsync("", embed: res);
        }
        

        #region HELPER_FUNCTIONS
        private Task CheckForPollReply(MessageCreateEventArgs e)
        {
            if (e.Message.Author.IsBot || e.Channel != _pollchannel)
                return Task.CompletedTask;

            int vote;
            try {
                vote = int.Parse(e.Message.Content);
            } catch {
                return Task.CompletedTask;
            }

            if (vote > 0 && vote <= _opnum) {
                if (_idsvoted.Contains(e.Author.Id)) {
                    e.Channel.SendMessageAsync("You have already voted.");
                } else {
                    _idsvoted.Add(e.Author.Id);
                    _votes[vote - 1]++;
                    e.Channel.SendMessageAsync($"{e.Author.Mention} voted for: " + vote);
                }
            } else {
                e.Channel.SendMessageAsync("Invalid poll option");
            }

            return Task.CompletedTask;
        }
        #endregion
    }
}
