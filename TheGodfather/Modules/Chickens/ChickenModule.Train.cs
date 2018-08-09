#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

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
        [Group("train"), UsesInteractivity]
        [Description("Train your chicken using your credits from WM bank.")]
        [Aliases("tr", "t", "exercise")]
        [UsageExamples("!chicken train")]
        public class TrainModule : TheGodfatherModule
        {

            public TrainModule(SharedData shared, DBService db) 
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Yellow;
            }


            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => StrengthAsync(ctx);


            #region COMMAND_CHICKEN_TRAIN_STRENGTH
            [Command("strength")]
            [Description("Train your chicken's strength using your credits from WM bank.")]
            [Aliases("str", "st", "s")]
            [UsageExamples("!chicken train strength")]
            public async Task StrengthAsync(CommandContext ctx)
            {
                Chicken chicken = await this.Database.GetChickenAsync(ctx.User.Id, ctx.Guild.Id);
                if (chicken == null)
                    throw new CommandFailedException("You do not own a chicken!");

                if (this.Shared.GetEventInChannel(ctx.Channel.Id) is ChickenWar)
                    throw new CommandFailedException("There is a chicken war running in this channel. No trainings are allowed before the war finishes.");

                if (chicken.Stats.TotalVitality < 25)
                    throw new CommandFailedException($"{ctx.User.Mention}, your chicken is too weak for that action! Heal it using {Formatter.BlockCode("chicken heal")} command.");

                long price = chicken.TrainStrengthPrice;
                if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention}, are you sure you want to train your chicken for {Formatter.Bold($"{price:n0}")} {this.Shared.GuildConfigurations[ctx.Guild.Id].Currency ?? "credits"}?\n\nNote: This action will also weaken the vitality of your chicken by 1."))
                    return;

                if (!await this.Database.DecreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, price))
                    throw new CommandFailedException($"You do not have enought {this.Shared.GuildConfigurations[ctx.Guild.Id].Currency ?? "credits"} to train a chicken ({price:n0} needed)!");

                string result;
                if (chicken.TrainStrength())
                    result = $"{ctx.User.Mention}'s chicken learned alot from the training. New strength: {chicken.Stats.TotalStrength}";
                else
                    result = $"{ctx.User.Mention}'s chicken got tired and didn't learn anything. New strength: {chicken.Stats.TotalStrength}";
                chicken.Stats.BareVitality--;

                await this.Database.ModifyChickenAsync(chicken, ctx.Guild.Id);
                await InformAsync(ctx, StaticDiscordEmoji.Chicken, result);
            }
            #endregion

            #region COMMAND_CHICKEN_TRAIN_VITALITY
            [Command("vitality")]
            [Description("Train your chicken's vitality using your credits from WM bank.")]
            [Aliases("vit", "vi", "v")]
            [UsageExamples("!chicken train vitality")]
            public async Task VitalityAsync(CommandContext ctx)
            {
                Chicken chicken = await this.Database.GetChickenAsync(ctx.User.Id, ctx.Guild.Id);
                if (chicken == null)
                    throw new CommandFailedException("You do not own a chicken!");

                if (this.Shared.GetEventInChannel(ctx.Channel.Id) is ChickenWar)
                    throw new CommandFailedException("There is a chicken war running in this channel. No trainings are allowed before the war finishes.");

                if (chicken.Stats.TotalVitality < 25)
                    throw new CommandFailedException($"{ctx.User.Mention}, your chicken is too weak for that action! Heal it using {Formatter.BlockCode("chicken heal")} command.");

                long price = chicken.TrainVitalityPrice;
                if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention}, are you sure you want to train your chicken for {Formatter.Bold($"{price:n0}")} {this.Shared.GuildConfigurations[ctx.Guild.Id].Currency ?? "credits"}?\n\nNote: This action will also weaken the vitality of your chicken by 1."))
                    return;

                if (!await this.Database.DecreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, price))
                    throw new CommandFailedException($"You do not have enought {this.Shared.GuildConfigurations[ctx.Guild.Id].Currency ?? "credits"} to train a chicken ({price:n0} needed)!");

                string result;
                if (chicken.TrainVitality())
                    result = $"{ctx.User.Mention}'s chicken learned alot from the training. New max vitality: {chicken.Stats.TotalMaxVitality}";
                else
                    result = $"{ctx.User.Mention}'s chicken got tired and didn't learn anything. New max vitality: {chicken.Stats.TotalMaxVitality}";
                chicken.Stats.BareVitality--;

                await this.Database.ModifyChickenAsync(chicken, ctx.Guild.Id);
                await InformAsync(ctx, StaticDiscordEmoji.Chicken, result);
            }
            #endregion
        }
    }
}
