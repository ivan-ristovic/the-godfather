#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfatherBot
{
    [Description("Random number generation commands.")]
    public class CommandsGamble
    {
        #region COMMAND_ROLL
        [Command("roll"), Description("Rolls a dice.")]
        [Aliases("dice")]
        public async Task Roll(CommandContext ctx)
        {
            var rnd = new Random();
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":game_die:")} {ctx.User.Mention} rolled a {rnd.Next(1, 7)} !");
        }
        #endregion

        #region COMMAND_SLOT
        [Command("slot"), Description("Roll a slot machine.")]
        [Aliases("slotmachine")]
        public async Task SlotMachine(CommandContext ctx, [Description("Bid")] int bid = 5)
        {
            if (bid < 5) {
                await ctx.RespondAsync("5 is the minimum bid!");
                return;
            }

            if (!CommandsBank.RetrieveCreditsSucceeded(ctx.User.Id, bid)) {
                await ctx.RespondAsync("You do not have enough credits in WM bank!");
                return;
            }

            var slot_res = RollSlot(ctx);
            int won = EvaluateSlotResult(slot_res, bid);

            var embed = new DiscordEmbed() {
                Title = "TOTALLY NOT RIGGED SLOT MACHINE",
                Description = MakeStringFromResult(slot_res),
                Color = 0xFFFF00    // Yellow
            };
            var res = new DiscordEmbedField() {
                Name = "Result: ",
                Value = $"You won {won} credits!"
            };
            embed.Fields.Add(res);

            await ctx.RespondAsync("", embed: embed);

            if (won > 0)
                CommandsBank.IncreaseBalance(ctx.User.Id, won);
        }
        #endregion

        #region HELPER_FUNCTIONS
        private DiscordEmoji[,] RollSlot(CommandContext ctx)
        {
            DiscordEmoji[] emoji = {
                DiscordEmoji.FromName(ctx.Client, ":peach:"),
                DiscordEmoji.FromName(ctx.Client, ":moneybag:"),
                DiscordEmoji.FromName(ctx.Client, ":gift:"),
                DiscordEmoji.FromName(ctx.Client, ":large_blue_diamond:"),
                DiscordEmoji.FromName(ctx.Client, ":seven:"),
                DiscordEmoji.FromName(ctx.Client, ":cherries:")
            };

            var rnd = new Random();
            DiscordEmoji[,] result = new DiscordEmoji[3,3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    result[i, j] = emoji[rnd.Next(0, emoji.Length)];

            return result;
        }

        private string MakeStringFromResult(DiscordEmoji[,] res)
        {
            string s = "";
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++)
                    s += res[i, j].ToString();
                s += '\n';
            }
            return s;
        }

        private int EvaluateSlotResult(DiscordEmoji[,] res, int bid)
        {
            int pts = bid;

            // Rows
            for (int i = 0; i < 3; i++) {
                if (res[i, 0] == res[i, 1] && res[i, 1] == res[i, 2]) {
                    if (res[i, 0].ToString() == ":large_blue_diamond:")
                        pts *= 50;
                    else if (res[i, 0].ToString() == ":moneybag:")
                        pts *= 25;
                    else if (res[i, 0].ToString() == ":seven:")
                        pts *= 10;
                    else
                        pts *= 5;
                }
            }

            // Columns
            for (int i = 0; i < 3; i++) {
                if (res[0, i] == res[1, i] && res[1, i] == res[2, i]) {
                    if (res[0, i].ToString() == ":large_blue_diamond:")
                        pts *= 50;
                    else if (res[0, i].ToString() == ":moneybag:")
                        pts *= 25;
                    else if (res[0, i].ToString() == ":seven:")
                        pts *= 10;
                    else
                        pts *= 5;
                }
            }

            return pts == bid ? 0 : pts;
        }
        #endregion
    }

    [Group("bank", CanInvokeWithoutSubcommand = true)]
    [Description("$$$")]
    public class CommandsBank
    {
        #region STATIC_FIELDS
        private static Dictionary<ulong, int> _accounts = new Dictionary<ulong, int>();
        #endregion


        public async Task ExecuteGroup(CommandContext ctx)
        {
            await Status(ctx);
        }


        #region COMMAND_REGISTER
        [Command("register")]
        [Aliases("r", "signup", "activate")]
        public async Task Register(CommandContext ctx)
        {
            if (_accounts.ContainsKey(ctx.User.Id)) {
                await ctx.RespondAsync("You already own an account in WM bank!");
            } else {
                _accounts.Add(ctx.User.Id, 25);
                await ctx.RespondAsync("Account opened! Since WM bank is so generous, you get 25 credits for free.");
            }
        }
        #endregion

        #region COMMAND_STATUS
        [Command("status")]
        [Aliases("s", "balance")]
        public async Task Status(CommandContext ctx)
        {
            int ammount = 0;
            if (_accounts.ContainsKey(ctx.User.Id))
                ammount = _accounts[ctx.User.Id];

            var embed = new DiscordEmbed() {
                Title = "Account balance for " + ctx.User.Username,
                Timestamp = DateTime.Now,
                Color = 0xFFFF00    // Yellow
            };
            var balance = new DiscordEmbedField() {
                Name = "Balance: ",
                Value = ammount.ToString()
            };
            embed.Fields.Add(balance);
            await ctx.RespondAsync("", embed: embed);
        }
        #endregion

        #region COMMAND_TRANSFER
        [Command("transfer")]
        [Aliases("lend")]
        public async Task Transfer(CommandContext ctx,
                                  [Description("User to send credits to:")] DiscordUser u = null,
                                  [Description("User to send credits to:")] int ammount = 0)
        {
            if (u == null)
                throw new Exception("Account to transfer the credits to is missing.");

            if (!_accounts.ContainsKey(ctx.User.Id) || !_accounts.ContainsKey(u.Id)) {
                await ctx.RespondAsync("One or more accounts not found in the bank.");
                return;
            }
            
            if (ammount <= 0 || _accounts[ctx.User.Id] < ammount) {
                await ctx.RespondAsync("Invalid ammount (check your funds).");
                return;
            }

            _accounts[ctx.User.Id] -= ammount;
            _accounts[u.Id] += ammount;
        }
        #endregion


        #region HELPER_FUNCTIONS
        public static bool RetrieveCreditsSucceeded(ulong id, int ammount)
        {
            if (!_accounts.ContainsKey(id) || _accounts[id] < ammount)
                return false;
            _accounts[id] -= ammount;
            return true;
        }

        public static void IncreaseBalance(ulong id, int ammount)
        {
            if (!_accounts.ContainsKey(id))
                _accounts.Add(id, 0);
            _accounts[id] += ammount;
        }
        #endregion
    }
}
