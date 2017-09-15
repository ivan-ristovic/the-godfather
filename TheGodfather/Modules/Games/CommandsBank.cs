#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion


namespace TheGodfatherBot
{

    [Group("bank", CanInvokeWithoutSubcommand = true)]
    [Description("$$$")]
    [Aliases("$", "$$", "$$$")]
    public class CommandsBank
    {
        #region STATIC_FIELDS
        private static Dictionary<ulong, int> _accounts = new Dictionary<ulong, int>();
        #endregion


        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            await Status(ctx);
        }


        #region COMMAND_GRANT
        [Command("grant")]
        [Aliases("give")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task Register(CommandContext ctx,
                                  [Description("User")] DiscordUser u = null,
                                  [Description("Ammount")] int ammount = 0)
        {
            if (u == null || ammount <= 0 || ammount > 1000)
                throw new ArgumentException("Invalid user or ammount.");

            IncreaseBalance(u.Id, ammount);
            await ctx.RespondAsync($"User {u.Username} won {ammount} credits on a lottery! (seems legit)");
        }
        #endregion

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

            var embed = new DiscordEmbedBuilder() {
                Title = "Account balance for " + ctx.User.Username,
                Timestamp = DateTime.Now,
                Color = DiscordColor.Yellow
            };
            embed.AddField("Balance: ", ammount.ToString());
            await ctx.RespondAsync("", embed: embed);
        }
        #endregion

        #region COMMAND_TOP
        [Command("top")]
        [Aliases("leaderboard")]
        public async Task Top(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder() { Title = "WEALTHIEST PEOPLE IN WM BANK:" };

            int i = 10;
            foreach (var pair in _accounts.ToList().OrderBy(key => key.Value))
                if (i-- != 0) {
                    var username = ctx.Guild.GetMemberAsync(pair.Key).Result.Username;
                    embed.AddField(username, pair.Value.ToString(), inline: true);
                }

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
                throw new ArgumentException("Account to transfer the credits to is missing.");

            if (!_accounts.ContainsKey(ctx.User.Id) || !_accounts.ContainsKey(u.Id))
                throw new KeyNotFoundException("One or more accounts not found in the bank.");

            if (ammount <= 0 || _accounts[ctx.User.Id] < ammount)
                throw new ArgumentOutOfRangeException("Invalid ammount (check your funds).");

            _accounts[ctx.User.Id] -= ammount;
            _accounts[u.Id] += ammount;

            await ctx.RespondAsync($"Transfer from {ctx.User.Mention} to {u.Mention} is complete.");
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
