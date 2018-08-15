#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Humanizer;

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Currency.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Currency
{
    [Group("bank"), Module(ModuleType.Currency), NotBlocked]
    [Description("WM bank commands. Group call prints out given user's bank balance. Accounts periodically get an increase.")]
    [Aliases("$", "$$", "$$$")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [UsageExamples("!bank",
                   "!bank @Someone")]
    public class BankModule : TheGodfatherModule
    {

        public BankModule(SharedData shared, DBService db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.DarkGreen;
        }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("User.")] DiscordUser user = null)
            => this.GetBalanceAsync(ctx, user);


        #region COMMAND_BANK_BALANCE
        [Command("balance")]
        [Description("View someone's bank account in this guild.")]
        [Aliases("s", "status", "bal", "money")]
        [UsageExamples("!bank balance @Someone")]
        public async Task GetBalanceAsync(CommandContext ctx,
                                         [Description("User.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            long? balance = await this.Database.GetBankAccountBalanceAsync(user.Id, ctx.Guild.Id);

            var emb = new DiscordEmbedBuilder() {
                Title = $"{StaticDiscordEmoji.MoneyBag} Bank account for {user.Username}",
                Color = this.ModuleColor,
                ThumbnailUrl = user.AvatarUrl
            };

            if (balance.HasValue) {
                emb.WithDescription($"Account value: {Formatter.Bold(balance.Value.ToWords())} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}");
                emb.AddField("Numeric value", $"{balance.Value:n0}");
            } else {
                emb.WithDescription($"No existing account! Use command {Formatter.InlineCode("bank register")} to open an account.");
            }
            emb.WithFooter("\"Your money is safe in our hands.\" - WM Bank");

            await ctx.RespondAsync(embed: emb.Build());
        }
        #endregion

        #region COMMAND_BANK_CURRENCY
        [Command("currency")]
        [Description("Set currency for this guild. Currency can be either emoji or text.")]
        [Aliases("sc", "setcurrency")]
        [UsageExamples("!bank currency :euro:",
                       "!bank currency My Custom Currency Name")]
        public async Task GetOrSetCurrencyAsync(CommandContext ctx,
                                               [RemainingText, Description("New currency.")] string currency = null)
        {
            if (string.IsNullOrWhiteSpace(currency)) {
                await this.InformAsync(ctx, StaticDiscordEmoji.MoneyBag, $"Currency for this guild: {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credit"}");
            } else {
                if (currency.Length > 30)
                    throw new CommandFailedException("Currency name cannot be longer than 30 characters!");

                this.Shared.GetGuildConfig(ctx.Guild.Id).Currency = currency;
                await this.Database.UpdateGuildSettingsAsync(ctx.Guild.Id, this.Shared.GetGuildConfig(ctx.Guild.Id));

                await this.InformAsync(ctx, $"Changed the currency to: {currency}", important: false);
            }
        }
        #endregion

        #region COMMAND_BANK_GRANT
        [Command("grant"), Priority(1)]
        [Description("Magically increase another user's bank balance.")]
        [Aliases("give")]
        [UsageExamples("!bank grant @Someone 1000",
                       "!bank grant 1000 @Someone")]
        [RequirePrivilegedUser]
        public async Task GrantAsync(CommandContext ctx,
                                    [Description("User.")] DiscordUser user,
                                    [Description("Amount.")] long amount)
        {
            if (amount < 0 || amount > 1_000_000_000_000)
                throw new InvalidCommandUsageException($"Invalid amount! Needs to be in range [1, {1_000_000_000_000:n0}]");

            if (!await this.Database.HasBankAccountAsync(user.Id, ctx.Guild.Id))
                throw new CommandFailedException("Given user does not have a WM bank account!");

            await this.Database.IncreaseBankAccountBalanceAsync(user.Id, ctx.Guild.Id, amount);
            await this.InformAsync(ctx, StaticDiscordEmoji.MoneyBag, $"{Formatter.Bold(user.Mention)} won {Formatter.Bold($"{amount:n0}")} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"} on the lottery! (seems legit)");
        }
        
        [Command("grant"), Priority(0)]
        public Task GrantAsync(CommandContext ctx,
                              [Description("Amount.")] long amount,
                              [Description("User.")] DiscordUser user)
            => this.GrantAsync(ctx, user, amount);
        #endregion

        #region COMMAND_BANK_REGISTER
        [Command("register")]
        [Description("Open an account in WM bank for this guild.")]
        [Aliases("r", "signup", "activate")]
        [UsageExamples("!bank register")]
        public async Task RegisterAsync(CommandContext ctx)
        {
            if (await this.Database.HasBankAccountAsync(ctx.User.Id, ctx.Guild.Id))
                throw new CommandFailedException("You already own an account in WM bank!");

            await this.Database.OpenBankAccountAsync(ctx.User.Id, ctx.Guild.Id);
            await this.InformAsync(ctx, StaticDiscordEmoji.MoneyBag, $"Account opened for you, {ctx.User.Mention}! Since WM bank is so generous, you get 10000 {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"} for free.");
        }
        #endregion

        #region COMMAND_BANK_TOP
        [Command("top")]
        [Description("Print the richest users.")]
        [Aliases("leaderboard", "elite")]
        [UsageExamples("!bank top")]
        public async Task GetLeaderboardAsync(CommandContext ctx)
        {
            IReadOnlyList<(ulong, long)> top = await this.Database.GetTopBankAccountsAsync(ctx.Guild.Id);

            var sb = new StringBuilder();
            foreach ((ulong uid, long balance) in top) {
                try {
                    DiscordUser u = await ctx.Client.GetUserAsync(uid);
                    sb.AppendLine($"{Formatter.Bold(u.Mention)} | {Formatter.InlineCode($"{balance:n0}")}");
                } catch (NotFoundException) {
                    await this.Database.CloseBankAccountAsync(uid);
                }
            }

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Wealthiest users for guild {ctx.Guild.Name}",
                Description = sb.ToString(),
                Color = this.ModuleColor
            }.Build());
        }
        #endregion

        #region COMMAND_BANK_TOPGLOBAL
        [Command("topglobal")]
        [Description("Print the globally richest users.")]
        [Aliases("globalleaderboard", "globalelite", "gtop", "topg", "globaltop")]
        [UsageExamples("!bank gtop")]
        public async Task GetGlobalLeaderboardAsync(CommandContext ctx)
        {
            IReadOnlyList<(ulong, long)> top = await this.Database.GetTopBankAccountsAsync();

            var sb = new StringBuilder();
            foreach ((ulong uid, long balance) in top) {
                try {
                    DiscordUser u = await ctx.Client.GetUserAsync(uid);
                    sb.AppendLine($"{Formatter.Bold(u.Mention)} | {Formatter.InlineCode($"{balance:n0}")}");
                } catch (NotFoundException) {
                    await this.Database.CloseBankAccountAsync(uid);
                }
            }

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = "Globally wealthiest users:",
                Description = sb.ToString(),
                Color = this.ModuleColor
            }.Build());
        }
        #endregion

        #region COMMAND_BANK_TRANSFER
        [Command("transfer"), Priority(1)]
        [Description("Transfer funds from your account to another one.")]
        [Aliases("lend")]
        [UsageExamples("!bank transfer @Someone 40",
                       "!bank transfer 40 @Someone")]
        public async Task TransferCreditsAsync(CommandContext ctx,
                                              [Description("User to send credits to.")] DiscordUser user,
                                              [Description("Amount of currency to transfer.")] long amount)
        {
            if (amount <= 0)
                throw new CommandFailedException("The transfer amount must be a positive value.");

            if (user.Id == ctx.User.Id)
                throw new CommandFailedException("You can't transfer funds to yourself.");

            await this.Database.TransferBetweenBankAccountsAsync(ctx.User.Id, user.Id, ctx.Guild.Id, amount);
            await this.InformAsync(ctx, important: false);
        }

        [Command("transfer"), Priority(0)]
        public Task TransferCreditsAsync(CommandContext ctx,
                                        [Description("Amount of currency to transfer.")] long amount,
                                        [Description("User to send credits to.")] DiscordUser user)
            => this.TransferCreditsAsync(ctx, user, amount);
        #endregion

        #region COMMAND_BANK_UNREGISTER
        [Command("unregister")]
        [Description("Delete an account from WM bank.")]
        [Aliases("ur", "signout", "deleteaccount", "delacc", "disable", "deactivate")]
        [UsageExamples("!bank unregister @Someone")]
        [RequirePrivilegedUser]
        public async Task UnregisterAsync(CommandContext ctx,
                                         [Description("User whose account to delete.")] DiscordUser user,
                                         [Description("Globally delete?")] bool global = false)
        {
            if (!await this.Database.HasBankAccountAsync(user.Id, ctx.Guild.Id))
                throw new CommandFailedException("There is no account registered for that user in WM bank!");

            if (global)
                await this.Database.CloseBankAccountAsync(user.Id);
            else
                await this.Database.CloseBankAccountAsync(user.Id, ctx.Guild.Id);

            await this.InformAsync(ctx, important: false);
        }
        #endregion
    }
}
