#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Games
{
    [Group("bank")]
    [Description("Bank manipulation.")]
    [Aliases("$", "$$", "$$$")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [PreExecutionCheck]
    public class BankModule
    {

        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                           [Description("User.")] DiscordUser u = null)
        {
            await GetStatusAsync(ctx, u).ConfigureAwait(false);
        }


        #region COMMAND_GRANT
        [Command("grant")]
        [Description("Magically give funds to a user.")]
        [Aliases("give")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task GrantAsync(CommandContext ctx,
                                    [Description("User.")] DiscordUser u,
                                    [Description("Amount.")] int amount)
        {
            if (u == null || amount <= 0 || amount > 1000)
                throw new InvalidCommandUsageException("Invalid user or amount.");

            if (!await ctx.Services.GetService<DatabaseService>().HasBankAccountAsync(ctx.User.Id).ConfigureAwait(false))
                throw new CommandFailedException("Given user does not have a WM bank account!");

            await ctx.Services.GetService<DatabaseService>().IncreaseBalanceForUserAsync(u.Id, amount)
                .ConfigureAwait(false);
            await ctx.RespondAsync($"User {Formatter.Bold(u.Username)} won {Formatter.Bold(amount.ToString())} credits on a lottery! (seems legit)")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_REGISTER
        [Command("register")]
        [Description("Create an account in WM bank.")]
        [Aliases("r", "signup", "activate")]
        public async Task RegisterAsync(CommandContext ctx)
        {
            if (await ctx.Services.GetService<DatabaseService>().HasBankAccountAsync(ctx.User.Id).ConfigureAwait(false))
                throw new CommandFailedException("You already own an account in WM bank!");

            await ctx.Services.GetService<DatabaseService>().OpenBankAccountForUserAsync(ctx.User.Id)
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

            int? balance = await ctx.Services.GetService<DatabaseService>().GetBalanceForUserAsync(u.Id)
                .ConfigureAwait(false);

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Account balance for {u.Username}",
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

            var top = await ctx.Services.GetService<DatabaseService>().GetTopBankAccountsAsync()
                .ConfigureAwait(false);
            foreach (var row in top) {
                ulong.TryParse(row["uid"], out ulong uid);
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
                                              [Description("Amount.")] int amount)
        {
            if (u == null)
                throw new InvalidCommandUsageException("Account to transfer the credits to is missing.");
            
            if (amount <= 0)
                throw new CommandFailedException("The amount must be positive integer.");

            await ctx.Services.GetService<DatabaseService>().TransferCurrencyAsync(ctx.User.Id, u.Id, amount)
                .ConfigureAwait(false);

            await ctx.RespondAsync($"Transfer from {ctx.User.Mention} to {u.Mention} is complete.")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
