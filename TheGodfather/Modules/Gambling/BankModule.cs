#region USING_DIRECTIVES
using System;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Gambling
{
    [Group("bank")]
    [Description("Bank manipulation.")]
    [Aliases("$", "$$", "$$$")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class BankModule : GodfatherBaseModule
    {

        public BankModule(DatabaseService db) : base(db: db) { }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("User.")] DiscordUser user = null)
        {
            await GetBalanceAsync(ctx, user).ConfigureAwait(false);
        }


        #region COMMAND_BALANCE
        [Command("balance")]
        [Description("View account balance for given user. If the user is no given, checks sender's balance.")]
        [Aliases("s", "status", "bal", "money", "credits")]
        [UsageExample("!bank balance @Someone")]
        public async Task GetBalanceAsync(CommandContext ctx,
                                         [Description("User.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            int? balance = await DatabaseService.GetBalanceForUserAsync(user.Id)
                .ConfigureAwait(false);

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Account balance for {user.Username}",
                Description = $"{Formatter.Bold(balance.HasValue ? balance.ToString() : "No existing account!")}",
                Color = DiscordColor.Yellow
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GRANT
        [Command("grant"), Priority(1)]
        [Description("Magically give funds to a user.")]
        [Aliases("give")]
        [UsageExample("!bank grant @Someone 1000")]
        [UsageExample("!bank grant 1000 @Someone")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task GrantAsync(CommandContext ctx,
                                    [Description("User.")] DiscordUser user,
                                    [Description("Amount.")] int amount)
        {
            if (amount <= 0 || amount > 100000)
                throw new InvalidCommandUsageException("Invalid amount. Must be in range [1-100000].");

            if (!await DatabaseService.HasBankAccountAsync(ctx.User.Id).ConfigureAwait(false))
                throw new CommandFailedException("Given user does not have a WM bank account!");

            await DatabaseService.IncreaseBalanceForUserAsync(user.Id, amount)
                .ConfigureAwait(false);
            await ReplySuccessAsync(ctx, $"User {Formatter.Bold(user.Username)} won {Formatter.Bold(amount.ToString())} credits on a lottery! (seems legit)")
                .ConfigureAwait(false);
        }

        [Command("grant"), Priority(0)]
        public async Task GrantAsync(CommandContext ctx,
                                    [Description("Amount.")] int amount,
                                    [Description("User.")] DiscordUser user)
            => await GrantAsync(ctx, user, amount).ConfigureAwait(false);
        #endregion

        #region COMMAND_REGISTER
        [Command("register")]
        [Description("Create an account in WM bank.")]
        [Aliases("r", "signup", "activate")]
        [UsageExample("!bank register")]
        public async Task RegisterAsync(CommandContext ctx)
        {
            if (await DatabaseService.HasBankAccountAsync(ctx.User.Id).ConfigureAwait(false))
                throw new CommandFailedException("You already own an account in WM bank!");

            await DatabaseService.OpenBankAccountForUserAsync(ctx.User.Id)
                .ConfigureAwait(false);
            await ReplySuccessAsync(ctx, $"Account opened for you, {ctx.User.Mention}! Since WM bank is so generous, you get 25 credits for free.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_TOP
        [Command("top")]
        [Description("Print the richest users.")]
        [Aliases("leaderboard", "elite")]
        [UsageExample("!bank top")]
        public async Task GetLeaderboardAsync(CommandContext ctx)
        {
            var top = await DatabaseService.GetTopTenBankAccountsAsync()
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var row in top) {
                if (!ulong.TryParse(row["uid"], out ulong uid))
                    continue;
                var u = await ctx.Client.GetUserAsync(uid)
                    .ConfigureAwait(false);
                sb.AppendLine($"{Formatter.Bold(u.Username)} : {row["balance"]}");
            }

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = "WEALTHIEST PEOPLE IN WM BANK:",
                Description = sb.ToString(),
                Color = DiscordColor.Gold
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_TRANSFER
        [Command("transfer"), Priority(1)]
        [Description("Transfer funds from one account to another.")]
        [Aliases("lend")]
        [UsageExample("!bank transfer @Someone 40")]
        [UsageExample("!bank transfer 40 @Someone")]
        public async Task TransferCreditsAsync(CommandContext ctx,
                                              [Description("User to send credits to.")] DiscordUser user,
                                              [Description("Amount.")] int amount)
        {
            if (amount <= 0)
                throw new CommandFailedException("The amount must be positive integer.");

            if (user.Id == ctx.User.Id)
                throw new CommandFailedException("You can't transfer funds to yourself.");

            await DatabaseService.TransferCurrencyAsync(ctx.User.Id, user.Id, amount)
                .ConfigureAwait(false);

            await ReplySuccessAsync(ctx, $"Transfer from {Formatter.Bold(ctx.User.Username)} to {Formatter.Bold(user.Username)} is complete.")
                .ConfigureAwait(false);
        }

        [Command("transfer"), Priority(0)]
        public async Task TransferCreditsAsync(CommandContext ctx,
                                              [Description("Amount.")] int amount,
                                              [Description("User to send credits to.")] DiscordUser user)
            => await TransferCreditsAsync(ctx, user, amount).ConfigureAwait(false);
        #endregion
    }
}
