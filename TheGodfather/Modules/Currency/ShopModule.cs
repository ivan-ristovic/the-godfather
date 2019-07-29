#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Currency.Extensions;
using TheGodfather.Modules.Administration.Services;
#endregion

namespace TheGodfather.Modules.Currency
{
    [Group("shop"), Module(ModuleType.Currency), NotBlocked]
    [Description("Shop for items using WM credits from your bank account. If invoked without subcommand, lists all available items for purchase.")]
    [Aliases("store")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class ShopModule : TheGodfatherModule
    {

        public ShopModule(SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {
            
        }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);


        #region COMMAND_SHOP_ADD
        [Command("add"), Priority(1)]
        [Description("Add a new item to guild purchasable items list.")]
        [Aliases("+", "a", "+=", "<", "<<", "additem")]
        [UsageExampleArgs("Barbie 500", "\"New Barbie\" 500", "500 Newest Barbie")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Item price.")] long price,
                                  [RemainingText, Description("Item name.")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("The name  for the item is missing.");

            if (name.Length >= 60)
                throw new InvalidCommandUsageException("Item name cannot exceed 60 characters");

            CachedGuildConfig gcfg = ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);

            if (price <1  || price > 100_000_000_000)
                throw new InvalidCommandUsageException($"Item price must be positive and cannot exceed 100 billion {gcfg.Currency}.");

            using (DatabaseContext db = this.Database.CreateContext()) {
                db.PurchasableItems.Add(new DatabasePurchasableItem {
                    GuildId = ctx.Guild.Id,
                    Name = name,
                    Price = price
                });
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, $"Item {Formatter.Bold(name)} ({Formatter.Bold(price.ToString())} {gcfg.Currency}) successfully added to this guild's shop.", important: false);
        }

        [Command("add"), Priority(0)]
        public Task AddAsync(CommandContext ctx,
                            [Description("Item name.")] string name,
                            [Description("Item price.")] long price)
            => this.AddAsync(ctx, price, name);
        #endregion

        #region COMMAND_SHOP_BUY
        [Command("buy"), UsesInteractivity, Priority(1)]
        [Description("Purchase an item from this guild's shop.")]
        [Aliases("purchase", "shutupandtakemymoney", "b", "p")]
        [UsageExampleArgs("3")]
        public async Task BuyAsync(CommandContext ctx,
                                  [Description("Item ID.")] int id)
        {
            DatabasePurchasableItem item;
            using (DatabaseContext db = this.Database.CreateContext()) {
                item = await db.PurchasableItems.FindAsync(id);
                if (item is null || item.GuildId != ctx.Guild.Id)
                    throw new CommandFailedException("Item with such ID does not exist in this guild's shop!");

                if (db.PurchasedItems.Any(i => i.ItemId == id && i.UserId == ctx.User.Id))
                    throw new CommandFailedException("You have already purchased this item!");
            }

            CachedGuildConfig gcfg = ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);

            if (!await ctx.WaitForBoolReplyAsync($"Are you sure you want to buy a {Formatter.Bold(item.Name)} for {Formatter.Bold(item.Price.ToString())} {gcfg.Currency}?"))
                return;

            using (DatabaseContext db = this.Database.CreateContext()) {
                if (!await db.TryDecreaseBankAccountAsync(ctx.User.Id, ctx.Guild.Id, item.Price))
                    throw new CommandFailedException("You do not have enough money to purchase that item!");

                db.PurchasedItems.Add(new DatabasePurchasedItem {
                    ItemId = item.Id,
                    UserId = ctx.User.Id
                });

                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, StaticDiscordEmoji.MoneyBag, $"{ctx.User.Mention} bought a {Formatter.Bold(item.Name)} for {Formatter.Bold(item.Price.ToString())} {gcfg.Currency}!", important: false);
        }

        [Command("buy"), UsesInteractivity, Priority(1)]
        public async Task BuyAsync(CommandContext ctx,
                                  [Description("Item name.")] string name)
        {
            DatabasePurchasableItem item;
            using (DatabaseContext db = this.Database.CreateContext()) {
                item = db.PurchasableItems.FirstOrDefault(i => i.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (item is null || item.GuildId != ctx.Guild.Id)
                    throw new CommandFailedException("Item with such ID does not exist in this guild's shop!");

                if (db.PurchasedItems.Any(i => item.Id == i.ItemId && i.UserId == ctx.User.Id))
                    throw new CommandFailedException("You have already purchased this item!");
            }

            CachedGuildConfig gcfg = ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);

            if (!await ctx.WaitForBoolReplyAsync($"Are you sure you want to buy a {Formatter.Bold(item.Name)} for {Formatter.Bold(item.Price.ToString())} {gcfg.Currency}?"))
                return;

            using (DatabaseContext db = this.Database.CreateContext()) {
                if (!await db.TryDecreaseBankAccountAsync(ctx.User.Id, ctx.Guild.Id, item.Price))
                    throw new CommandFailedException("You do not have enough money to purchase that item!");

                db.PurchasedItems.Add(new DatabasePurchasedItem {
                    ItemId = item.Id,
                    UserId = ctx.User.Id
                });

                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, StaticDiscordEmoji.MoneyBag, $"{ctx.User.Mention} bought a {Formatter.Bold(item.Name)} for {Formatter.Bold(item.Price.ToString())} {gcfg.Currency}!", important: false);
        }


        #endregion

        #region COMMAND_SHOP_SELL
        [Command("sell"), UsesInteractivity]
        [Description("Sell a purchased item for half the buy price.")]
        [Aliases("return")]
        [UsageExampleArgs("3")]
        public async Task SellAsync(CommandContext ctx,
                                   [Description("Item ID.")] int id)
        {
            DatabasePurchasableItem item;
            DatabasePurchasedItem purchased;
            using (DatabaseContext db = this.Database.CreateContext()) {
                item = await db.PurchasableItems.FindAsync(id);
                if (item is null)
                    throw new CommandFailedException("Item with such ID does not exist in this guild's shop!");

                purchased = await db.PurchasedItems.FindAsync(item.Id, (long)ctx.User.Id);
                if (purchased == null)
                    throw new CommandFailedException("You did not purchase this item!");
            }

            CachedGuildConfig gcfg = ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);

            long retval = item.Price / 2;
            if (!await ctx.WaitForBoolReplyAsync($"Are you sure you want to sell a {Formatter.Bold(item.Name)} for {Formatter.Bold(retval.ToString())} {gcfg.Currency}?"))
                return;

            using (DatabaseContext db = this.Database.CreateContext()) {
                db.PurchasedItems.Remove(purchased);
                await db.ModifyBankAccountAsync(ctx.User.Id, ctx.Guild.Id, v => v + retval);
                await db.SaveChangesAsync();
            }
            
            await this.InformAsync(ctx, StaticDiscordEmoji.MoneyBag, $"{ctx.User.Mention} sold a {Formatter.Bold(item.Name)} for {Formatter.Bold(retval.ToString())} {gcfg.Currency}!", important: false);
        }
        #endregion

