#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Modules.Chickens.Common;
#endregion

namespace TheGodfather.Modules.Chickens
{
    public partial class ChickenModule
    {
        [Group("train"), Module(ModuleType.Chickens)]
        [Description("Train your chicken using your credits from WM bank.")]
        [Aliases("tr", "t", "exercise")]
        [UsageExample("!chicken train")]
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
            [UsageExample("!chicken train strength")]
            public async Task StrengthAsync(CommandContext ctx)
            {
                var chicken = await Database.GetChickenInfoAsync(ctx.User.Id, ctx.Guild.Id)
                    .ConfigureAwait(false);
                if (chicken == null)
                    throw new CommandFailedException("You do not own a chicken!");

                if (ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenWar)
                    throw new CommandFailedException("There is a chicken war running in this channel. No trainings are allowed before the war finishes.");

                var price = chicken.TrainPrice;
                if (!await ctx.AskYesNoQuestionAsync($"{ctx.User.Mention}, are you sure you want to train your chicken for {Formatter.Bold(price.ToString())} credits?"))
                    return;

                if (!await Database.TakeCreditsFromUserAsync(ctx.User.Id, ctx.Guild.Id, price).ConfigureAwait(false))
                    throw new CommandFailedException($"You do not have enought credits to train a chicken ({chicken.TrainPrice} needed)!");

                string result;
                if (chicken.TrainStrength())
                    result = $"{ctx.User.Mention}'s chicken learned alot from the training. New strength: {chicken.Stats.Strength}";
                else
                    result = $"{ctx.User.Mention}'s chicken got tired and didn't learn anything. New strength: {chicken.Stats.Strength}";

                await Database.ModifyChickenAsync(chicken, ctx.Guild.Id)
                    .ConfigureAwait(false);

                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken, result)
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_CHICKEN_TRAIN_VITALITY
            [Command("vitelity"), Module(ModuleType.Chickens)]
            [Description("Train your chicken's vitality using your credits from WM bank.")]
            [Aliases("vit", "vi", "v")]
            [UsageExample("!chicken train vitality")]
            public async Task VitalityAsync(CommandContext ctx)
            {
                var chicken = await Database.GetChickenInfoAsync(ctx.User.Id, ctx.Guild.Id)
                    .ConfigureAwait(false);
                if (chicken == null)
                    throw new CommandFailedException("You do not own a chicken!");

                if (ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenWar)
                    throw new CommandFailedException("There is a chicken war running in this channel. No trainings are allowed before the war finishes.");

                var price = chicken.TrainPrice;
                if (!await ctx.AskYesNoQuestionAsync($"{ctx.User.Mention}, are you sure you want to train your chicken for {Formatter.Bold(price.ToString())} credits?"))
                    return;

                if (!await Database.TakeCreditsFromUserAsync(ctx.User.Id, ctx.Guild.Id, price).ConfigureAwait(false))
                    throw new CommandFailedException($"You do not have enought credits to train a chicken ({chicken.TrainPrice} needed)!");

                string result;
                if (chicken.TrainVitality())
                    result = $"{ctx.User.Mention}'s chicken learned alot from the training. New max vitality: {chicken.Stats.MaxVitality}";
                else
                    result = $"{ctx.User.Mention}'s chicken got tired and didn't learn anything. New max vitality: {chicken.Stats.MaxVitality}";

                await Database.ModifyChickenAsync(chicken, ctx.Guild.Id)
                    .ConfigureAwait(false);

                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken, result)
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
