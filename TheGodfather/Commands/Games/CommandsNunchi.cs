#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Games
{
    public partial class CommandsGames
    {
        [Group("nunchi", CanInvokeWithoutSubcommand = true)]
        [Description("Nunchi game commands")]
        [Cooldown(2, 5, CooldownBucketType.User), Cooldown(3, 5, CooldownBucketType.Channel)]
        [CheckIgnore]
        public class CommandsNunchi
        {
            #region PRIVATE_FIELDS
            private ConcurrentDictionary<ulong, List<ulong>> _participants = new ConcurrentDictionary<ulong, List<ulong>>();
            private ConcurrentDictionary<ulong, bool> _started = new ConcurrentDictionary<ulong, bool>();
            #endregion


            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                await NewGameAsync(ctx);
            }


            #region COMMAND_NUNCHI_NEW
            [Command("new")]
            [Description("Start a new nunchi game.")]
            [Aliases("create")]
            public async Task NewGameAsync(CommandContext ctx)
            {
                if (_participants.ContainsKey(ctx.Channel.Id))
                    throw new CommandFailedException("Nunchi game already in progress!");

                _participants.TryAdd(ctx.Channel.Id, new List<ulong>());
                _started.TryAdd(ctx.Channel.Id, false);

                await ctx.RespondAsync("Nunchi will start in 30s or when there are 10 participants. Type " + Formatter.InlineCode("!game nunchi join") + " to join the game.")
                    .ConfigureAwait(false);
                await Task.Delay(30000)
                    .ConfigureAwait(false);

                if (_participants[ctx.Channel.Id].Count > 1) {
                    await StartGameAsync(ctx)
                        .ConfigureAwait(false);
                } else {
                    await ctx.RespondAsync("Not enough users joined the game.")
                        .ConfigureAwait(false);
                    StopGame(ctx.Channel.Id);
                }
            }
            #endregion

            #region COMMAND_NUNCHI_JOIN
            [Command("join")]
            [Description("Join a nunchi game.")]
            [Aliases("+", "compete")]
            public async Task JoinGameAsync(CommandContext ctx)
            {
                if (!_participants.ContainsKey(ctx.Channel.Id))
                    throw new CommandFailedException("There is no nunchi game in this channel!");

                if (_participants[ctx.Channel.Id].Any(id => id == ctx.User.Id))
                    throw new CommandFailedException("You are already participating in the game!");

                if (_started[ctx.Channel.Id])
                    throw new CommandFailedException("Game already started, you can't join it.");

                if (_participants[ctx.Channel.Id].Count >= 10)
                    throw new CommandFailedException("Game is full, kthxbye.");

                _participants[ctx.Channel.Id].Add(ctx.User.Id);

                await ctx.RespondAsync($"{ctx.User.Mention} joined the game.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_NUNCHI_RULES
            [Command("rules")]
            [Description("Explain the game.")]
            [Aliases("help")]
            public async Task RulesAsync(CommandContext ctx)
            {
                await ctx.RespondAsync(
                    "I will start by typing a number. Users have to count up by 1 from that number. " +
                    "If someone makes a mistake (types an incorrent number, or repeats the same number) " +
                    "they are out of the game. If nobody posts a number 20s after the last number was posted, " +
                    "then the user that posted that number wins the game."
                ).ConfigureAwait(false);
            }
            #endregion


            #region HELPER_FUNCTIONS
            private async Task StartGameAsync(CommandContext ctx)
            {
                int num = new Random().Next(1000);
                await ctx.RespondAsync(num.ToString())
                    .ConfigureAwait(false);

                var interactivity = ctx.Client.GetInteractivityModule();
                DiscordUser winner = null;
                while (_participants[ctx.Channel.Id].Count > 1) {
                    int n = 0;
                    var msg = await interactivity.WaitForMessageAsync(
                        xm => {
                            if (xm.Channel.Id != ctx.Channel.Id || xm.Author.IsBot)
                                return false;
                            if (!_participants[ctx.Channel.Id].Contains(xm.Author.Id))
                                return false;
                            try {
                                n = int.Parse(xm.Content);
                            } catch {
                                return false;
                            }
                            return true;
                        },
                        TimeSpan.FromSeconds(20)
                    ).ConfigureAwait(false);
                    if (msg == null || n == 0) {
                        if (winner == null) {
                            await ctx.RespondAsync("No replies, aborting...")
                                .ConfigureAwait(false);
                        } else {
                            await ctx.RespondAsync($"{winner.Mention} won due to no replies from other users!")
                                .ConfigureAwait(false);
                        }
                        StopGame(ctx.Channel.Id);
                        return;
                    } else if (n == num + 1) {
                        num++;
                        winner = msg.User;
                    } else {
                        await ctx.RespondAsync(msg.User.Mention + " lost!")
                            .ConfigureAwait(false);
                        if (winner != null && winner.Id == msg.User.Id)
                            winner = null;
                        _participants[ctx.Channel.Id].Remove(msg.User.Id);
                    }
                }

                await ctx.RespondAsync("Game over! Winner: " + winner.Mention)
                    .ConfigureAwait(false);
                StopGame(ctx.Channel.Id);
            }

            private void StopGame(ulong id)
            {
                _participants.TryRemove(id, out _);
                _started.TryRemove(id, out _);
            }
            #endregion
        }
    }
}