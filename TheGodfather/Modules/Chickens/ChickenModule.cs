#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Modules.Chickens.Extensions;
using TheGodfather.Modules.Currency.Extensions;
using TheGodfather.Services;
using TheGodfather.Services.Common;
#endregion

namespace TheGodfather.Modules.Chickens
{
    [Group("chicken"), Module(ModuleType.Chickens), NotBlocked]
    [Description("Manage your chicken. If invoked without subcommands, prints out your chicken information.")]
    [Aliases("cock", "hen", "chick", "coc", "cc")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]

    public partial class ChickenModule : TheGodfatherServiceModule<ChannelEventService>
    {

        public ChickenModule(ChannelEventService service, DbContextBuilder db)
            : base(service, db)
        {

        }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("User.")] DiscordMember member = null)
            => this.InfoAsync(ctx, member);


        #region COMMAND_CHICKEN_FIGHT
        [Command("fight"), Priority(1)]
        [Description("Make your chicken and another user's chicken fight eachother!")]
        [Aliases("f", "duel", "attack")]
        public async Task FightAsync(CommandContext ctx,
                                    [Description("Member whose chicken to fight.")] DiscordMember member)
        {
            if (this.Service.IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar _))
                throw new CommandFailedException("There is a chicken war running in this channel. No fights are allowed before the war finishes.");

            if (member.Id == ctx.User.Id)
                throw new CommandFailedException("You can't fight against your own chicken!");

            Chicken? chicken1 = await ChickenOperations.FindAsync(ctx.Client, this.Database, ctx.Guild.Id, ctx.User.Id);
            if (chicken1 is null)
                throw new CommandFailedException("You do not own a chicken!");

            if (chicken1.Stats.TotalVitality < 25)
                throw new CommandFailedException($"{ctx.User.Mention}, your chicken is too weak to start a fight with another chicken! Heal it using {Formatter.InlineCode("chicken heal")} command.");

            Chicken? chicken2 = await ChickenOperations.FindAsync(ctx.Client, this.Database, ctx.Guild.Id, member.Id);
            if (chicken2 is null)
                throw new CommandFailedException("The specified user does not own a chicken!");

            if (Math.Abs(chicken1.Stats.TotalStrength - chicken2.Stats.TotalStrength) > 75)
                throw new CommandFailedException("The strength difference is too big (75 max)! Please find a more suitable opponent.");

            string header = $"{Formatter.Bold(chicken1.Name)} ({chicken1.Stats.ToShortString()}) {Emojis.DuelSwords} {Formatter.Bold(chicken2.Name)} ({chicken2.Stats.ToShortString()}) {Emojis.Chicken}\n\n";

            Chicken winner = chicken1.Fight(chicken2);
            winner.Owner = winner.UserId == ctx.User.Id ? ctx.User : member;
            Chicken loser = winner == chicken1 ? chicken2 : chicken1;
            int gain = winner.DetermineStrengthGain(loser);
            winner.Stats.BareStrength += gain;
            winner.Stats.BareVitality -= gain;
            if (winner.Stats.TotalVitality == 0)
                winner.Stats.BareVitality++;
            loser.Stats.BareVitality -= 50;

            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                db.Chickens.Update(winner);
                if (loser.Stats.TotalVitality > 0)
                    db.Chickens.Update(loser);
                else
                    db.Chickens.Remove(loser);
                await db.ModifyBankAccountAsync(ctx.User.Id, ctx.Guild.Id, v => v + gain * 200);
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, Emojis.Chicken,
                header +
                $"{Emojis.Trophy} Winner: {Formatter.Bold(winner.Name)}\n\n" +
                $"{Formatter.Bold(winner.Name)} gained {Formatter.Bold(gain.ToString())} strength!\n" +
                (loser.Stats.TotalVitality > 0 ? $"{Formatter.Bold(loser.Name)} lost {Formatter.Bold("50")} HP!" : $"{Formatter.Bold(loser.Name)} died in the battle!") +
                $"\n\n{winner.Owner.Mention} won {gain * 200} {ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency}."
                , important: true
            );
        }

        [Command("fight"), Priority(0)]
        public async Task FightAsync(CommandContext ctx,
                                    [Description("Name of the chicken to fight.")] string chickenName)
        {
            Chicken? chicken = await ChickenOperations.FindAsync(ctx.Client, this.Database, ctx.Guild.Id, chickenName);
            if (chicken is null)
                throw new CommandFailedException("Couldn't find any chickens with that name!");

            try {
                await this.FightAsync(ctx, await ctx.Guild.GetMemberAsync(chicken.UserId));
            } catch (NotFoundException) {
                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    db.Chickens.Remove(chicken);
                    await db.SaveChangesAsync();
                }
                throw new CommandFailedException("The user whose chicken you tried to fight is not currently in this guild. The chicken has been put to sleep.");
            }
        }
        #endregion

        #region COMMAND_CHICKEN_FLU
        [Command("flu"), UsesInteractivity]
        [Description("Pay a well-known scientist to create a disease that disintegrates weak chickens.")]
        [Aliases("cancer", "disease", "blackdeath")]
        public async Task FluAsync(CommandContext ctx)
        {
            if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar _))
                throw new CommandFailedException("There is a chicken war running in this channel. No actions are allowed before the war finishes.");

            CachedGuildConfig gcfg = ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);

            if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention}, are you sure you want to pay {Formatter.Bold("1,000,000")} {gcfg.Currency} to create a disease?"))
                return;

            short threshold = (short)GFRandom.Generator.Next(50, 100);
            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                if (!await db.TryDecreaseBankAccountAsync(ctx.User.Id, ctx.Guild.Id, 1000000))
                    throw new CommandFailedException($"You do not have enough {gcfg.Currency} to pay for the disease creation!");
                db.Chickens.RemoveRange(db.Chickens.Where(c => c.Vitality < threshold));
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, Emojis.Chicken, $"The deadly chicken flu killed all chickens with vitality less than or equal to {Formatter.Bold(threshold.ToString())}!");
        }
        #endregion

        #region COMMAND_CHICKEN_HEAL
        [Command("heal")]
        [Description("Heal your chicken (+100 HP). There is one medicine made each 5 minutes, so you need to grab it before the others do!")]
        [Aliases("+hp", "hp")]
        [Cooldown(1, 300, CooldownBucketType.Guild)]
        public async Task HealAsync(CommandContext ctx)
        {
            if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar _))
                throw new CommandFailedException("There is a chicken war running in this channel. You are not allowed to heal your chicken before the war finishes.");

            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                Chicken? chicken = await ChickenOperations.FindAsync(ctx.Client, this.Database, ctx.Guild.Id, ctx.User.Id, findOwner: false);
                if (chicken is null)
                    throw new CommandFailedException("You do not own a chicken in this guild!");
                chicken.Vitality = (chicken.Vitality + 100) > chicken.BareMaxVitality ? chicken.BareMaxVitality : (chicken.Vitality + 100);
                db.Chickens.Update(chicken);
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, Emojis.Chicken, $"{ctx.User.Mention} healed his chicken (+100 to current HP)!");
        }
        #endregion

        #region COMMAND_CHICKEN_INFO
        [Command("info")]
        [Description("View user's chicken info. If the user is not given, views sender's chicken info.")]
        [Aliases("information", "stats")]

        public async Task InfoAsync(CommandContext ctx,
                                   [Description("User.")] DiscordMember member = null)
        {
            member = member ?? ctx.Member;

            CachedGuildConfig gcfg = ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
            Chicken? chicken = await ChickenOperations.FindAsync(ctx.Client, this.Database, ctx.Guild.Id, member.Id, findOwner: false);
            if (chicken is null)
                throw new CommandFailedException($"User {member.Mention} does not own a chicken in this guild! Use command {Formatter.InlineCode("chicken buy")} to buy a chicken (1000 {gcfg.Currency}).");
            chicken.Owner = member;

            await ctx.RespondAsync(embed: chicken.ToDiscordEmbed());
        }
        #endregion

        #region COMMAND_CHICKEN_RENAME
        [Command("rename")]
        [Description("Rename your chicken.")]
        [Aliases("rn", "name")]

        public async Task RenameAsync(CommandContext ctx,
                                     [RemainingText, Description("New chicken name.")] string newname)
        {
            if (string.IsNullOrWhiteSpace(newname))
                throw new InvalidCommandUsageException("New name for your chicken is missing.");

            if (newname.Length < 2 || newname.Length > 30)
                throw new InvalidCommandUsageException("Name cannot be shorter than 2 and longer than 30 characters.");

            if (!newname.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))
                throw new InvalidCommandUsageException("Name cannot contain characters that are not letters or digits.");

            if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar _))
                throw new CommandFailedException("There is a chicken war running in this channel. No renames are allowed before the war finishes.");

            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                Chicken chicken = db.Chickens.FirstOrDefault(c => c.GuildIdDb == (long)ctx.Guild.Id && c.UserIdDb == (long)ctx.User.Id);
                if (chicken is null)
                    throw new CommandFailedException("You do not own a chicken in this guild!");
                chicken.Name = newname;
                db.Update(chicken);
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, Emojis.Chicken, $"{ctx.User.Mention} renamed his chicken to {Formatter.Bold(newname)}");
        }
        #endregion

        #region COMMAND_CHICKEN_SELL
        [Command("sell"), UsesInteractivity]
        [Description("Sell your chicken.")]
        [Aliases("s")]
        public async Task SellAsync(CommandContext ctx)
        {
            if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar _))
                throw new CommandFailedException("There is a chicken war running in this channel. No sells are allowed before the war finishes.");

            Chicken? chicken = await ChickenOperations.FindAsync(ctx.Client, this.Database, ctx.Guild.Id, ctx.User.Id, findOwner: false);
            if (chicken is null)
                throw new CommandFailedException("You do not own a chicken!");
            chicken.Owner = ctx.User;

            CachedGuildConfig gcfg = ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
            long price = chicken.SellPrice;
            if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention}, are you sure you want to sell your chicken for {Formatter.Bold($"{price:n0}")} {gcfg.Currency}?"))
                return;

            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                await db.ModifyBankAccountAsync(ctx.User.Id, ctx.Guild.Id, v => v + price);
                db.Chickens.Remove(chicken);
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, Emojis.Chicken, $"{ctx.User.Mention} sold {Formatter.Bold(chicken.Name)} for {Formatter.Bold($"{price:n0}")} {gcfg.Currency}!");
        }
        #endregion

        #region COMMAND_CHICKEN_TOP
        [Command("top")]
        [Description("View the list of strongest chickens in the current guild.")]
        [Aliases("best", "strongest")]
        public async Task TopAsync(CommandContext ctx)
        {
            List<Chicken> chickens;
            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                chickens = await db.Chickens
                    .Where(c => c.GuildIdDb == (long)ctx.Guild.Id)
                    .Include(c => c.Upgrades)
                        .ThenInclude(u => u.Upgrade)
                    .OrderBy(c => c.Stats.TotalStrength)
                    .ToListAsync();
            }

            foreach (Chicken chicken in chickens)
                await chicken.SetOwnerAsync(ctx.Client, this.Database);

            if (!chickens.Any())
                throw new CommandFailedException("No chickens bought in this guild.");

            await ctx.SendCollectionInPagesAsync(
                "Strongest chickens in this guild:",
                chickens.OrderByDescending(c => c.Stats.TotalStrength),
                c => $"{Formatter.Bold(c.Name)} | {c.Owner?.Mention ?? "unknown owner (removed)"} | {c.Stats.TotalStrength} ({c.Stats.BareStrength}) STR",
                this.ModuleColor
            );
        }
        #endregion

        #region COMMAND_CHICKEN_TOPGLOBAL
        [Command("topglobal")]
        [Description("View the list of strongest chickens globally.")]
        [Aliases("bestglobally", "globallystrongest", "globaltop", "topg", "gtop")]
        public async Task GlobalTopAsync(CommandContext ctx)
        {
            List<Chicken> chickens;
            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                chickens = await db.Chickens
                    .Include(c => c.Upgrades)
                        .ThenInclude(u => u.Upgrade)
                    .OrderBy(c => c.Stats.TotalStrength)
                    .ToListAsync();
            }

            foreach (Chicken chicken in chickens)
                await chicken.SetOwnerAsync(ctx.Client, this.Database);

            if (!chickens.Any())
                throw new CommandFailedException("No chickens bought.");

            await ctx.SendCollectionInPagesAsync(
                "Strongest chickens globally:",
                chickens.OrderByDescending(c => c.Stats.TotalStrength),
                c => $"{Formatter.Bold(c.Name)} | {c.Owner?.Mention ?? "unknown owner (removed)"} | {c.Stats.TotalStrength} ({c.Stats.BareStrength}) STR",
                this.ModuleColor
            );
        }
        #endregion
    }
}
