using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Currency.Services;

namespace TheGodfather.Modules.Currency
{
    [Group("work"), Module(ModuleType.Currency), NotBlocked]
    [Aliases("job")]
    [Cooldown(1, 60, CooldownBucketType.User)]
    [RequireGuild]
    public sealed class WorkModule : TheGodfatherServiceModule<WorkService>
    {
        #region work
        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            string workStr = await this.Service.WorkAsync(ctx.Guild.Id, ctx.User.Id);
            await this.RespondWithWorkString(ctx, workStr, "str-work-footer");
        }
        #endregion

        #region work streets
        [Command("streets")]
        [Aliases("prostitute")]
        [Cooldown(1, 120, CooldownBucketType.User)]
        public async Task StreetsAsync(CommandContext ctx)
        {
            string workStr = await this.Service.StreetsAsync(ctx.Guild.Id, ctx.User.Id);
            await this.RespondWithWorkString(ctx, workStr, "str-work-streets-footer");
        }
        #endregion

        #region work crime
        [Command("crime")]
        [Cooldown(1, 300, CooldownBucketType.User)]
        public async Task CrimeAsync(CommandContext ctx)
        {
            string workStr = await this.Service.StreetsAsync(ctx.Guild.Id, ctx.User.Id);
            await this.RespondWithWorkString(ctx, workStr, "str-work-crime-footer");
        }
        #endregion


        #region internals
        public Task RespondWithWorkString(CommandContext ctx, string str, string footer)
        {
            string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithColor(this.ModuleColor);
                emb.WithDescription($"{ctx.User.Mention} {str} {currency}");
                emb.WithLocalizedFooter(footer, ctx.User.AvatarUrl);
            });
        }
        #endregion
    }
}