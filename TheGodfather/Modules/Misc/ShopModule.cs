#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("shop"), Module(ModuleType.Miscellaneous)]
    [Description("Shop for items using WM credits from your bank account. If invoked without subcommand, lists all available items for purchase.")]
    [Aliases("store")]
    [UsageExample("!shop")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class ShopModule : TheGodfatherBaseModule
    {

        public ShopModule(DBService db) : base(db: db) { }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => ListAsync(ctx);


        #region COMMAND_SHOP_ADD
        [Command("add"), Priority(1)]
        [Module(ModuleType.Miscellaneous)]
        [Description("Add a new item to guild purchasable items list.")]
        [Aliases("+", "a")]
        [UsageExample("!shop add Barbie 500")]
        [UsageExample("!shop add \"New Barbie\" 500")]
        [UsageExample("!shop add 500 Newest Barbie")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Item price.")] int price,
                                  [RemainingText, Description("Item name.")] string name)
        {
            if (name?.Length >= 60)
                throw new InvalidCommandUsageException("Item name cannot exceed 60 characters");

            if (price <= 0 || price > 2000000000)
                throw new InvalidCommandUsageException("Item price must be positive and cannot exceed 2 billion credits.");

            await Database.AddPurchasableItemAsync(ctx.Guild.Id, name, price)
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"Item {Formatter.Bold(name)} ({Formatter.Bold(price.ToString())} credits) successfully added")
                .ConfigureAwait(false);
        }

        [Command("add"), Priority(0)]
        public Task AddAsync(CommandContext ctx,
                            [Description("Item name.")] string name,
                            [Description("Item price.")] int price)
            => AddAsync(ctx, price, name);
        #endregion

        #region COMMAND_SHOP_BUY
        [Command("buy"), Module(ModuleType.Miscellaneous)]
        [Description("Purchase an item from this guild's shop.")]
        [Aliases("purchase", "shutupandtakemymoney", "b", "p")]
        [UsageExample("!shop buy 3")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task BuyAsync(CommandContext ctx,
                                  [Description("Item ID.")] int id)
        {
            var item = await Database.GetPurchasableItemAsync(ctx.Guild.Id, id)
                .ConfigureAwait(false);
            if (item == null)
                throw new CommandFailedException("Item with such ID does not exist in this guild's shop!");

            if (!await Database.TakeCreditsFromUserAsync(ctx.User.Id, item.Price))
                throw new CommandFailedException("You do not have enough money to purchase that item!");

            await Database.RegisterPurchaseForItemAsync(ctx.User.Id, item.Id)
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"{ctx.User.Mention} bought a {Formatter.Bold(item.Name)} for {Formatter.Bold(item.Price.ToString())} credits!", ":moneybag:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SHOP_DELETE
        [Command("delete"), Priority(1)]
        [Module(ModuleType.Miscellaneous)]
        [Description("Remove purchasable item from this guild item list. You can remove by ID or by name.")]
        [Aliases("-", "remove", "rm", "del")]
        [UsageExample("!shop delete Barbie")]
        [UsageExample("!shop delete 5")]
        [UsageExample("!shop delete 1 2 3 4 5")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("ID list of items to remove.")] params int[] ids)
        {
            if (!ids.Any())
                throw new InvalidCommandUsageException("Missing item IDs.");

            await Database.RemovePurchasableItemsAsync(ctx.Guild.Id, ids)
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SHOP_LIST
        [Command("list"), Module(ModuleType.Miscellaneous)]
        [Description("List all purchasable items for this guild.")]
        [Aliases("ls")]
        [UsageExample("!shop list")]
        public async Task ListAsync(CommandContext ctx)
        {
            var items = await Database.GetPurchasableItemsForGuildAsync(ctx.Guild.Id)
                .ConfigureAwait(false);

            if (!items.Any())
                throw new CommandFailedException("No items in shop!");

            await ctx.SendPaginatedCollectionAsync(
                $"{ctx.Guild.Name}'s shop:",
                items,
                item => $"{item.Id} | {Formatter.Bold(item.Name)} : {Formatter.Bold(item.Price.ToString())} credits",
                DiscordColor.Azure,
                5
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
