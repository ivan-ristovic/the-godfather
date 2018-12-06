#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Humanizer;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Currency.Extensions;
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

        public BankModule(SharedData shared, DatabaseContextBuilder db)
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
            user = user ?? ctx.User;

            long? balance;
            using (DatabaseContext db = this.Database.CreateContext()) {
                DatabaseBankAccount account = await db.BankAccounts.FindAsync((long)ctx.Guild.Id, (long)user.Id);
                balance = account?.Balance;
            }

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

                DatabaseGuildConfig gcfg = await this.ModifyGuildConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.Currency = currency;
                });
                
                await this.InformAsync(ctx, $"Changed the currency to: {gcfg.Currency}", important: false);
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

            using (DatabaseContext db = this.Database.CreateContext()) {
                DatabaseBankAccount account = await db.BankAccounts.FindAsync((long)ctx.Guild.Id, (long)user.Id);
                if (account is null)
                    throw new CommandFailedException("Given user does not have a WM bank account!");
                account.Balance += amount;
                db.BankAccounts.Update(account);
                await db.SaveChangesAsync();
            }
            
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
            using (DatabaseContext db = this.Database.CreateContext()) {
                if (await db.BankAccounts.FindAsync((long)ctx.Guild.Id, (long)ctx.User.Id) is null)
                    db.BankAccounts.Add(new DatabaseBankAccount() { GuildId = ctx.Guild.Id, UserId = ctx.User.Id });
                else
                    throw new CommandFailedException("You already own an account in WM bank!");

                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, StaticDiscordEmoji.MoneyBag, $"Account opened for you, {ctx.User.Mention}! Since WM bank is so generous, you get {DatabaseBankAccount.StartingBalance} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"} for free.");
        }
        #endregion

        #region COMMAND_BANK_TOP
        [Command("top")]
        [Description("Print the richest users.")]
        [Aliases("leaderboard", "elite")]
        [UsageExamples("!bank top")]
        public async Task GetLeaderboardAsync(CommandContext ctx)
        {
            List<DatabaseBankAccount> topAccounts;

            using (DatabaseContext db = this.Database.CreateContext()) {
                topAccounts = await db.BankAccounts
                    .Where(a => a.GuildId == ctx.Guild.Id)
                    .OrderByDescending(a => a.Balance)
                    .Take(10)
                    .ToListAsync();
            }

            var sb = new StringBuilder();
            foreach (DatabaseBankAccount account in topAccounts) {
                try {
                    DiscordUser u = await ctx.Client.GetUserAsync(account.UserId);
                    sb.AppendLine($"{Formatter.Bold(u.Mention)} | {Formatter.InlineCode($"{account.Balance:n0}")}");
                } catch (NotFoundException) {

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
            List<DatabaseBankAccount> topAccounts;

            using (DatabaseContext db = this.Database.CreateContext()) {
                topAccounts = await db.BankAccounts
                    .OrderByDescending(a => a.Balance)
                    .Take(10)
                    .ToListAsync();
            }

            var sb = new StringBuilder();
            foreach (DatabaseBankAccount account in topAccounts) {
                try {
                    DiscordUser u = await ctx.Client.GetUserAsync(account.UserId);
                    sb.AppendLine($"{Formatter.Bold(u.Mention)} | {Formatter.InlineCode($"{account.Balance:n0}")}");
                } catch (NotFoundException) {

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

            using (DatabaseContext db = this.Database.CreateContext()) {
                try {
                    await db.Database.BeginTransactionAsync();

                    if (!await db.TryDecreaseBankAccountAsync(ctx.User.Id, ctx.Guild.Id, amount))
                        throw new CommandFailedException("You have insufficient funds.");
                    await db.ModifyBankAccountAsync(user.Id, ctx.Guild.Id, v => v + amount);
                    await db.SaveChangesAsync();
                    
                    db.Database.CommitTransaction();
                } catch {
                    db.Database.RollbackTransaction();
                    throw;
                }
            }

            await this.InformAsync(ctx, important: false);
        }

        [Command("transfer"), Priority(0)]
        public Task TransferCreditsAsync(CommandContext ctx,
                                        [Description("Amount of currency to transfer.")] long amount,
                                        [Description("User to send credits to.")] DiscordUser user)
            => this.TransferCreditsAsync(ctx, user, amount);
        #endregion

        #region COMMAND_BANK_UNREGISTER
        [Command("unregister"), Priority(1)]
        [Description("Delete an account from WM bank.")]
        [Aliases("ur", "signout", "deleteaccount", "delacc", "disable", "deactivate")]
        [UsageExamples("!bank unregister @Someone")]
        [RequirePrivilegedUser]
        public async Task UnregisterAsync(CommandContext ctx,
                                         [Description("User whose account to delete.")] DiscordUser user,
                                         [Description("Globally delete?")] bool global = false)
        {
            using (DatabaseContext db = this.Database.CreateContext()) {
                if (global) { 
                    db.BankAccounts.RemoveRange(db.BankAccounts.Where(a => a.UserId == user.Id));
                } else {
                    DatabaseBankAccount acc = db.BankAccounts.SingleOrDefault(a => a.GuildId == ctx.Guild.Id && a.UserId == user.Id);
                    if (acc is null)
                        throw new CommandFailedException($"User {Formatter.Bold(user.Username)} does not have a bank account in this guild.");
                    db.BankAccounts.Remove(acc);
                }
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, important: false);
        }

        [Command("unregister"), Priority(0)]
        public Task UnregisterAsync(CommandContext ctx,
                                   [Description("User whose account to delete.")] DiscordMember member)
            => this.UnregisterAsync(ctx, member as DiscordUser, false);
        #endregion
    }
}
