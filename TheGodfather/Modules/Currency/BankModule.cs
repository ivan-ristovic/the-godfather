using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Currency.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Currency
{
    [Group("bank"), Module(ModuleType.Currency), NotBlocked]
    [Aliases("$", "$$", "$$$")]
    [RequireGuild, Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class BankModule : TheGodfatherServiceModule<BankAccountService>
    {
        #region bank
        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-member")] DiscordMember? member = null)
            => this.GetBalanceAsync(ctx, member);
        #endregion

        #region bank balance
        [Command("balance")]
        [Aliases("s", "status", "bal", "money")]
        public async Task GetBalanceAsync(CommandContext ctx,
                                         [Description("desc-member")] DiscordMember? member = null)
        {
            member ??= ctx.Member;

            BankAccount? balance = await this.Service.GetAsync(ctx.Guild.Id, member.Id);
            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedTitle("fmt-bank-acc", Emojis.MoneyBag, member.ToDiscriminatorString());
                emb.WithThumbnail(member.AvatarUrl);
                if (balance is { }) {
                    string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
                    CultureInfo culture = this.Localization.GetGuildCulture(ctx.Guild.Id);
                    emb.WithLocalizedDescription("fmt-bank-acc-value", balance.Balance.ToWords(culture), currency);
                    emb.AddLocalizedTitleField("fmt-bank-acc-value-num", $"{balance.Balance:n0}");
                } else {
                    emb.WithLocalizedDescription("fmt-bank-acc-none");
                }
                emb.WithLocalizedFooter("fmt-bank-footer", ctx.Client.CurrentUser.AvatarUrl);
            });
        }
        #endregion

        #region config currency
        [Command("currency"), Priority(1)]
        [Aliases("setcurrency", "curr")]
        public async Task CurrencyAsync(CommandContext ctx,
                                       [Description("desc-currency")] string currency)
        {
            if (string.IsNullOrWhiteSpace(currency) || currency.Length > GuildConfig.CurrencyLimit)
                throw new CommandFailedException(ctx, "cmd-err-currency");

            await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.Currency = currency);

            await this.CurrencyAsync(ctx);

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle("evt-cfg-upd");
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField("str-currency", currency, inline: true);
            });
        }

        [Command("suggestions"), Priority(0)]
        public Task CurrencyAsync(CommandContext ctx)
        {
            CachedGuildConfig gcfg = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
            return ctx.InfoAsync(this.ModuleColor, Emojis.MoneyBag, "str-currency-get", gcfg.Currency);
        }
        #endregion

        #region bank grant
        [Command("grant"), Priority(1)]
        [Aliases("give")]
        [RequirePrivilegedUser]
        public async Task GrantAsync(CommandContext ctx,
                                    [Description("desc-member")] DiscordMember member,
                                    [Description("desc-amount")] long amount)
        {
            if (amount < 1 || amount > 1_000_000_000_000)
                throw new InvalidCommandUsageException(ctx, "cmd-err-bank-grant", 1_000_000_000_000);

            await this.Service.IncreaseBankAccountAsync(ctx.Guild.Id, member.Id, amount);

            string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.MoneyBag, "fmt-bank-grant", member.Mention, amount, currency);
        }

        [Command("grant"), Priority(0)]
        public Task GrantAsync(CommandContext ctx,
                              [Description("desc-amount")] long amount,
                              [Description("desc-member")] DiscordMember member)
            => this.GrantAsync(ctx, member, amount);
        #endregion

        #region bank register
        [Command("register")]
        [Aliases("r", "signup", "activate")]
        public async Task RegisterAsync(CommandContext ctx)
        {
            if (await this.Service.ContainsAsync(ctx.Guild.Id, ctx.User.Id))
                throw new CommandFailedException(ctx, "cmd-err-bank-register");

            await this.Service.AddAsync(new BankAccount {
                GuildId = ctx.Guild.Id,
                UserId = ctx.User.Id,
            });

            string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.MoneyBag, "fmt-bank-register", ctx.User.Mention, BankAccount.StartingBalance, currency);
        }
        #endregion

        #region bank top
        [Command("top")]
        [Aliases("leaderboard", "elite")]
        public async Task TopAsync(CommandContext ctx)
        {
            IReadOnlyList<BankAccount> top = await this.Service.GetTopAccountsAsync(ctx.Guild.Id);

            var sb = new StringBuilder();
            var toRemove = new List<BankAccount>();
            foreach (BankAccount account in top) {
                try {
                    DiscordMember u = await ctx.Guild.GetMemberAsync(account.UserId);
                    sb.AppendLine($"{Formatter.Bold(u.Mention)} | {Formatter.InlineCode($"{account.Balance:n0}")}");
                } catch (NotFoundException) {
                    LogExt.Debug(ctx, "Found 404 member while listing bank accouns: {UserId}", account.UserId);
                    sb.AppendLine($"{Formatter.Bold("?")} | {Formatter.InlineCode($"{account.Balance:n0}")}");
                    toRemove.Add(account);
                }
            }

            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithLocalizedTitle("str-bank-top");
                emb.WithColor(this.ModuleColor);
                emb.WithDescription(sb);
            });

            if (toRemove.Any())
                await this.Service.RemoveAsync(toRemove);
        }
        #endregion

        #region bank topglobal
        [Command("topglobal")]
        [Aliases("globalleaderboard", "globalelite", "gtop", "topg", "globaltop")]
        public async Task GlobalTopAsync(CommandContext ctx)
        {
            IReadOnlyList<BankAccount> top = await this.Service.GetTopAccountsAsync();

            var sb = new StringBuilder();
            var toRemove = new List<BankAccount>();
            foreach (BankAccount account in top) {
                try {
                    DiscordMember u = await ctx.Guild.GetMemberAsync(account.UserId);
                    sb.AppendLine($"{Formatter.Bold(u.Mention)} | {Formatter.InlineCode($"{account.Balance:n0}")}");
                } catch (NotFoundException) {
                    LogExt.Debug(ctx, "Found 404 member while listing bank accouns: {UserId}", account.UserId);
                    sb.AppendLine($"{Formatter.Bold("?")} | {Formatter.InlineCode($"{account.Balance:n0}")}");
                    toRemove.Add(account);
                }
            }

            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithLocalizedTitle("str-bank-topg");
                emb.WithColor(this.ModuleColor);
                emb.WithDescription(sb);
            });

            if (toRemove.Any())
                await this.Service.RemoveAsync(toRemove);
        }
        #endregion

        #region bank transfer
        [Command("transfer"), Priority(1)]
        [Aliases("lend", "tr")]
        public async Task TransferAsync(CommandContext ctx,
                                       [Description("desc-member")] DiscordMember member,
                                       [Description("desc-amount")] long amount)
        {
            if (amount < 1 || amount > 1_000_000_000_000)
                throw new InvalidCommandUsageException(ctx, "cmd-err-bank-grant", $"{1_000_000_000_000:n0}");

            if (member == ctx.User)
                throw new CommandFailedException(ctx, "cmd-err-self-action");

            if (!await this.Service.TransferAsync(ctx.Guild.Id, ctx.User.Id, member.Id, amount))
                throw new CommandFailedException(ctx, "cmd-err-funds-insuf");

            await ctx.InfoAsync(this.ModuleColor);
        }

        [Command("transfer"), Priority(0)]
        public Task TransferAsync(CommandContext ctx,
                                 [Description("desc-amount")] long amount,
                                 [Description("desc-member")] DiscordMember member)
            => this.TransferAsync(ctx, member, amount);
        #endregion

        #region bank unregister
        [Command("unregister"), Priority(1)]
        [Aliases("ur", "signout", "deleteaccount", "delacc", "disable", "deactivate")]
        [RequirePrivilegedUser]
        public async Task UnregisterAsync(CommandContext ctx,
                                         [Description("desc-member")] DiscordMember member,
                                         [Description("desc-bank-del-g")] bool global = false)
        {
            if (global)
                await this.Service.RemoveAsync(ctx.Guild.Id, member.Id);
            else
                await this.Service.RemoveAllAsync(member.Id);
            await ctx.InfoAsync(this.ModuleColor);
        }

        [Command("unregister"), Priority(0)]
        public Task UnregisterAsync(CommandContext ctx,
                                   [Description("desc-member")] DiscordMember member)
            => this.UnregisterAsync(ctx, member, false);
        #endregion
    }
}
