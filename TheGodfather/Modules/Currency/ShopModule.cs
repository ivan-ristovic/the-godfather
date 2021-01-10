using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Currency.Services;

namespace TheGodfather.Modules.Currency
{
    [Group("shop"), Module(ModuleType.Currency), NotBlocked]
    [Aliases("store", "mall")]
    [RequireGuild, Cooldown(3, 5, CooldownBucketType.Guild)]
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
                                                [Description("desc-member")] DiscordMember? member = null)
        {
            member ??= ctx.Member;

            IReadOnlyList<PurchasedItem> purchased = await this.Service.Purchases.GetAllCompleteAsync(member.Id);
            if (!purchased.Any()) {
                await ctx.FailAsync("cmd-err-shop-purchased-none", member.Mention);
                return;
            }

            await ctx.PaginateAsync(
                "fmt-shop-purchased",
                purchased.OrderBy(i => i.Item.Price),
                i => $"{Formatter.Bold(i.Item.Name)} | {i.Item.Price}",
                this.ModuleColor,
                5,
                member.Mention
            );
        }
        #endregion

        #region shop add
        [Command("add"), Priority(1)]
        [Aliases("register", "reg", "additem", "a", "+", "+=", "<<", "<", "<-", "<=")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("desc-shop-price")] long price,
                                  [RemainingText, Description("desc-shop-name")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException(ctx, "cmd-err-name-404");

            if (name.Length >= PurchasableItem.NameLimit)
                throw new InvalidCommandUsageException(ctx, "cmd-err-name", PurchasableItem.NameLimit);

            if (price is < 1 or > PurchasableItem.PriceLimit)
                throw new InvalidCommandUsageException(ctx, "cmd-err-shop-price", PurchasableItem.PriceLimit);

            await this.Service.AddAsync(new PurchasableItem {
                GuildId = ctx.Guild.Id,
                Price = price,
                Name = name,
            });

            await ctx.InfoAsync(this.ModuleColor);
        }

        [Command("add"), Priority(0)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-shop-name")] string name,
                            [Description("desc-shop-price")] long price)
            => this.AddAsync(ctx, price, name);
        #endregion

        #region shop buy
        [Command("buy"), UsesInteractivity, Priority(1)]
        [Aliases("purchase", "shutupandtakemymoney", "b", "p")]
        public async Task BuyAsync(CommandContext ctx,
                                  [Description("desc-shop-ids")] params int[] ids)
        {
            await foreach (PurchasableItem? item in this.FetchItemsAsync(ctx, ids)) {
                if (item is null)
                    throw new CommandFailedException(ctx, "cmd-err-shop-404");
                await this.InternalPurchaseAsync(ctx, item);
            }
        }

        [Command("buy"), UsesInteractivity, Priority(1)]
        public async Task BuyAsync(CommandContext ctx,
                                  [Description("desc-shop-name")] string name)
        {
            IReadOnlyList<PurchasableItem> items = await this.Service.GetAllAsync(ctx.Guild.Id);
            PurchasableItem? item = items.FirstOrDefault(i => i.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (item is null)
                throw new CommandFailedException(ctx, "cmd-err-shop-404");

            await this.InternalPurchaseAsync(ctx, item);
        }


        #endregion

        #region shop sell
        [Command("sell"), UsesInteractivity]
        [Aliases("return")]
        public async Task SellAsync(CommandContext ctx,
                                   [Description("desc-shop-ids")] params int[] ids)
        {
            await foreach (PurchasableItem? item in this.FetchItemsAsync(ctx, ids)) {
                if (item is null)
                    throw new CommandFailedException(ctx, "cmd-err-shop-404");

                if (!await this.Service.Purchases.ContainsAsync(ctx.Guild.Id, item.Id))
                    throw new CommandFailedException(ctx, "cmd-err-shop-sell", item.Name);

                string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
                long sellPrice = item.Price / 2;
                if (!await ctx.WaitForBoolReplyAsync("q-shop-sell", args: new object[] { item.Name, item.Price, currency }))
                    return;

                await this.Service.Purchases.RemoveAsync(new PurchasedItem {
                    ItemId = item.Id,
                    UserId = ctx.User.Id,
                });
                await ctx.Services.GetRequiredService<BankAccountService>().IncreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, sellPrice);

                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.MoneyBag, "fmt-shop-sell", ctx.User.Mention, item.Name, item.Price, currency);
            }

        }
        #endregion

        #region shop delete
        [Command("delete"), Priority(1)]
        [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("desc-shop-del-ids")] params int[] ids)
        {
            if (ids is null || !ids.Any())
                throw new InvalidCommandUsageException(ctx, "cmd-err-ids-none");

            int removed = await this.Service.RemoveAsync(ctx.Guild.Id, ids);
            await ctx.ImpInfoAsync(this.ModuleColor, "fmt-shop-delete", removed);
        }

        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("desc-shop-item-ids")] string name)
        {
            IReadOnlyList<PurchasableItem> items = await this.Service.GetAllAsync(ctx.Guild.Id);
            int removed = await this.Service.RemoveAsync(items.Where(i => i.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)));
            await ctx.ImpInfoAsync(this.ModuleColor, "fmt-shop-delete", removed);
        }

        #endregion

        #region shop deleteall
        [Command("deleteall"), UsesInteractivity]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task RemoveAllAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("q-shop-clear"))
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
                throw new CommandFailedException(ctx, "cmd-err-shop-none");

            string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
            await ctx.PaginateAsync(
                "fmt-shop",
                items,
                item => $"{Formatter.InlineCode($"{item.Id:D4}")} | {Formatter.Bold(item.Name)} : {Formatter.Bold(item.Price.ToString())} {currency}",
                this.ModuleColor,
                5,
                ctx.Guild.Name
            );
        }
        #endregion


        #region internals
        private async Task InternalPurchaseAsync(CommandContext ctx, PurchasableItem item)
        {
            if (await this.Service.Purchases.ContainsAsync(ctx.User.Id, item.Id))
                throw new CommandFailedException(ctx, "cmd-err-shop-purchased", item.Name);

            string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
            if (!await ctx.WaitForBoolReplyAsync("q-shop-buy", args: new object[] { item.Name, item.Price, currency }))
                return;

            if (!await ctx.Services.GetRequiredService<BankAccountService>().TryDecreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, item.Price))
                throw new CommandFailedException(ctx, "cmd-err-funds-insuf");

            await this.Service.Purchases.AddAsync(new PurchasedItem {
                ItemId = item.Id,
                UserId = ctx.User.Id,
            });

            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.MoneyBag, "fmt-shop-buy", ctx.User.Mention, item.Name, item.Price, currency);
        }

        private async IAsyncEnumerable<PurchasableItem?> FetchItemsAsync(CommandContext ctx, params int[] ids)
        {
            foreach (int id in ids.Distinct())
                yield return await this.Service.GetAsync(ctx.Guild.Id, id);
        }
        #endregion
    }
}
