using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Currency.Services;

namespace TheGodfather.Modules.Currency;

[Group("shop")][Module(ModuleType.Currency)][NotBlocked]
[Aliases("store", "mall")]
[RequireGuild][Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class ShopModule : TheGodfatherServiceModule<ShopService>
{
    #region shop
    [GroupCommand]
    public Task ExecuteGroupAsync(CommandContext ctx)
        => this.ListAsync(ctx);
    #endregion

    #region shop purchases
    [Command("purchases")]
    [Aliases("myitems", "purchased", "bought")]
    public async Task GetPurchasedItemsAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember? member = null)
    {
        member ??= ctx.Member;

        IReadOnlyList<PurchasedItem> purchased = await this.Service.Purchases.GetAllCompleteAsync(member.Id);
        if (!purchased.Any()) {
            await ctx.FailAsync(TranslationKey.cmd_err_shop_purchased_none(member.Mention));
            return;
        }

        await ctx.PaginateAsync(
            TranslationKey.fmt_shop_purchased(member.ToDiscriminatorString()),
            purchased.OrderBy(i => i.Item.Price),
            i => $"{Formatter.Bold(i.Item.Name)} | {i.Item.Price}",
            this.ModuleColor,
            5
        );
    }
    #endregion

    #region shop add
    [Command("add")][Priority(1)]
    [Aliases("register", "reg", "additem", "a", "+", "+=", "<<", "<", "<-", "<=")]
    [RequireUserPermissions(Permissions.ManageGuild)]
    public async Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_shop_price)] long price,
        [RemainingText][Description(TranslationKey.desc_shop_name)] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_name_404);

        if (name.Length >= PurchasableItem.NameLimit)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_name(PurchasableItem.NameLimit));

        if (price is < 1 or > PurchasableItem.PriceLimit)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_shop_price(PurchasableItem.PriceLimit));

        await this.Service.AddAsync(new PurchasableItem {
            GuildId = ctx.Guild.Id,
            Price = price,
            Name = name
        });

        await ctx.InfoAsync(this.ModuleColor);
    }

    [Command("add")][Priority(0)]
    public Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_shop_name)] string name,
        [Description(TranslationKey.desc_shop_price)] long price)
        => this.AddAsync(ctx, price, name);
    #endregion

    #region shop buy
    [Command("buy")][UsesInteractivity][Priority(1)]
    [Aliases("purchase", "shutupandtakemymoney", "b", "p")]
    public async Task BuyAsync(CommandContext ctx,
        [Description(TranslationKey.desc_shop_ids)] params int[] ids)
    {
        await foreach (PurchasableItem? item in this.FetchItemsAsync(ctx, ids)) {
            if (item is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_shop_404);
            await this.InternalPurchaseAsync(ctx, item);
        }
    }

    [Command("buy")][UsesInteractivity][Priority(1)]
    public async Task BuyAsync(CommandContext ctx,
        [Description(TranslationKey.desc_shop_name)] string name)
    {
        IReadOnlyList<PurchasableItem> items = await this.Service.GetAllAsync(ctx.Guild.Id);
        PurchasableItem? item = items.FirstOrDefault(i => i.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        if (item is null)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_shop_404);

        await this.InternalPurchaseAsync(ctx, item);
    }


    #endregion

    #region shop sell
    [Command("sell")][UsesInteractivity]
    [Aliases("return")]
    public async Task SellAsync(CommandContext ctx,
        [Description(TranslationKey.desc_shop_ids)] params int[] ids)
    {
        await foreach (PurchasableItem? item in this.FetchItemsAsync(ctx, ids)) {
            if (item is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_shop_404);

            if (!await this.Service.Purchases.ContainsAsync(ctx.Guild.Id, item.Id))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_shop_sell(item.Name));

            string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
            long sellPrice = item.Price / 2;
            if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_shop_sell(item.Name, item.Price, currency)))
                return;

            await this.Service.Purchases.RemoveAsync(new PurchasedItem {
                ItemId = item.Id,
                UserId = ctx.User.Id
            });
            await ctx.Services.GetRequiredService<BankAccountService>().IncreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, sellPrice);

            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.MoneyBag, TranslationKey.fmt_shop_sell(ctx.User.Mention, item.Name, item.Price, currency));
        }

    }
    #endregion

    #region shop delete
    [Command("delete")][Priority(1)]
    [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    [RequireUserPermissions(Permissions.ManageGuild)]
    public async Task DeleteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_shop_del_ids)] params int[] ids)
    {
        if (ids is null || !ids.Any())
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_ids_none);

        int removed = await this.Service.RemoveAsync(ctx.Guild.Id, ids);
        await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_shop_delete(removed));
    }

    [Command("delete")][Priority(0)]
    public async Task DeleteAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_shop_ids)] string name)
    {
        IReadOnlyList<PurchasableItem> items = await this.Service.GetAllAsync(ctx.Guild.Id);
        int removed = await this.Service.RemoveAsync(items.Where(i => i.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)));
        await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_shop_delete(removed));
    }

    #endregion

    #region shop deleteall
    [Command("deleteall")][UsesInteractivity]
    [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    [RequireUserPermissions(Permissions.ManageGuild)]
    public async Task RemoveAllAsync(CommandContext ctx)
    {
        if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_shop_clear))
            return;

        IReadOnlyList<PurchasableItem> items = await this.Service.GetAllAsync(ctx.Guild.Id);
        await this.Service.RemoveAsync(items);
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region shop list
    [Command("list")]
    [Aliases("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync(CommandContext ctx)
    {
        IReadOnlyList<PurchasableItem> items = await this.Service.GetAllAsync(ctx.Guild.Id);
        if (!items.Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_shop_none);

        string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
        await ctx.PaginateAsync(
            TranslationKey.fmt_shop(ctx.Guild.Name),
            items,
            item => $"{Formatter.InlineCode($"{item.Id:D4}")} | {Formatter.Bold(item.Name)} : {Formatter.Bold(item.Price.ToString())} {currency}",
            this.ModuleColor,
            5
        );
    }
    #endregion


    #region internals
    private async Task InternalPurchaseAsync(CommandContext ctx, PurchasableItem item)
    {
        if (await this.Service.Purchases.ContainsAsync(ctx.User.Id, item.Id))
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_shop_purchased(item.Name));

        string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
        if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_shop_buy(item.Name, item.Price, currency)))
            return;

        if (!await ctx.Services.GetRequiredService<BankAccountService>().TryDecreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, item.Price))
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_funds_insuf);

        await this.Service.Purchases.AddAsync(new PurchasedItem {
            ItemId = item.Id,
            UserId = ctx.User.Id
        });

        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.MoneyBag, TranslationKey.fmt_shop_buy(ctx.User.Mention, item.Name, item.Price, currency));
    }

    private async IAsyncEnumerable<PurchasableItem?> FetchItemsAsync(CommandContext ctx, params int[] ids)
    {
        foreach (int id in ids.Distinct())
            yield return await this.Service.GetAsync(ctx.Guild.Id, id);
    }
    #endregion
}