#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Chickens
{
    [Group("chicken"), Module(ModuleType.Chickens)]
    [Description("Manage your chicken. If invoked without subcommands, prints out your chicken information.")]
    [Aliases("cock", "hen", "chick", "coc")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [UsageExample("!chicken")]
    [UsageExample("!chicken @Someone")]
    [ListeningCheck]
    public partial class ChickenModule : TheGodfatherBaseModule
    {

        public ChickenModule(DBService db) : base(db: db) { }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("User.")] DiscordUser user = null)
            => InfoAsync(ctx, user);


        #region COMMAND_CHICKEN_FIGHT
        [Command("fight"), Module(ModuleType.Chickens)]
        [Description("Make your chicken and another user's chicken fight until death!")]
        [Aliases("f", "duel", "attack")]
        [UsageExample("!chicken duel @Someone")]
        public async Task TrainAsync(CommandContext ctx,
                                    [Description("User.")] DiscordUser user)
        {
            if (ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenWar ambush)
                throw new CommandFailedException("There is a chicken war running in this channel. No fights are allowed before the war finishes.");

            if (user.Id == ctx.User.Id)
                throw new CommandFailedException("You can't fight against your own chicken!");

            var chicken1 = await Database.GetChickenInfoAsync(ctx.User.Id, ctx.Guild.Id)
                .ConfigureAwait(false);
            var chicken2 = await Database.GetChickenInfoAsync(user.Id, ctx.Guild.Id)
                .ConfigureAwait(false);
            if (chicken1 == null || chicken2 == null)
                throw new CommandFailedException("One of you does not own a chicken!");

            if (Math.Abs(chicken1.Stats.Strength - chicken2.Stats.Strength) > 50)
                throw new CommandFailedException("The strength difference is too big (50 max)! Please find a more suitable opponent.");

            if (!ctx.Guild.Members.Any(m => m.Id == chicken2.OwnerId))
                throw new CommandFailedException("The owner of that chicken is not a member of this guild so you cannot fight his chicken.");

            string header = $"{Formatter.Bold(chicken1.Name)} ({chicken1.Stats.ToString()}) {StaticDiscordEmoji.DuelSwords} {Formatter.Bold(chicken2.Name)} ({chicken2.Stats.ToString()}) {StaticDiscordEmoji.Chicken}\n\n";

            var winner = chicken1.Fight(chicken2);
            winner.Owner = winner.OwnerId == ctx.User.Id ? ctx.User : user;
            var loser = winner == chicken1 ? chicken2 : chicken1;
            short gain = winner.DetermineStrengthGain(loser);
            winner.Stats.Strength += gain;
            winner.Stats.Vitality -= gain;
            if (winner.Stats.Vitality == 0)
                winner.Stats.Vitality = 1;
            loser.Stats.Vitality -= 50;

            await Database.ModifyChickenAsync(winner, ctx.Guild.Id)
                .ConfigureAwait(false);
            if (loser.Stats.Vitality > 0)
                await Database.ModifyChickenAsync(loser, ctx.Guild.Id).ConfigureAwait(false);
            else
                await Database.RemoveChickenAsync(loser.OwnerId, ctx.Guild.Id).ConfigureAwait(false);

            await Database.GiveCreditsToUserAsync(winner.OwnerId, ctx.Guild.Id, gain * 2000)
                .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken,
                header +
                $"{StaticDiscordEmoji.Trophy} Winner: {Formatter.Bold(winner.Name)}\n\n" +
                $"{Formatter.Bold(winner.Name)} gained {Formatter.Bold(gain.ToString())} strength!\n\n" +
                (loser.Stats.Vitality == 0 ? $"{Formatter.Bold(loser.Name)} died in the battle!\n\n" : "") +
                $"{winner.Owner.Mention} won {gain * 200} credits."
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHICKEN_INFO
        [Command("info"), Module(ModuleType.Chickens)]
        [Description("View user's chicken info. If the user is not given, views sender's chicken info.")]
        [Aliases("information", "stats")]
        [UsageExample("!chicken info @Someone")]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description("User.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            var chicken = await Database.GetChickenInfoAsync(user.Id, ctx.Guild.Id)
                .ConfigureAwait(false);
            if (chicken == null)
                throw new CommandFailedException($"User {user.Mention} does not own a chicken in this guild! Use command {Formatter.InlineCode("chicken buy")} to buy a chicken (1000 credits).");

            await ctx.RespondAsync(embed: chicken.Embed(user))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHICKEN_RENAME
        [Command("rename"), Module(ModuleType.Chickens)]
        [Description("Rename your chicken.")]
        [Aliases("rn", "name")]
        [UsageExample("!chicken name New Name")]
        public async Task RenameAsync(CommandContext ctx,
                                     [RemainingText, Description("Chicken name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("New name for your chicken is missing.");

            if (name.Length < 2 || name.Length > 30)
                throw new InvalidCommandUsageException("Name cannot be shorter than 2 and longer than 30 characters.");

            if (!name.All(c => Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c)))
                throw new InvalidCommandUsageException("Name cannot contain characters that are not letters or digits.");

            if (ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenWar ambush)
                throw new CommandFailedException("There is a chicken war running in this channel. No renames are allowed before the war finishes.");

            var chicken = await Database.GetChickenInfoAsync(ctx.User.Id, ctx.Guild.Id)
                .ConfigureAwait(false);
            if (chicken == null)
                throw new CommandFailedException("You do not own a chicken!");

            chicken.Name = name;
            await Database.ModifyChickenAsync(chicken, ctx.Guild.Id)
                .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken, $"{ctx.User.Mention} renamed his chicken to {Formatter.Bold(name)}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHICKEN_SELL
        [Command("sell"), Module(ModuleType.Chickens)]
        [Description("Sell your chicken.")]
        [Aliases("s")]
        [UsageExample("!chicken sell")]
        public async Task SellAsync(CommandContext ctx)
        {
            var chicken = await Database.GetChickenInfoAsync(ctx.User.Id, ctx.Guild.Id)
                .ConfigureAwait(false);
            if (chicken == null)
                throw new CommandFailedException("You do not own a chicken!");

            if (ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenWar ambush)
                throw new CommandFailedException("There is a chicken war running in this channel. No sells are allowed before the war finishes.");

            var price = chicken.SellPrice;
            if (!await ctx.AskYesNoQuestionAsync($"{ctx.User.Mention}, are you sure you want to sell your chicken for {Formatter.Bold(price.ToString())} credits?"))
                return;

            await Database.RemoveChickenAsync(ctx.User.Id, ctx.Guild.Id)
                .ConfigureAwait(false);
            await Database.GiveCreditsToUserAsync(ctx.User.Id, ctx.Guild.Id, price)
                .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken, $"{ctx.User.Mention} sold {Formatter.Bold(chicken.Name)} for {Formatter.Bold(price.ToString())} credits!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHICKEN_TOP
        [Command("top"), Module(ModuleType.Chickens)]
        [Description("View the list of strongest chickens in the current guild.")]
        [Aliases("best", "strongest")]
        [UsageExample("!chicken top")]
        public async Task InfoAsync(CommandContext ctx)
        {
            var chickens = await Database.GetStrongestChickensForGuildAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
            if (chickens == null || !chickens.Any())
                throw new CommandFailedException("No chickens bought in this guild.");

            foreach (var chicken in chickens) {
                try {
                    chicken.Owner = await ctx.Client.GetUserAsync(chicken.OwnerId)
                        .ConfigureAwait(false);
                } catch (Exception e) {
                    TheGodfather.LogHandle.LogException(LogLevel.Warning, e);
                }
            }

            await ctx.SendPaginatedCollectionAsync(
                "Strongest chickens in this guild:",
                chickens,
                c => $"{Formatter.Bold(c.Name)} | {c.Owner.Mention} | {c.Stats.Strength} STR"
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHICKEN_TRAIN
        [Command("train"), Module(ModuleType.Chickens)]
        [Description("Train your chicken using your credits from WM bank.")]
        [Aliases("tr", "t", "exercise")]
        [UsageExample("!chicken train")]
        public async Task TrainAsync(CommandContext ctx)
        {
            var chicken = await Database.GetChickenInfoAsync(ctx.User.Id, ctx.Guild.Id)
                .ConfigureAwait(false);
            if (chicken == null)
                throw new CommandFailedException("You do not own a chicken!");

            if (ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenWar ambush)
                throw new CommandFailedException("There is a chicken war running in this channel. No trainings are allowed before the war finishes.");

            var price = chicken.TrainPrice;
            if (!await ctx.AskYesNoQuestionAsync($"{ctx.User.Mention}, are you sure you want to train your chicken for {Formatter.Bold(price.ToString())} credits?"))
                return;

            if (!await Database.TakeCreditsFromUserAsync(ctx.User.Id, ctx.Guild.Id, price).ConfigureAwait(false))
                throw new CommandFailedException($"You do not have enought credits to train a chicken ({chicken.TrainPrice} needed)!");

            string result;
            if (chicken.Train())
                result = $"{ctx.User.Mention}'s chicken learned alot from the training. New strength: {chicken.Stats.Strength}";
            else
                result = $"{ctx.User.Mention}'s chicken got tired and didn't learn anything. New strength: {chicken.Stats.Strength}";

            await Database.ModifyChickenAsync(chicken, ctx.Guild.Id)
                .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken, result)
                .ConfigureAwait(false);
        }
        #endregion
    }
}
