using System.Globalization;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Currency.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Currency;

[Group("bank")][Module(ModuleType.Currency)][NotBlocked]
[Aliases("$", "$$", "$$$")]
[RequireGuild][Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class BankModule : TheGodfatherServiceModule<BankAccountService>
{
    #region bank
    [GroupCommand]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember? member = null)
        => this.GetBalanceAsync(ctx, member);
    #endregion

    #region bank balance
    [Command("balance")]
    [Aliases("s", "status", "bal", "money")]
    public async Task GetBalanceAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember? member = null)
    {
        member ??= ctx.Member;

        BankAccount? balance = await this.Service.GetAsync(ctx.Guild.Id, member.Id);
        await ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithColor(this.ModuleColor);
            emb.WithLocalizedTitle(TranslationKey.fmt_bank_acc(Emojis.MoneyBag, member.ToDiscriminatorString()));
            emb.WithThumbnail(member.AvatarUrl);
            if (balance is { }) {
                string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
                CultureInfo culture = this.Localization.GetGuildCulture(ctx.Guild.Id);
                emb.WithLocalizedDescription(TranslationKey.fmt_bank_acc_value(balance.Balance.ToWords(culture), currency));
                emb.AddLocalizedField(TranslationKey.str_bank_acc_value_num, $"{balance.Balance:n0} {currency}");
            } else {
                emb.WithLocalizedDescription(TranslationKey.fmt_bank_acc_none);
            }
            emb.WithLocalizedFooter(TranslationKey.fmt_bank_footer, ctx.Client.CurrentUser.AvatarUrl);
        });
    }
    #endregion

    #region bank currency
    [Command("currency")][Priority(1)]
    [Aliases("setcurrency", "curr")]
    public async Task CurrencyAsync(CommandContext ctx,
        [Description(TranslationKey.desc_currency)] string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length > GuildConfig.CurrencyLimit)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_currency(GuildConfig.CurrencyLimit));

        await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.Currency = currency);

        await this.CurrencyAsync(ctx);

        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_currency, currency, true);
        });
    }

    [Command("currency")][Priority(0)]
    public Task CurrencyAsync(CommandContext ctx)
    {
        CachedGuildConfig gcfg = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
        return ctx.ImpInfoAsync(this.ModuleColor, Emojis.MoneyBag, TranslationKey.str_currency_get(gcfg.Currency));
    }
    #endregion

    #region bank grant
    [Command("grant")][Priority(1)]
    [Aliases("give")]
    [RequirePrivilegedUser]
    public async Task GrantAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [Description(TranslationKey.desc_amount)] long amount)
    {
        if (amount is < 1 or > 1_000_000_000_000)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_bank_grant($"{1_000_000_000_000:n0}"));

        await this.Service.IncreaseBankAccountAsync(ctx.Guild.Id, member.Id, amount);

        string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.MoneyBag, TranslationKey.fmt_bank_grant(member.Mention, amount, currency));
    }

    [Command("grant")][Priority(0)]
    public Task GrantAsync(CommandContext ctx,
        [Description(TranslationKey.desc_amount)] long amount,
        [Description(TranslationKey.desc_member)] DiscordMember member)
        => this.GrantAsync(ctx, member, amount);
    #endregion

    #region bank register
    [Command("register")]
    [Aliases("r", "signup", "activate")]
    public async Task RegisterAsync(CommandContext ctx)
    {
        if (await this.Service.ContainsAsync(ctx.Guild.Id, ctx.User.Id))
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_bank_register);

        await this.Service.AddAsync(new BankAccount {
            GuildId = ctx.Guild.Id,
            UserId = ctx.User.Id
        });

        string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.MoneyBag, TranslationKey.fmt_bank_register(ctx.User.Mention, BankAccount.StartingBalance, currency));
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
        foreach (BankAccount account in top)
            try {
                DiscordMember u = await ctx.Guild.GetMemberAsync(account.UserId);
                sb.AppendLine($"{Formatter.Bold(u.Mention)} | {Formatter.InlineCode($"{account.Balance:n0}")}");
            } catch (NotFoundException) {
                LogExt.Debug(ctx, "Found 404 member while listing bank accouns: {UserId}", account.UserId);
                sb.AppendLine($"{Formatter.Bold("?")} | {Formatter.InlineCode($"{account.Balance:n0}")}");
                toRemove.Add(account);
            }

        await ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_bank_top);
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
        foreach (BankAccount account in top)
            try {
                DiscordMember u = await ctx.Guild.GetMemberAsync(account.UserId);
                sb.AppendLine($"{Formatter.Bold(u.Mention)} | {Formatter.InlineCode($"{account.Balance:n0}")}");
            } catch (NotFoundException) {
                LogExt.Debug(ctx, "Found 404 member while listing bank accouns: {UserId}", account.UserId);
                sb.AppendLine($"{Formatter.Bold("?")} | {Formatter.InlineCode($"{account.Balance:n0}")}");
                toRemove.Add(account);
            }

        await ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_bank_topg);
            emb.WithColor(this.ModuleColor);
            emb.WithDescription(sb);
        });

        if (toRemove.Any())
            await this.Service.RemoveAsync(toRemove);
    }
    #endregion

    #region bank transfer
    [Command("transfer")][Priority(1)]
    [Aliases("lend", "tr")]
    public async Task TransferAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [Description(TranslationKey.desc_amount)] long amount)
    {
        if (amount is < 1 or > 1_000_000_000_000)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_bank_grant($"{1_000_000_000_000:n0}"));

        if (member == ctx.User)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_self_action);

        if (!await this.Service.TransferAsync(ctx.Guild.Id, ctx.User.Id, member.Id, amount))
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_funds_insuf);

        await ctx.InfoAsync(this.ModuleColor);
    }

    [Command("transfer")][Priority(0)]
    public Task TransferAsync(CommandContext ctx,
        [Description(TranslationKey.desc_amount)] long amount,
        [Description(TranslationKey.desc_member)] DiscordMember member)
        => this.TransferAsync(ctx, member, amount);
    #endregion

    #region bank unregister
    [Command("unregister")]
    [Aliases("ur", "signout", "deleteaccount", "delacc", "disable", "deactivate")]
    [RequirePrivilegedUser]
    public async Task UnregisterAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [Description(TranslationKey.desc_bank_del_g)] bool global = false)
    {
        if (global)
            await this.Service.RemoveAsync(ctx.Guild.Id, member.Id);
        else
            await this.Service.RemoveAllAsync(member.Id);
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion
}