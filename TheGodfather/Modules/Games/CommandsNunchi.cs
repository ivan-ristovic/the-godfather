#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot.Modules.Games
{
    [Group("nunchi", CanInvokeWithoutSubcommand = true)]
    [Description("Nunchi game commands")]
    public class CommandsNunchi
    {
        #region PRIVATE_FIELDS
        private ConcurrentDictionary<ulong, List<ulong>> _participants = new ConcurrentDictionary<ulong, List<ulong>>();
        private ConcurrentDictionary<ulong, bool> _started = new ConcurrentDictionary<ulong, bool>();
        #endregion


        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            await NewGame(ctx);
        }



        #region COMMAND_NUNCHI_NEW
        [Command("new")]
        [Description("Start a new nunchi game.")]
        [Aliases("create")]
        public async Task NewGame(CommandContext ctx)
        {
            if (_participants.ContainsKey(ctx.Channel.Id))
                throw new Exception("Nunchi game already in progress!");

            _participants.TryAdd(ctx.Channel.Id, new List<ulong>());
            _started.TryAdd(ctx.Channel.Id, false);

            await ctx.RespondAsync("Nunchi will start in 30s or when there are 10 participants. Type ``!nunchi join`` to join the game.");
            await Task.Delay(30000);

            if (_participants[ctx.Channel.Id].Count > 1)
                await StartGame(ctx);
            else {
                await ctx.RespondAsync("Not enough users joined the game.");
                StopGame(ctx);
            }
        }
        #endregion

        #region COMMAND_NUNCHI_JOIN
        [Command("join")]
        [Description("Join a nunchi game.")]
        [Aliases("+", "compete")]
        public async Task JoinGame(CommandContext ctx)
        {
            if (!_participants.ContainsKey(ctx.Channel.Id))
                throw new Exception("There is no nunchi game in this channel!");

            if (_participants[ctx.Channel.Id].Any(id => id == ctx.User.Id))
                throw new Exception("You are already participating in the game!");

            if (_started[ctx.Channel.Id])
                throw new Exception("Game already started, you can't join it.");

            if (_participants[ctx.Channel.Id].Count >= 10)
                throw new Exception("Game full.");

            _participants[ctx.Channel.Id].Add(ctx.User.Id);

            await ctx.RespondAsync($"{ctx.User.Mention} joined the game.");
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task StartGame(CommandContext ctx)
        {
            int num = new Random().Next(1000);
            await ctx.RespondAsync(num.ToString());

            var interactivity = ctx.Client.GetInteractivityModule();
            while (_participants[ctx.Channel.Id].Count > 1) {
                int n = 0;
                var msg = await interactivity.WaitForMessageAsync(
                    xm => {
                        if (xm.Channel.Id != ctx.Channel.Id || xm.Author.IsBot)
                            return false;
                        try {
                            n = int.Parse(xm.Content);
                        } catch {
                            return false;
                        }
                        return true;
                    },
                    TimeSpan.FromMinutes(1)
                );
                await ctx.RespondAsync(n.ToString());
                if (msg == null || n == 0) {
                    await ctx.RespondAsync("No reply, aborting...");
                    StopGame(ctx);
                    return;
                } else if (n == num + 1) {
                    num++;
                } else {
                    await ctx.RespondAsync(msg.User.Mention + " lost!");
                    _participants[ctx.Channel.Id].Remove(msg.User.Id);
                }
            }

            var winner = await ctx.Client.GetUserAsync(_participants[ctx.Channel.Id][0]);
            await ctx.RespondAsync("Game over! Winner: " + winner.Mention);
            StopGame(ctx);
        }

        private void StopGame(CommandContext ctx)
        {
            List<ulong> outl;
            _participants.TryRemove(ctx.Channel.Id, out outl);
            bool outb;
            _started.TryRemove(ctx.Channel.Id, out outb);
        }
        #endregion
    }
}