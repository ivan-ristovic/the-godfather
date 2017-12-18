#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Helpers.DataManagers;
using TheGodfather.Exceptions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Games
{
    [Group("bank", CanInvokeWithoutSubcommand = true)]
    [Description("Bank manipulation.")]
    [Aliases("$", "$$", "$$$")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [PreExecutionCheck]
    public class CommandsBank
    {

        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            await GetStatusAsync(ctx);
        }


        #region COMMAND_GRANT
        [Command("grant")]
        [Description("Magically give funds to a user.")]
        [Aliases("give")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task GrantAsync(CommandContext ctx,
                                    [Description("User.")] DiscordUser u,
                                    [Description("Ammount.")] int ammount)
        {
            if (u == null || ammount <= 0 || ammount > 1000)
                throw new InvalidCommandUsageException("Invalid user or ammount.");

            if (!await ctx.Dependencies.GetDependency<DatabaseService>().HasBankAccountAsync(ctx.User.Id).ConfigureAwait(false))
                throw new CommandFailedException("Given user does not have a WM bank account!");

            await ctx.Dependencies.GetDependency<DatabaseService>().IncreaseBalanceForUserAsync(u.Id, ammount)
                .ConfigureAwait(false);
            await ctx.RespondAsync($"User {Formatter.Bold(u.Username)} won {Formatter.Bold(ammount.ToString())} credits on a lottery! (seems legit)")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_REGISTER
        [Command("register")]
        [Description("Create an account in WM bank.")]
        [Aliases("r", "signup", "activate")]
        public async Task RegisterAsync(CommandContext ctx)
        {
            if (await ctx.Dependencies.GetDependency<DatabaseService>().HasBankAccountAsync(ctx.User.Id).ConfigureAwait(false))
                throw new CommandFailedException("You already own an account in WM bank!");

            await ctx.Dependencies.GetDependency<DatabaseService>().OpenAccountForUserAsync(ctx.User.Id)
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Account opened for you, {ctx.User.Mention}! Since WM bank is so generous, you get 25 credits for free.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_STATUS
        [Command("status")]
        [Description("View account balance for user.")]
        [Aliases("s", "balance")]
        public async Task GetStatusAsync(CommandContext ctx,
                                        [Description("User.")] DiscordUser u = null)
        {
            if (u == null)
                u = ctx.User;

            var balance = await ctx.Dependencies.GetDependency<DatabaseService>().GetBalanceForUserAsync(ctx.User.Id)
                .ConfigureAwait(false);

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Account balance for {ctx.User.Username}",
                Description = $"{Formatter.Bold(balance.HasValue ? balance.ToString() : "No existing account!")}",
                Color = DiscordColor.Yellow
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_TOP
        [Command("top")]
        [Description("Print the richest users.")]
        [Aliases("leaderboard")]
        public async Task GetLeaderboardAsync(CommandContext ctx)
        {
            var em = new DiscordEmbedBuilder() {
                Title = "WEALTHIEST PEOPLE IN WM BANK:",
                Color = DiscordColor.Yellow
            };

            var top = await ctx.Dependencies.GetDependency<DatabaseService>().GetTopAccountsAsync()
                .ConfigureAwait(false);
            foreach (var row in top) {
                ulong uid = 0;
                ulong.TryParse(row["uid"], out uid);
                var member = await ctx.Guild.GetMemberAsync(uid)
                    .ConfigureAwait(false);
                em.AddField(member.Username, row["balance"], inline: true);
            }

            await ctx.RespondAsync(embed: em.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_TRANSFER
        [Command("transfer")]
        [Description("Transfer funds from one account to another.")]
        [Aliases("lend")]
        public async Task TransferCreditsAsync(CommandContext ctx,
                                              [Description("User to send credits to.")] DiscordUser u,
                                              [Description("Ammount.")] int ammount)
        {
            if (u == null)
                throw new InvalidCommandUsageException("Account to transfer the credits to is missing.");
            
            if (ammount <= 0)
                throw new CommandFailedException("The amount must be positive integer.");

            await ctx.Dependencies.GetDependency<DatabaseService>().TransferCurrencyAsync(ctx.User.Id, u.Id, ammount)
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Transfer from {ctx.User.Mention} to {u.Mention} is complete.")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
