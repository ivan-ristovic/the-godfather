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
using TheGodfather.Services.Database;
using TheGodfather.Services.Database.Bank;
using TheGodfather.Services.Database.Chickens;
#endregion

namespace TheGodfather.Modules.Chickens
{
    public partial class ChickenModule
    {
        [Group("upgrades")]
        [Description("Upgrade your chicken with items you can buy using your credits from WM bank. Group call lists all available upgrades.")]
        [Aliases("perks", "upgrade", "u")]
        [UsageExamples("!chicken upgrade")]
        public class UpgradeModule : TheGodfatherModule
        {

            public UpgradeModule(DBService db) 
                : base(db: db)
            {
                this.ModuleColor = DiscordColor.Yellow;
            }


            [GroupCommand, Priority(1), UsesInteractivity]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("ID of the upgrade to buy.")] int id)
            {
                if (ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenWar)
                    throw new CommandFailedException("There is a chicken war running in this channel. No sells are allowed before the war finishes.");

                Chicken chicken = await this.Database.GetChickenAsync(ctx.User.Id, ctx.Guild.Id);
                if (chicken == null)
                    throw new CommandFailedException($"You do not own a chicken in this guild! Use command {Formatter.InlineCode("chicken buy")} to buy a chicken (requires atleast 1000 credits).");

                if (chicken.Stats.Upgrades.Any(u => u.Id == id))
                    throw new CommandFailedException("Your chicken already has that upgrade!");

                ChickenUpgrade upgrade = await this.Database.GetChickenUpgradeAsync(id);
                if (upgrade == null)
                    throw new CommandFailedException($"An upgrade with ID {Formatter.InlineCode(id.ToString())} does not exist! Use command {Formatter.InlineCode("chicken upgrades")} to view all available upgrades.");

                if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention}, are you sure you want to buy {Formatter.Bold(upgrade.Name)} for {Formatter.Bold($"{upgrade.Price:n0}")} credits?"))
                    return;

                if (!await this.Database.DecreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, upgrade.Price))
                    throw new CommandFailedException($"You do not have enought credits to buy that upgrade!");

                await this.Database.AddChickenUpgradeAsync(ctx.User.Id, ctx.Guild.Id, upgrade);
                await ctx.InformSuccessAsync(StaticDiscordEmoji.Chicken, $"{ctx.User.Mention} bought upgraded his chicken with {Formatter.Bold(upgrade.Name)} (+{upgrade.Modifier}) {upgrade.UpgradesStat.ToShortString()}!");
            }

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => ListAsync(ctx);


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
                    upgrades,
                    upgrade => $"{upgrade.Id} | {upgrade.Name} | {Formatter.Bold($"{upgrade.Price:n0}")} | +{upgrade.Modifier} {upgrade.UpgradesStat.ToShortString()}",
                    this.ModuleColor
                );
            }
            #endregion
        }
    }
}
