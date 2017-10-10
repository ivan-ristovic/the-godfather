#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Games
{
    [Group("bank", CanInvokeWithoutSubcommand = true)]
    [Description("$$$")]
    [Aliases("$", "$$", "$$$")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
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
        [Description("Magically give funds to a user.")]
        [Aliases("give")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task Register(CommandContext ctx,
                                  [Description("User.")] DiscordUser u = null,
                                  [Description("Ammount.")] int ammount = 0)
        {
            if (u == null || ammount <= 0 || ammount > 1000)
                throw new InvalidCommandUsageException("Invalid user or ammount.");

            IncreaseBalance(u.Id, ammount);
            await ctx.RespondAsync($"User {Formatter.Bold(u.Username)} won {Formatter.Bold(ammount.ToString())} credits on a lottery! (seems legit)");
        }
        #endregion

        #region COMMAND_REGISTER
        [Command("register")]
        [Description("Create an account in WM bank.")]
        [Aliases("r", "signup", "activate")]
        public async Task Register(CommandContext ctx)
        {
            if (_accounts.ContainsKey(ctx.User.Id)) {
                throw new CommandFailedException("You already own an account in WM bank!");
            } else {
                _accounts.Add(ctx.User.Id, 25);
                await ctx.RespondAsync("Account opened! Since WM bank is so generous, you get 25 credits for free.");
            }
        }
        #endregion

        #region COMMAND_STATUS
        [Command("status")]
        [Description("View your account balance.")]
        [Aliases("s", "balance")]
        public async Task Status(CommandContext ctx)
        {
            int ammount = 0;
            if (_accounts.ContainsKey(ctx.User.Id))
                ammount = _accounts[ctx.User.Id];

            var embed = new DiscordEmbedBuilder() {
                Title = "Account balance for " + Formatter.Bold(ctx.User.Username),
                Timestamp = DateTime.Now,
                Color = DiscordColor.Yellow
            };
            embed.AddField("Balance: ", ammount.ToString());
            await ctx.RespondAsync("", embed: embed);
        }
        #endregion

        #region COMMAND_TOP
        [Command("top")]
        [Description("Print the richest users.")]
        [Aliases("leaderboard")]
        public async Task Top(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder() { Title = "WEALTHIEST PEOPLE IN WM BANK:" };

            int i = 10;
            foreach (var pair in _accounts.ToList().OrderBy(key => key.Value)) {
                if (i-- != 0) {
                    var username = ctx.Guild.GetMemberAsync(pair.Key).Result.Username;
                    embed.AddField(username, pair.Value.ToString(), inline: true);
                }
            }

            await ctx.RespondAsync("", embed: embed);
        }
        #endregion

        #region COMMAND_TRANSFER
        [Command("transfer")]
        [Description("Transfer funds from one account to another.")]
        [Aliases("lend")]
        public async Task Transfer(CommandContext ctx,
                                  [Description("User to send credits to.")] DiscordUser u = null,
                                  [Description("Ammount.")] int ammount = 0)
        {
            if (u == null)
                throw new InvalidCommandUsageException("Account to transfer the credits to is missing.");

            if (!_accounts.ContainsKey(ctx.User.Id) || !_accounts.ContainsKey(u.Id))
                throw new CommandFailedException("One or more accounts not found in the bank.", new KeyNotFoundException());

            if (ammount <= 0 || _accounts[ctx.User.Id] < ammount)
                throw new CommandFailedException("Invalid ammount (check your funds).", new ArgumentOutOfRangeException());

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
