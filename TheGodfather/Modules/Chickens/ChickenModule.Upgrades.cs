#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Chickens
{
    public partial class ChickenModule
    {
        [Group("upgrades"), Module(ModuleType.Chickens)]
        [Description("Upgrade your chicken with items you can buy using your credits from WM bank. Invoking the group lists all upgrades available.")]
        [Aliases("perks", "upgrade")]
        [UsageExample("!chicken upgrade")]
        public class UpgradeModule : TheGodfatherBaseModule
        {

            public UpgradeModule(DBService db) : base(db: db) { }


            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("ID of the upgrade to buy.")] int id)
                => BuyAsync(ctx, id);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [RemainingText, Description("Upgrade to buy.")] string name = null)
            {
                if (string.IsNullOrWhiteSpace(name))
                    return ListAsync(ctx);
                else
                    return BuyAsync(ctx, name);
            }


            #region COMMAND_CHICKEN_UPGRADE_LIST
            [Command("list"), Module(ModuleType.Chickens)]
            [Description("List all available upgrades.")]
            [Aliases("ls", "view")]
            [UsageExample("!chicken upgrade list")]
            public async Task ListAsync(CommandContext ctx)
            {
                var upgrades = await Database.GetAllChickenUpgradesAsync()
                    .ConfigureAwait(false);

                await ctx.SendPaginatedCollectionAsync(
                    "Available chicken upgrades",
                    upgrades,
                    upgrade => $"{upgrade.Id} | {upgrade.Name} | {Formatter.Bold(upgrade.Price.ToString())} | +{upgrade.Modifier} {upgrade.UpgradesStat.ToStatString()}",
                    DiscordColor.Orange
                ).ConfigureAwait(false);
            }
            #endregion


            private Task BuyAsync(CommandContext ctx,
                                 [RemainingText, Description("Upgrade to buy.")] string name = null)
            {
                return Task.CompletedTask;
            }

            private Task BuyAsync(CommandContext ctx,
                                 [Description("ID of the upgrade to buy.")] int id)
            {
                return Task.CompletedTask;
            }
        }
    }
}
