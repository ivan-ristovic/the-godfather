#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Currency.Common;
using TheGodfather.Services;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Currency
{
    [Group("chicken"), Module(ModuleType.Currency)]
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


        #region GROUP_CHICKEN_BUY
        [Group("buy"), Module(ModuleType.Currency)]
        [Description("Buy a new chicken in this guild using your credits from WM bank.")]
        [Aliases("b", "shop")]
        [UsageExample("!chicken buy My Chicken Name")]
        public class BuyChicken : TheGodfatherBaseModule
        {

            public BuyChicken(DBService db) : base(db: db) { }


            [GroupCommand]
            public Task BuyAsync(CommandContext ctx,
                                [RemainingText, Description("Chicken name.")] string name = null)
                => HandleBuyAsync(ctx, ChickenType.Default, name);


            #region COMMAND_CHICKEN_BUY_DEFAULT
            [Command("default"), Module(ModuleType.Currency)]
            [Description("Buy a chicken of default strength (cheapest).")]
            [Aliases("d", "def")]
            [UsageExample("!chicken buy default My Chicken Name")]
            public Task DefaultAsync(CommandContext ctx,
                                    [RemainingText, Description("Chicken name.")] string name = null)
                => HandleBuyAsync(ctx, ChickenType.Default, name);
            #endregion

            #region COMMAND_CHICKEN_BUY_WELLFED
            [Command("wellfed"), Module(ModuleType.Currency)]
            [Description("Buy a well-fed chicken.")]
            [Aliases("wf", "fed")]
            [UsageExample("!chicken buy wellfed My Chicken Name")]
            public Task WellFedAsync(CommandContext ctx,
                                    [RemainingText, Description("Chicken name.")] string name = null)
                => HandleBuyAsync(ctx, ChickenType.WellFed, name);
            #endregion

            #region COMMAND_CHICKEN_BUY_TRAINED
            [Command("trained"), Module(ModuleType.Currency)]
            [Description("Buy a trained chicken.")]
            [Aliases("wf", "fed")]
            [UsageExample("!chicken buy trained My Chicken Name")]
            public Task TrainedAsync(CommandContext ctx,
                                    [RemainingText, Description("Chicken name.")] string name = null)
                => HandleBuyAsync(ctx, ChickenType.Trained, name);
            #endregion

            #region COMMAND_CHICKEN_BUY_EMPOWERED
            [Command("steroidempowered"), Module(ModuleType.Currency)]
            [Description("Buy a steroid-empowered chicken.")]
            [Aliases("steroid", "empowered")]
            [UsageExample("!chicken buy steroidempowered My Chicken Name")]
            public Task EmpoweredAsync(CommandContext ctx,
                                      [RemainingText, Description("Chicken name.")] string name = null)
                => HandleBuyAsync(ctx, ChickenType.SteroidEmpowered, name);
            #endregion

            #region COMMAND_CHICKEN_BUY_ALIEN
            [Command("alien"), Module(ModuleType.Currency)]
            [Description("Buy an alien chicken.")]
            [Aliases("a", "extraterrestrial")]
            [UsageExample("!chicken buy alien My Chicken Name")]
            public Task AlienAsync(CommandContext ctx,
                                  [RemainingText, Description("Chicken name.")] string name = null)
                => HandleBuyAsync(ctx, ChickenType.Alien, name);
            #endregion

            #region COMMAND_CHICKEN_BUY_LIST
            [Command("list"), Module(ModuleType.Currency)]
            [Description("List all available chicken types.")]
            [Aliases("ls", "view")]
            [UsageExample("!chicken buy list")]
            public async Task ListAsync(CommandContext ctx)
            {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Available chicken types",
                    Color = DiscordColor.Orange
                };

                emb.AddField("Default", $"STR: {Formatter.Bold(Chicken.StartingStrength[ChickenType.Default].ToString())}\nPrice: {Formatter.Bold(Chicken.Price[ChickenType.Default].ToString())}", inline: true);
                emb.AddField("Well-Fed", $"STR: {Formatter.Bold(Chicken.StartingStrength[ChickenType.WellFed].ToString())}\nPrice: {Formatter.Bold(Chicken.Price[ChickenType.WellFed].ToString())}", inline: true);
                emb.AddField("Trained", $"STR: {Formatter.Bold(Chicken.StartingStrength[ChickenType.Trained].ToString())}\nPrice: {Formatter.Bold(Chicken.Price[ChickenType.Trained].ToString())}", inline: true);
                emb.AddField("Steroid Empowered", $"STR: {Formatter.Bold(Chicken.StartingStrength[ChickenType.SteroidEmpowered].ToString())}\nPrice: {Formatter.Bold(Chicken.Price[ChickenType.SteroidEmpowered].ToString())}", inline: true);
                emb.AddField("Alien", $"STR: {Formatter.Bold(Chicken.StartingStrength[ChickenType.Alien].ToString())}\nPrice: {Formatter.Bold(Chicken.Price[ChickenType.Alien].ToString())}", inline: true);

                await ctx.RespondAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
            #endregion


            #region HELPER_FUNCTIONS
            private async Task HandleBuyAsync(CommandContext ctx, ChickenType type, string name = null)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidCommandUsageException("Name for your chicken is missing.");

                if (name.Length < 2 || name.Length > 30)
                    throw new InvalidCommandUsageException("Name cannot be shorter than 2 and longer than 30 characters.");

                if (!name.All(c => Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c)))
                    throw new InvalidCommandUsageException("Name cannot contain characters that are not letters or digits.");

                if (await Database.GetChickenInfoAsync(ctx.User.Id, ctx.Guild.Id).ConfigureAwait(false) != null)
                    throw new CommandFailedException("You already own a chicken!");
                
                if (!await ctx.AskYesNoQuestionAsync($"Are you sure you want to buy a chicken for {Formatter.Bold(Chicken.Price[type].ToString())} credits?"))
                    return;

                if (!await Database.TakeCreditsFromUserAsync(ctx.User.Id, ctx.Guild.Id, Chicken.Price[type]).ConfigureAwait(false))
                    throw new CommandFailedException($"You do not have enought credits to buy a chicken ({Chicken.Price[type]} needed)!");

                await Database.BuyChickenAsync(ctx.User.Id, ctx.Guild.Id, name, Chicken.StartingStrength[type])
                    .ConfigureAwait(false);

                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken, $"{ctx.User.Mention} bought a chicken named {Formatter.Bold(name)}")
                    .ConfigureAwait(false);
            }
            #endregion
        }
        #endregion


        #region COMMAND_CHICKEN_FIGHT
        [Command("fight"), Module(ModuleType.Currency)]
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

            if (Math.Abs(chicken1.Strength - chicken2.Strength) > 50)
                throw new CommandFailedException("The strength difference is too big (50 max)! Please find a more suitable opponent.");

            if (!ctx.Guild.Members.Any(m => m.Id == chicken2.OwnerId))
                throw new CommandFailedException("The owner of that chicken is not a member of this guild so you cannot fight his chicken.");

            string header = $"{Formatter.Bold(chicken1.Name)} ({chicken1.Strength}) {StaticDiscordEmoji.DuelSwords} {Formatter.Bold(chicken2.Name)} ({chicken2.Strength}) {StaticDiscordEmoji.Chicken}\n\n";

            var winner = chicken1.Fight(chicken2);
            winner.Owner = winner.OwnerId == ctx.User.Id ? ctx.User : user;
            var loser = winner == chicken1 ? chicken2 : chicken1;
            short gain = Chicken.DetermineGain(winner.Strength, loser.Strength);
            winner.Strength += gain;

            await Database.ModifyChickenAsync(winner, ctx.Guild.Id)
                .ConfigureAwait(false);
            await Database.RemoveChickenAsync(loser.OwnerId, ctx.Guild.Id)
                .ConfigureAwait(false);
            await Database.GiveCreditsToUserAsync(winner.OwnerId, ctx.Guild.Id, gain * 200)
                .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken,
                header +
                $"{StaticDiscordEmoji.Trophy} Winner: {Formatter.Bold(winner.Name)}\n\n" +
                $"{Formatter.Bold(winner.Name)} gained {Formatter.Bold(gain.ToString())} strength!\n\n" +
                $"{Formatter.Bold(loser.Name)} died in the battle!\n\n" +
                $"{winner.Owner.Mention} won {gain * 200} credits."
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHICKEN_INFO
        [Command("info"), Module(ModuleType.Currency)]
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
        [Command("rename"), Module(ModuleType.Currency)]
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
        [Command("sell"), Module(ModuleType.Currency)]
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
            if (!await ctx.AskYesNoQuestionAsync($"Are you sure you want to sell your chicken for {Formatter.Bold(price.ToString())} credits?"))
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
        [Command("top"), Module(ModuleType.Currency)]
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
                c => $"{Formatter.Bold(c.Name)} | {c.Owner.Mention} | {c.Strength} STR"
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHICKEN_TRAIN
        [Command("train"), Module(ModuleType.Currency)]
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
            if (!await ctx.AskYesNoQuestionAsync($"Are you sure you want to train your chicken for {Formatter.Bold(price.ToString())} credits?"))
                return;

            if (!await Database.TakeCreditsFromUserAsync(ctx.User.Id, ctx.Guild.Id, price).ConfigureAwait(false))
                throw new CommandFailedException($"You do not have enought credits to train a chicken ({chicken.TrainPrice} needed)!");

            string result;
            if (chicken.Train()) 
                result = $"Your chicken learned alot from the training. New strength: {chicken.Strength}";
            else 
                result = $"Your chicken got tired and didn't learn anything. New strength: {chicken.Strength}";

            await Database.ModifyChickenAsync(chicken, ctx.Guild.Id)
                .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken, result)
                .ConfigureAwait(false);
        }
        #endregion
    }
}
