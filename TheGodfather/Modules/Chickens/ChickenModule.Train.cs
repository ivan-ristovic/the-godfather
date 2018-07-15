#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services.Database.Bank;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Chickens
{
    public partial class ChickenModule
    {
        [Group("train"), Module(ModuleType.Chickens)]
        [Description("Train your chicken using your credits from WM bank.")]
        [Aliases("tr", "t", "exercise")]
        [UsageExamples("!chicken train")]
        [UsesInteractivity]
        public class TrainModule : TheGodfatherBaseModule
        {

            public TrainModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => StrengthAsync(ctx);


            #region COMMAND_CHICKEN_TRAIN_STRENGTH
            [Command("strength"), Module(ModuleType.Chickens)]
            [Description("Train your chicken's strength using your credits from WM bank.")]
            [Aliases("str", "st", "s")]
            [UsageExamples("!chicken train strength")]
            public async Task StrengthAsync(CommandContext ctx)
            {
                var chicken = await Database.GetChickenInfoAsync(ctx.User.Id, ctx.Guild.Id)
                    .ConfigureAwait(false);
                if (chicken == null)
                    throw new CommandFailedException("You do not own a chicken!");

                if (ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenWar)
                    throw new CommandFailedException("There is a chicken war running in this channel. No trainings are allowed before the war finishes.");

                if (chicken.Stats.TotalVitality < 25)
                    throw new CommandFailedException($"{ctx.User.Mention}, your chicken is too weak for that action! Heal it using {Formatter.BlockCode("chicken heal")} command.");

                var price = chicken.TrainStrengthPrice;
                if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention}, are you sure you want to train your chicken for {Formatter.Bold(price.ToString())} credits?\n\nNote: This action will also weaken the vitality of your chicken by 1."))
                    return;

                if (!await Database.DecreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, price).ConfigureAwait(false))
                    throw new CommandFailedException($"You do not have enought credits to train a chicken ({price} needed)!");

                string result;
                if (chicken.TrainStrength())
                    result = $"{ctx.User.Mention}'s chicken learned alot from the training. New strength: {chicken.Stats.TotalStrength}";
                else
                    result = $"{ctx.User.Mention}'s chicken got tired and didn't learn anything. New strength: {chicken.Stats.TotalStrength}";
                chicken.Stats.BareVitality--;

                await Database.ModifyChickenAsync(chicken, ctx.Guild.Id)
                    .ConfigureAwait(false);

                await ctx.InformSuccessAsync(StaticDiscordEmoji.Chicken, result)
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_CHICKEN_TRAIN_VITALITY
            [Command("vitality"), Module(ModuleType.Chickens)]
            [Description("Train your chicken's vitality using your credits from WM bank.")]
            [Aliases("vit", "vi", "v")]
            [UsageExamples("!chicken train vitality")]
            public async Task VitalityAsync(CommandContext ctx)
            {
                var chicken = await Database.GetChickenInfoAsync(ctx.User.Id, ctx.Guild.Id)
                    .ConfigureAwait(false);
                if (chicken == null)
                    throw new CommandFailedException("You do not own a chicken!");

                if (ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenWar)
                    throw new CommandFailedException("There is a chicken war running in this channel. No trainings are allowed before the war finishes.");

                if (chicken.Stats.TotalVitality < 25)
                    throw new CommandFailedException($"{ctx.User.Mention}, your chicken is too weak for that action! Heal it using {Formatter.BlockCode("chicken heal")} command.");

                var price = chicken.TrainVitalityPrice;
                if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention}, are you sure you want to train your chicken for {Formatter.Bold(price.ToString())} credits?\n\nNote: This action will also weaken the vitality of your chicken by 1."))
                    return;

                if (!await Database.DecreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, price).ConfigureAwait(false))
                    throw new CommandFailedException($"You do not have enought credits to train a chicken ({price} needed)!");

                string result;
                if (chicken.TrainVitality())
                    result = $"{ctx.User.Mention}'s chicken learned alot from the training. New max vitality: {chicken.Stats.TotalMaxVitality}";
                else
                    result = $"{ctx.User.Mention}'s chicken got tired and didn't learn anything. New max vitality: {chicken.Stats.TotalMaxVitality}";
                chicken.Stats.BareVitality--;

                await Database.ModifyChickenAsync(chicken, ctx.Guild.Id)
                    .ConfigureAwait(false);

                await ctx.InformSuccessAsync(StaticDiscordEmoji.Chicken, result)
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
