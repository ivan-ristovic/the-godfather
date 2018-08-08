#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System;
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
    [Group("chicken"), Module(ModuleType.Chickens), NotBlocked]
    [Description("Manage your chicken. If invoked without subcommands, prints out your chicken information.")]
    [Aliases("cock", "hen", "chick", "coc", "cc")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [UsageExamples("!chicken",
                   "!chicken @Someone")]
    public partial class ChickenModule : TheGodfatherModule
    {

        public ChickenModule(SharedData shared, DBService db) 
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Yellow;
        }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("User.")] DiscordMember member = null)
            => InfoAsync(ctx, member);


        #region COMMAND_CHICKEN_FIGHT
        [Command("fight")]
        [Description("Make your chicken and another user's chicken fight eachother!")]
        [Aliases("f", "duel", "attack")]
        [UsageExamples("!chicken duel @Someone")]
        public async Task FightAsync(CommandContext ctx,
                                    [Description("Member whose chicken to fight.")] DiscordMember member)
        {
            if (this.Shared.GetEventInChannel(ctx.Channel.Id) is ChickenWar ambush)
                throw new CommandFailedException("There is a chicken war running in this channel. No fights are allowed before the war finishes.");

            if (member.Id == ctx.User.Id)
                throw new CommandFailedException("You can't fight against your own chicken!");

            Chicken chicken1 = await this.Database.GetChickenAsync(ctx.User.Id, ctx.Guild.Id);
            if (chicken1 != null) {
                if (chicken1.Stats.TotalVitality < 25)
                    throw new CommandFailedException($"{ctx.User.Mention}, your chicken is too weak to start a fight with another chicken! Heal it using {Formatter.InlineCode("chicken heal")} command.");
            } else {
                throw new CommandFailedException("You do not own a chicken!");
            }

            Chicken chicken2 = await this.Database.GetChickenAsync(member.Id, ctx.Guild.Id);
            if (chicken2 == null)
                throw new CommandFailedException("The specified user does not own a chicken!");

            if (Math.Abs(chicken1.Stats.TotalStrength - chicken2.Stats.TotalStrength) > 50)
                throw new CommandFailedException("The bare strength difference is too big (50 max)! Please find a more suitable opponent.");

            string header = $"{Formatter.Bold(chicken1.Name)} ({chicken1.Stats.ToShortString()}) {StaticDiscordEmoji.DuelSwords} {Formatter.Bold(chicken2.Name)} ({chicken2.Stats.ToShortString()}) {StaticDiscordEmoji.Chicken}\n\n";

            Chicken winner = chicken1.Fight(chicken2);
            winner.Owner = winner.OwnerId == ctx.User.Id ? ctx.User : member;
            Chicken loser = winner == chicken1 ? chicken2 : chicken1;
            int gain = winner.DetermineStrengthGain(loser);
            winner.Stats.BareStrength += gain;
            winner.Stats.BareMaxVitality -= gain;
            if (winner.Stats.TotalVitality == 0)
                winner.Stats.BareVitality = 1;
            loser.Stats.BareVitality -= 50;

            await this.Database.ModifyChickenAsync(winner, ctx.Guild.Id);
            if (loser.Stats.TotalVitality > 0)
                await this.Database.ModifyChickenAsync(loser, ctx.Guild.Id);
            else
                await this.Database.RemoveChickenAsync(loser.OwnerId, ctx.Guild.Id) ;

            await this.Database.IncreaseBankAccountBalanceAsync(winner.OwnerId, ctx.Guild.Id, gain * 2000);

            await InformAsync(ctx, StaticDiscordEmoji.Chicken,
                header +
                $"{StaticDiscordEmoji.Trophy} Winner: {Formatter.Bold(winner.Name)}\n\n" +
                $"{Formatter.Bold(winner.Name)} gained {Formatter.Bold(gain.ToString())} strength!\n" +
                (loser.Stats.TotalVitality > 0 ? $"{Formatter.Bold(loser.Name)} lost {Formatter.Bold("50")} HP!" : $"{Formatter.Bold(loser.Name)} died in the battle!") +
                $"\n\n{winner.Owner.Mention} won {gain * 200} credits."
                , important: true
            );
        }
        #endregion

        #region COMMAND_CHICKEN_FLU
        [Command("flu"), UsesInteractivity]
        [Description("Pay a well-known scientist to create a disease that disintegrates weak chickens.")]
        [Aliases("cancer", "disease", "blackdeath")]
        [UsageExamples("!chicken flu")]
        public async Task FluAsync(CommandContext ctx)
        {
            if (this.Shared.GetEventInChannel(ctx.Channel.Id) is ChickenWar ambush)
                throw new CommandFailedException("There is a chicken war running in this channel. No actions are allowed before the war finishes.");

            if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention}, are you sure you want to pay {Formatter.Bold("1,000,000")} credits to create a disease?"))
                return;

            if (!await this.Database.DecreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, 1000000))
                throw new CommandFailedException($"You do not have enought credits to pay for the disease creation!");

            short threshold = (short)GFRandom.Generator.Next(50, 100);
            await this.Database.FilterChickensByVitalityAsync(ctx.Guild.Id, threshold);

            await InformAsync(ctx, StaticDiscordEmoji.Chicken, $"The deadly chicken flu killed all chickens with vitality less than or equal to {Formatter.Bold(threshold.ToString())}!");
        }
        #endregion

        #region COMMAND_CHICKEN_HEAL
        [Command("heal")]
        [Description("Heal your chicken (+100 HP). There is one medicine made each 10 minutes, so you need to grab it before the others do!")]
        [Aliases("+hp", "hp")]
        [UsageExamples("!chicken heal")]
        [Cooldown(1, 300, CooldownBucketType.Guild)]
        public async Task HealAsync(CommandContext ctx)
        {
            if (this.Shared.GetEventInChannel(ctx.Channel.Id) is ChickenWar)
                throw new CommandFailedException("There is a chicken war running in this channel. You are not allowed to heal your chicken before the war finishes.");

            Chicken chicken = await this.Database.GetChickenAsync(ctx.User.Id, ctx.Guild.Id);
            if (chicken == null)
                throw new CommandFailedException("You do not own a chicken!");

            chicken.Stats.BareVitality += 100;
            await this.Database.ModifyChickenAsync(chicken, ctx.Guild.Id);

            await InformAsync(ctx, StaticDiscordEmoji.Chicken, $"{ctx.User.Mention} healed his chicken (+100 to current HP)!");
        }
        #endregion

        #region COMMAND_CHICKEN_INFO
        [Command("info")]
        [Description("View user's chicken info. If the user is not given, views sender's chicken info.")]
        [Aliases("information", "stats")]
        [UsageExamples("!chicken info @Someone")]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description("User.")] DiscordMember member = null)
        {
            if (member == null)
                member = ctx.Member;

            Chicken chicken = await this.Database.GetChickenAsync(member.Id, ctx.Guild.Id)
                .ConfigureAwait(false);
            if (chicken == null)
                throw new CommandFailedException($"User {member.Mention} does not own a chicken in this guild! Use command {Formatter.InlineCode("chicken buy")} to buy a chicken (1000 credits).");

            await ctx.RespondAsync(embed: chicken.ToDiscordEmbed(member));
        }
        #endregion

        #region COMMAND_CHICKEN_RENAME
        [Command("rename")]
        [Description("Rename your chicken.")]
        [Aliases("rn", "name")]
        [UsageExamples("!chicken name New Name")]
        public async Task RenameAsync(CommandContext ctx,
                                     [RemainingText, Description("New chicken name.")] string newname)
        {
            if (string.IsNullOrWhiteSpace(newname))
                throw new InvalidCommandUsageException("New name for your chicken is missing.");

            if (newname.Length < 2 || newname.Length > 30)
                throw new InvalidCommandUsageException("Name cannot be shorter than 2 and longer than 30 characters.");

            if (!newname.All(c => Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c)))
                throw new InvalidCommandUsageException("Name cannot contain characters that are not letters or digits.");

            if (this.Shared.GetEventInChannel(ctx.Channel.Id) is ChickenWar ambush)
                throw new CommandFailedException("There is a chicken war running in this channel. No renames are allowed before the war finishes.");

            Chicken chicken = await this.Database.GetChickenAsync(ctx.User.Id, ctx.Guild.Id);
            if (chicken == null)
                throw new CommandFailedException("You do not own a chicken!");

            chicken.Name = newname;
            await this.Database.ModifyChickenAsync(chicken, ctx.Guild.Id);
            await InformAsync(ctx, StaticDiscordEmoji.Chicken, $"{ctx.User.Mention} renamed his chicken to {Formatter.Bold(newname)}");
        }
        #endregion

        #region COMMAND_CHICKEN_SELL
        [Command("sell"), UsesInteractivity]
        [Description("Sell your chicken.")]
        [Aliases("s")]
        [UsageExamples("!chicken sell")]
        public async Task SellAsync(CommandContext ctx)
        {
            Chicken chicken = await this.Database.GetChickenAsync(ctx.User.Id, ctx.Guild.Id)
                .ConfigureAwait(false);
            if (chicken == null)
                throw new CommandFailedException("You do not own a chicken!");

            if (this.Shared.GetEventInChannel(ctx.Channel.Id) is ChickenWar ambush)
                throw new CommandFailedException("There is a chicken war running in this channel. No sells are allowed before the war finishes.");

            long price = chicken.SellPrice;
            if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention}, are you sure you want to sell your chicken for {Formatter.Bold($"{price:n0}")} credits?"))
                return;

            await this.Database.RemoveChickenAsync(ctx.User.Id, ctx.Guild.Id);
            await this.Database.IncreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, price);

            await InformAsync(ctx, StaticDiscordEmoji.Chicken, $"{ctx.User.Mention} sold {Formatter.Bold(chicken.Name)} for {Formatter.Bold($"{price:n0}")} credits!");
        }
        #endregion

        #region COMMAND_CHICKEN_TOP
        [Command("top")]
        [Description("View the list of strongest chickens in the current guild.")]
        [Aliases("best", "strongest")]
        [UsageExamples("!chicken top")]
        public async Task TopAsync(CommandContext ctx)
        {
            IReadOnlyList<Chicken> chickens = await this.Database.GetStrongestChickensAsync(ctx.Guild.Id);
            if (chickens == null || !chickens.Any())
                throw new CommandFailedException("No chickens bought in this guild.");

            foreach (Chicken chicken in chickens) {
                IReadOnlyList<ChickenUpgrade> upgrades = await this.Database.GetUpgradesForChickenAsync(ctx.User.Id, ctx.Guild.Id);
                chicken.Stats.Upgrades = upgrades;
                try {
                    chicken.Owner = await ctx.Client.GetUserAsync(chicken.OwnerId);
                } catch (Exception e) {
                    this.Shared.LogProvider.LogException(LogLevel.Warning, e);
                }
            }

            await ctx.SendCollectionInPagesAsync(
                "Strongest chickens in this guild:",
                chickens,
                c => $"{Formatter.Bold(c.Name)} | {c.Owner.Mention} | {c.Stats.TotalStrength} ({c.Stats.BareStrength}) STR",
                this.ModuleColor
            );
        }
        #endregion

        #region COMMAND_CHICKEN_TOPGLOBAL
        [Command("topglobal")]
        [Description("View the list of strongest chickens globally.")]
        [Aliases("bestglobally", "globallystrongest", "globaltop", "topg", "gtop")]
        [UsageExamples("!chicken topglobal")]
        public async Task GlobalTopAsync(CommandContext ctx)
        {
            IReadOnlyList<Chicken> chickens = await this.Database.GetStrongestChickensAsync();
            if (chickens == null || !chickens.Any())
                throw new CommandFailedException("No chickens bought.");

            foreach (Chicken chicken in chickens) {
                IReadOnlyList<ChickenUpgrade> upgrades = await this.Database.GetUpgradesForChickenAsync(ctx.User.Id, ctx.Guild.Id);
                chicken.Stats.Upgrades = upgrades;
                try {
                    chicken.Owner = await ctx.Client.GetUserAsync(chicken.OwnerId);
                } catch (Exception e) {
                    this.Shared.LogProvider.LogException(LogLevel.Warning, e);
                }
            }

            await ctx.SendCollectionInPagesAsync(
                "Strongest chickens globally:",
                chickens,
                c => $"{Formatter.Bold(c.Name)} | {c.Owner.Mention} | {c.Stats.TotalStrength} ({c.Stats.BareStrength}) STR",
                this.ModuleColor
            );
        }
        #endregion
    }
}
