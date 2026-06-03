using System.Globalization;
using System.Text;
using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Currency.Services;

namespace TheGodfather.Modules.Currency;

[Group("bank")]
[Alias("$", "$$", "$$$")]
[RequireContext(ContextType.Guild)]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class BankModule : TheGodfatherModule
{
    private const long MaxGrant = 1_000_000_000_000;

    private readonly BankAccountService bank;
    private readonly GuildConfigService guildConfig;


    public BankModule(LocalizationService localization, BankAccountService bank, GuildConfigService guildConfig)
        : base(localization, new Color(0x2ECC71))
    {
        this.bank = bank;
        this.guildConfig = guildConfig;
    }


    [Command("")]
    public Task ExecuteGroupAsync(IGuildUser? member = null)
        => this.GetBalanceAsync(member);

    [Command("balance")]
    [Alias("s", "status", "bal", "money")]
    public async Task GetBalanceAsync(IGuildUser? member = null)
    {
        member ??= (IGuildUser)this.Context.User;

        BankAccount? balance = await this.bank.GetAsync(this.Context.Guild.Id, member.Id);
        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithColor(this.ModuleColor);
            emb.WithLocalizedTitle(TranslationKey.fmt_bank_acc(Emojis.MoneyBag, UserDiscriminatorString(member)));
            emb.WithThumbnail(member.GetAvatarUrl() ?? member.GetDefaultAvatarUrl());
            if (balance is not null) {
                string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
                CultureInfo culture = this.Localization.GetGuildCulture(this.Context.Guild.Id);
                emb.WithLocalizedDescription(TranslationKey.fmt_bank_acc_value(balance.Balance.ToWords(culture), currency));
                emb.AddLocalizedField(TranslationKey.str_bank_acc_value_num, $"{balance.Balance:n0} {currency}");
            } else {
                emb.WithLocalizedDescription(TranslationKey.fmt_bank_acc_none(UserDisplayName(member)));
            }
            emb.WithLocalizedFooter(TranslationKey.fmt_bank_footer, this.Context.Client.CurrentUser.GetAvatarUrl());
        });
    }

    [Command("currency")]
    [Priority(1)]
    [Alias("setcurrency", "curr")]
    public async Task CurrencyAsync([Remainder] string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length > GuildConfig.CurrencyLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_currency(GuildConfig.CurrencyLimit));

        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.Currency = currency);
        await this.CurrencyAsync();
    }

    [Command("currency")]
    [Priority(0)]
    [Alias("setcurrency", "curr")]
    public Task CurrencyAsync()
    {
        CachedGuildConfig gcfg = this.guildConfig.GetCachedConfig(this.Context.Guild.Id);
        return this.ImpInfoAsync(this.ModuleColor, Emojis.MoneyBag, TranslationKey.str_currency_get(gcfg.Currency));
    }

    [Command("grant")]
    [Priority(1)]
    [Alias("give")]
    [RequirePrivilegedUser]
    public async Task GrantAsync(IGuildUser member, long amount)
    {
        if (amount is < 1 or > MaxGrant)
            throw new LocalizedCommandException(TranslationKey.cmd_err_bank_grant($"{MaxGrant:n0}"));

        if (await this.bank.GetAsync(this.Context.Guild.Id, member.Id) is null)
            throw new LocalizedCommandException(TranslationKey.fmt_bank_acc_none(UserDisplayName(member)));

        await this.bank.IncreaseBankAccountAsync(this.Context.Guild.Id, member.Id, amount);

        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
        await this.ImpInfoAsync(this.ModuleColor, Emojis.MoneyBag, TranslationKey.fmt_bank_grant(member.Mention, amount, currency));
    }

    [Command("grant")]
    [Priority(0)]
    [RequirePrivilegedUser]
    public Task GrantAsync(long amount, IGuildUser member)
        => this.GrantAsync(member, amount);

    [Command("register")]
    [Alias("r", "signup", "activate")]
    public async Task RegisterAsync()
    {
        if (await this.bank.ContainsAsync(this.Context.Guild.Id, this.Context.User.Id))
            throw new LocalizedCommandException(TranslationKey.cmd_err_bank_register);

        await this.bank.AddAsync(new BankAccount {
            GuildId = this.Context.Guild.Id,
            UserId = this.Context.User.Id
        });

        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
        await this.ImpInfoAsync(
            this.ModuleColor,
            Emojis.MoneyBag,
            TranslationKey.fmt_bank_register(this.Context.User.Mention, BankAccount.StartingBalance, currency)
        );
    }

    [Command("top")]
    [Alias("leaderboard", "elite")]
    public async Task TopAsync()
    {
        IReadOnlyList<BankAccount> top = await this.bank.GetTopAccountsAsync(this.Context.Guild.Id);
        var missing = new List<BankAccount>();
        string body = this.BuildTopList(top, missing);

        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_bank_top);
            emb.WithColor(this.ModuleColor);
            emb.WithDescription(body);
        });

        if (missing.Any())
            await this.bank.RemoveAsync(missing);
    }

    [Command("topglobal")]
    [Alias("globalleaderboard", "globalelite", "gtop", "topg", "globaltop")]
    public async Task GlobalTopAsync()
    {
        IReadOnlyList<BankAccount> top = await this.bank.GetTopAccountsAsync();
        var missing = new List<BankAccount>();
        string body = this.BuildTopList(top, missing);

        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_bank_topg);
            emb.WithColor(this.ModuleColor);
            emb.WithDescription(body);
        });

        if (missing.Any())
            await this.bank.RemoveAsync(missing);
    }

    [Command("transfer")]
    [Priority(1)]
    [Alias("lend", "tr")]
    public async Task TransferAsync(IGuildUser member, long amount)
    {
        if (amount is < 1 or > MaxGrant)
            throw new LocalizedCommandException(TranslationKey.cmd_err_bank_grant($"{MaxGrant:n0}"));

        if (member.Id == this.Context.User.Id)
            throw new LocalizedCommandException(TranslationKey.cmd_err_self_action);

        if (await this.bank.GetAsync(this.Context.Guild.Id, member.Id) is null)
            throw new LocalizedCommandException(TranslationKey.fmt_bank_acc_none(UserDisplayName(member)));

        if (!await this.bank.TransferAsync(this.Context.Guild.Id, this.Context.User.Id, member.Id, amount))
            throw new LocalizedCommandException(TranslationKey.cmd_err_funds_insuf);

        await this.InfoAsync(this.ModuleColor);
    }

    [Command("transfer")]
    [Priority(0)]
    public Task TransferAsync(long amount, IGuildUser member)
        => this.TransferAsync(member, amount);

    [Command("unregister")]
    [Alias("ur", "signout", "deleteaccount", "delacc", "disable", "deactivate")]
    [RequirePrivilegedUser]
    public async Task UnregisterAsync(IGuildUser member, bool global = false)
    {
        if (global)
            await this.bank.RemoveAsync(this.Context.Guild.Id, member.Id);
        else
            await this.bank.RemoveAllAsync(member.Id);

        await this.InfoAsync(this.ModuleColor);
    }

    private string BuildTopList(IEnumerable<BankAccount> top, ICollection<BankAccount> missing)
    {
        var sb = new StringBuilder();
        foreach (BankAccount account in top) {
            SocketGuildUser? user = this.Context.Guild.GetUser(account.UserId);
            if (user is null) {
                sb.AppendLine($"{Bold("?")} | {InlineCode($"{account.Balance:n0}")}");
                missing.Add(account);
            } else {
                sb.AppendLine($"{Bold(user.Mention)} | {InlineCode($"{account.Balance:n0}")}");
            }
        }

        return sb.ToString();
    }
}