        #region COMMAND_SHOP_DELETE
        [Command("delete"), Priority(1)]
        [Description("Remove purchasable item from this guild item list. You can remove an item by ID or by name.")]
        [Aliases("-", "remove", "rm", "del", "-=", ">", ">>")]
        [UsageExampleArgs("Barbie", "5", "1 2 3 4 5")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("ID list of items to remove.")] params int[] ids)
        {
            if (ids is null || !ids.Any())
                throw new InvalidCommandUsageException("Missing item IDs.");

            using (DatabaseContext db = this.Database.CreateContext()) {
                foreach (int id in ids.Distinct()) {
                    var item = new DatabasePurchasableItem { Id = id, GuildId = ctx.Guild.Id };
                    if (db.PurchasableItems.Contains(item))
                        db.PurchasableItems.Remove(item);
                }
                await db.SaveChangesAsync();
            }
            
            await this.InformAsync(ctx, $"Removed items with the following IDs: {string.Join(", ", ids)}", important: false);
        }
        #endregion

        #region COMMAND_SHOP_LIST
        [Command("list")]
        [Description("List all purchasable items for this guild.")]
        [Aliases("ls")]
        public async Task ListAsync(CommandContext ctx)
        {
            List<DatabasePurchasableItem> items;
            using (DatabaseContext db = this.Database.CreateContext()) {
                items = await db.PurchasableItems
                    .Where(i => i.GuildId == ctx.Guild.Id)
                    .OrderBy(i => i.Price)
                    .ToListAsync();
            }

            if (!items.Any())
                throw new CommandFailedException("No items in shop!");

            await ctx.SendCollectionInPagesAsync(
                $"Items for guild {ctx.Guild.Name}",
                items,
                item => $"{Formatter.InlineCode($"{item.Id:D4}")} | {Formatter.Bold(item.Name)} : {Formatter.Bold(item.Price.ToString())} {ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency}",
                this.ModuleColor,
                5
            );
        }
        #endregion
    }
}
