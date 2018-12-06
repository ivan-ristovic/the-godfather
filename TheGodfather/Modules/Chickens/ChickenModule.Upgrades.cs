#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Modules.Currency.Extensions;
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

            public UpgradeModule(SharedData shared, DatabaseContextBuilder db) 
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
                if (ids is null || !ids.Any())
                    throw new CommandFailedException("You need to specify the IDs of the upgrades you wish to purchase.");

                if (this.Shared.GetEventInChannel(ctx.Channel.Id) is ChickenWar)
                    throw new CommandFailedException("There is a chicken war running in this channel. No sells are allowed before the war finishes.");

                var chicken = Chicken.FromDatabase(this.Database, ctx.Guild.Id, ctx.User.Id);
                if (chicken is null)
                    throw new CommandFailedException($"You do not own a chicken in this guild! Use command {Formatter.InlineCode("chicken buy")} to buy a chicken (requires atleast 1000 {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}).");

                if (chicken.Stats.Upgrades.Any(u => ids.Contains(u.Id)))
                    throw new CommandFailedException("Your chicken already one of those upgrades!");

                using (DatabaseContext db = this.Database.CreateContext()) {
                    foreach (int id in ids) {
                        DatabaseChickenUpgrade upgrade = db.ChickenUpgrades.FirstOrDefault(u => u.Id == id);
                        if (upgrade is null)
                            throw new CommandFailedException($"An upgrade with ID {Formatter.InlineCode(id.ToString())} does not exist! Use command {Formatter.InlineCode("chicken upgrades")} to view all available upgrades.");

                        if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention} are you sure you want to buy {Formatter.Bold(upgrade.Name)} for {Formatter.Bold($"{upgrade.Cost :n0}")} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}?"))
                            return;

                        if (!await db.TryDecreaseBankAccountAsync(ctx.User.Id, ctx.Guild.Id, upgrade.Cost))
                            throw new CommandFailedException($"You do not have enough {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"} to buy that upgrade!");

                        db.ChickensBoughtUpgrades.Add(new DatabaseChickenBoughtUpgrade() {
                            Id = upgrade.Id,
                            GuildId = chicken.GuildId,
                            UserId = chicken.OwnerId
                        });
                        await this.InformAsync(ctx, StaticDiscordEmoji.Chicken, $"{ctx.User.Mention} bought upgraded his chicken with {Formatter.Bold(upgrade.Name)} (+{upgrade.Modifier}) {upgrade.UpgradesStat.ToShortString()}!");
                    }

                    await db.SaveChangesAsync();
                }
            }


            #region COMMAND_CHICKEN_UPGRADE_LIST
            [Command("list")]
            [Description("List all available upgrades.")]
            [Aliases("ls", "view")]
            [UsageExamples("!chicken upgrade list")]
            public async Task ListAsync(CommandContext ctx)
            {
                using (DatabaseContext db = this.Database.CreateContext()) {
                    await ctx.SendCollectionInPagesAsync(
                        "Available chicken upgrades",
                        db.ChickenUpgrades.OrderByDescending(u => u.Cost),
                        u => $"{Formatter.InlineCode($"{u.Id:D2}")} | {u.Name} | {Formatter.Bold($"{u.Cost:n0}")} | +{Formatter.Bold(u.Modifier.ToString())} {u.UpgradesStat.ToShortString()}",
                        this.ModuleColor
                    );
                }
            }
            #endregion
        }
    }
}
