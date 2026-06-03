using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Currency.Services;

namespace TheGodfather.Modules.Currency;

[Group("shop")]
[Alias("store", "mall")]
[RequireContext(ContextType.Guild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class ShopModule : TheGodfatherModule
{
    private readonly ShopService shop;
    private readonly BankAccountService bank;
    private readonly GuildConfigService guildConfig;


    public ShopModule(
        LocalizationService localization,
        ShopService shop,
        BankAccountService bank,
        GuildConfigService guildConfig)
        : base(localization, new Color(0x2ECC71))
    {
        this.shop = shop;
        this.bank = bank;
        this.guildConfig = guildConfig;
    }


    [Command("")]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("purchases")]
    [Alias("myitems", "purchased", "bought")]
    public async Task GetPurchasedItemsAsync(IGuildUser? member = null)
    {
        member ??= (IGuildUser)this.Context.User;

        IReadOnlyList<PurchasedItem> purchased = await this.shop.Purchases.GetAllCompleteAsync(member.Id);
        if (!purchased.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_shop_purchased_none(member.Mention));

        await this.RespondPagedAsync(
            TranslationKey.fmt_shop_purchased(UserDiscriminatorString(member)),
            purchased.OrderBy(item => item.Item.Price),
            item => $"{Bold(item.Item.Name)} | {item.Item.Price}",
            5
        );
    }

    [Command("add")]
    [Alias("register", "reg", "additem", "a", "+", "+=", "<<", "<", "<-", "<=")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task AddAsync([Remainder] string args)
    {
        (long price, string name) = ParsePriceName(args);
        if (string.IsNullOrWhiteSpace(name))
            throw new LocalizedCommandException(TranslationKey.cmd_err_name_404);

        if (name.Length >= PurchasableItem.NameLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_name(PurchasableItem.NameLimit));

        if (price is < 1 or > PurchasableItem.PriceLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_shop_price(PurchasableItem.PriceLimit));

        await this.shop.AddAsync(new PurchasableItem {
            GuildId = this.Context.Guild.Id,
            Price = price,
            Name = name
        });

        await this.InfoAsync(this.ModuleColor);
    }

    [Command("buy")]
    [Alias("purchase", "shutupandtakemymoney", "b", "p")]
    public async Task BuyAsync([Remainder] string itemSpec)
    {
        if (TryParseIds(itemSpec, out int[] ids)) {
            foreach (int id in ids.Distinct()) {
                PurchasableItem? item = await this.shop.GetAsync(this.Context.Guild.Id, id);
                if (item is null)
                    throw new LocalizedCommandException(TranslationKey.cmd_err_shop_404);
                await this.InternalPurchaseAsync(item);
            }
            return;
        }

        IReadOnlyList<PurchasableItem> items = await this.shop.GetAllAsync(this.Context.Guild.Id);
        PurchasableItem? namedItem = items.FirstOrDefault(item => item.Name.Equals(itemSpec, StringComparison.InvariantCultureIgnoreCase));
        if (namedItem is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_shop_404);

        await this.InternalPurchaseAsync(namedItem);
    }

    [Command("sell")]
    [Alias("return")]
    public async Task SellAsync([Remainder] string idSpec)
    {
        if (!TryParseIds(idSpec, out int[] ids) || !ids.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_ids_none);

        foreach (int id in ids.Distinct()) {
            PurchasableItem? item = await this.shop.GetAsync(this.Context.Guild.Id, id);
            if (item is null)
                throw new LocalizedCommandException(TranslationKey.cmd_err_shop_404);

            if (!await this.shop.Purchases.ContainsAsync(this.Context.User.Id, item.Id))
                throw new LocalizedCommandException(TranslationKey.cmd_err_shop_sell(item.Name));

            string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
            long sellPrice = item.Price / 2;
            if (!await this.ConfirmAsync(TranslationKey.q_shop_sell(item.Name, sellPrice, currency)))
                return;

            await this.shop.Purchases.RemoveAsync(new PurchasedItem {
                ItemId = item.Id,
                UserId = this.Context.User.Id
            });
            await this.bank.IncreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, sellPrice);

            await this.ImpInfoAsync(this.ModuleColor, Emojis.MoneyBag, TranslationKey.fmt_shop_sell(this.Context.User.Mention, item.Name, sellPrice, currency));
        }
    }

    [Command("delete")]
    [Alias("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task DeleteAsync([Remainder] string itemSpec)
    {
        int removed;
        if (TryParseIds(itemSpec, out int[] ids)) {
            if (!ids.Any())
                throw new LocalizedCommandException(TranslationKey.cmd_err_ids_none);
            removed = await this.shop.RemoveAsync(this.Context.Guild.Id, ids);
        } else {
            IReadOnlyList<PurchasableItem> items = await this.shop.GetAllAsync(this.Context.Guild.Id);
            removed = await this.shop.RemoveAsync(items.Where(item => item.Name.Equals(itemSpec, StringComparison.InvariantCultureIgnoreCase)));
        }

        await this.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_shop_delete(removed));
    }

    [Command("deleteall")]
    [Alias("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task RemoveAllAsync()
    {
        if (!await this.ConfirmAsync(TranslationKey.q_shop_clear))
            return;

        IReadOnlyList<PurchasableItem> items = await this.shop.GetAllAsync(this.Context.Guild.Id);
        await this.shop.RemoveAsync(items);
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l")]
    public async Task ListAsync()
    {
        IReadOnlyList<PurchasableItem> items = await this.shop.GetAllAsync(this.Context.Guild.Id);
        if (!items.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_shop_none);

        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
        await this.RespondPagedAsync(
            TranslationKey.fmt_shop(this.Context.Guild.Name),
            items,
            item => $"{InlineCode($"{item.Id:D4}")} | {Bold(item.Name)} : {Bold(item.Price)} {currency}",
            5
        );
    }

    private async Task InternalPurchaseAsync(PurchasableItem item)
    {
        if (await this.shop.Purchases.ContainsAsync(this.Context.User.Id, item.Id))
            throw new LocalizedCommandException(TranslationKey.cmd_err_shop_purchased(item.Name));

        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
        if (!await this.ConfirmAsync(TranslationKey.q_shop_buy(item.Name, item.Price, currency)))
            return;

        if (!await this.bank.TryDecreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, item.Price))
            throw new LocalizedCommandException(TranslationKey.cmd_err_funds_insuf);

        await this.shop.Purchases.AddAsync(new PurchasedItem {
            ItemId = item.Id,
            UserId = this.Context.User.Id
        });

        await this.ImpInfoAsync(this.ModuleColor, Emojis.MoneyBag, TranslationKey.fmt_shop_buy(this.Context.User.Mention, item.Name, item.Price, currency));
    }

    private static (long Price, string Name) ParsePriceName(string args)
    {
        if (string.IsNullOrWhiteSpace(args))
            return (0, string.Empty);

        string[] split = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (split.Length < 2)
            return (0, args.Trim());

        if (long.TryParse(split[0], out long firstPrice))
            return (firstPrice, string.Join(' ', split.Skip(1)));

        if (long.TryParse(split[^1], out long lastPrice))
            return (lastPrice, string.Join(' ', split.Take(split.Length - 1)));

        return (0, args.Trim());
    }

    private static bool TryParseIds(string text, out int[] ids)
    {
        ids = Array.Empty<int>();
        if (string.IsNullOrWhiteSpace(text))
            return false;

        string[] split = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var parsed = new List<int>();
        foreach (string token in split) {
            if (!int.TryParse(token, out int id))
                return false;
            parsed.Add(id);
        }

        ids = parsed.ToArray();
        return true;
    }
}
