#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Modules.Chickens.Extensions;
using TheGodfather.Modules.Currency.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Chickens
{
    public partial class ChickenModule
    {
        [Group("upgrades"), UsesInteractivity]
        [Description("Upgrade your chicken with items you can buy using your credits from WM bank. Group call lists all available upgrades.")]
        [Aliases("perks", "upgrade", "u")]
        [UsageExamples("!chicken upgrade")]
        public class UpgradeModule : TheGodfatherModule
        {

            public UpgradeModule(SharedData shared, DBService db) 
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Yellow;
            }


            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => this.ListAsync(ctx);

            [GroupCommand, Priority(0)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("IDs of the upgrades to buy.")] params int[] ids)
            {
                if (!ids.Any())
                    throw new CommandFailedException("You need to specify the IDs of the upgrades you wish to purchase.");

                if (this.Shared.GetEventInChannel(ctx.Channel.Id) is ChickenWar)
                    throw new CommandFailedException("There is a chicken war running in this channel. No sells are allowed before the war finishes.");

                Chicken chicken = await this.Database.GetChickenAsync(ctx.User.Id, ctx.Guild.Id);
                if (chicken is null)
                    throw new CommandFailedException($"You do not own a chicken in this guild! Use command {Formatter.InlineCode("chicken buy")} to buy a chicken (requires atleast 1000 {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}).");

                if (chicken.Stats.Upgrades.Any(u => ids.Contains(u.Id)))
                    throw new CommandFailedException("Your chicken already one of those upgrades!");

                foreach (int id in ids) {
                    ChickenUpgrade upgrade = await this.Database.GetChickenUpgradeAsync(id);
                    if (upgrade is null)
                        throw new CommandFailedException($"An upgrade with ID {Formatter.InlineCode(ids.ToString())} does not exist! Use command {Formatter.InlineCode("chicken upgrades")} to view all available upgrades.");

                    if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention}, are you sure you want to buy {Formatter.Bold(upgrade.Name)} for {Formatter.Bold($"{upgrade.Price:n0}")} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}?"))
                        return;

                    if (!await this.Database.DecreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, upgrade.Price))
                        throw new CommandFailedException($"You do not have enought {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"} to buy that upgrade!");

                    await this.Database.AddChickenUpgradeAsync(ctx.User.Id, ctx.Guild.Id, upgrade);
                    await this.InformAsync(ctx, StaticDiscordEmoji.Chicken, $"{ctx.User.Mention} bought upgraded his chicken with {Formatter.Bold(upgrade.Name)} (+{upgrade.Modifier}) {upgrade.UpgradesStat.ToShortString()}!");
                }
            }


            #region COMMAND_CHICKEN_UPGRADE_LIST
            [Command("list")]
            [Description("List all available upgrades.")]
            [Aliases("ls", "view")]
            [UsageExamples("!chicken upgrade list")]
            public async Task ListAsync(CommandContext ctx)
            {
                IReadOnlyList<ChickenUpgrade> upgrades = await this.Database.GetAllChickenUpgradesAsync();

                await ctx.SendCollectionInPagesAsync(
                    "Available chicken upgrades",
                    upgrades.OrderByDescending(u => u.Price),
                    upgrade => $"{Formatter.InlineCode($"{upgrade.Id:D2}")} | {upgrade.Name} | {Formatter.Bold($"{upgrade.Price:n0}")} | +{Formatter.Bold(upgrade.Modifier.ToString())} {upgrade.UpgradesStat.ToShortString()}",
                    this.ModuleColor
                );
            }
            #endregion
        }
    }
}
