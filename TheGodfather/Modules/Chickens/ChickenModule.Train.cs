using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Modules.Chickens.Services;
using TheGodfather.Modules.Currency.Services;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Chickens
{
    public partial class ChickenModule
    {
        [Group("train"), UsesInteractivity]
        [Aliases("tr", "t", "exercise")]
        public sealed class TrainModule : TheGodfatherServiceModule<ChickenService>
        {
            #region chicken train
            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => this.StrengthAsync(ctx);
            #endregion

            #region chicken train strength
            [Command("strength")]
            [Aliases("str", "st", "s")]
            public async Task StrengthAsync(CommandContext ctx)
            {
                Chicken? chicken = await this.PreTrainCheckAsync(ctx, "STR");
                if (chicken is null)
                    return;

                bool success = chicken.TrainStrength();
                chicken.Stats.BareVitality--;

                await this.Service.UpdateAsync(chicken);
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Chicken, 
                    success ? "fmt-chicken-train-succ" : "fmt-chicken-train-fail", ctx.User.Mention, "STR", chicken.Stats.TotalStrength
                );
            }
            #endregion

            #region chicken train vitality
            [Command("vitality")]
            [Aliases("vit", "vi", "v")]
            public async Task VitalityAsync(CommandContext ctx)
            {
                Chicken? chicken = await this.PreTrainCheckAsync(ctx, "VIT");
                if (chicken is null)
                    return;

                bool success = chicken.TrainVitality();
                chicken.Stats.BareVitality--;

                await this.Service.UpdateAsync(chicken);
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Chicken,
                    success ? "fmt-chicken-train-succ" : "fmt-chicken-train-fail", ctx.User.Mention, "VIT", chicken.Stats.TotalMaxVitality
                );
            }
            #endregion


            #region internals
            private async Task<Chicken?> PreTrainCheckAsync(CommandContext ctx, string stat)
            {
                if (ctx.Services.GetRequiredService<ChannelEventService>().IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar _))
                    throw new CommandFailedException(ctx, "cmd-err-chicken-war");

                Chicken? chicken = await this.Service.GetCompleteAsync(ctx.Guild.Id, ctx.User.Id);
                if (chicken is null)
                    throw new CommandFailedException(ctx, "cmd-err-chicken-none");
                chicken.Owner = ctx.User;

                if (chicken.Stats.TotalVitality < Chicken.MinVitalityToFight)
                    throw new CommandFailedException(ctx, "cmd-err-chicken-weak", ctx.User.Mention);

                CachedGuildConfig gcfg = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
                long price = stat switch {
                    "STR" => chicken.TrainStrengthPrice,
                    "VIT" => chicken.TrainVitalityPrice,
                    _ => throw new CommandFailedException(ctx),
                };

                if (!await ctx.WaitForBoolReplyAsync("q-chicken-train", args: new object[] { ctx.User.Mention, stat, price, gcfg.Currency }))
                    return null;

                if (!await ctx.Services.GetRequiredService<BankAccountService>().TryDecreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, price))
                    throw new CommandFailedException(ctx, "cmd-err-funds", gcfg.Currency, price);

                return chicken;
            }
            #endregion
        }
    }
}
