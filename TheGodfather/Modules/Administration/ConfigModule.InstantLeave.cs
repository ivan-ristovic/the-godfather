using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration
{
    public sealed partial class ConfigModule
    {
        [Group("instantleave")]
        [Aliases("il")]
        public sealed class AntiInstantLeaveModule : TheGodfatherServiceModule<AntiInstantLeaveService>
        {
            #region config instantleave
            [GroupCommand, Priority(2)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description(TranslationKey.desc_enable)] bool enable,
                                               [Description(TranslationKey.desc_sens)] short cooldown)
            {
                if (cooldown is < AntiInstantLeaveSettings.MinCooldown or > AntiInstantLeaveSettings.MaxCooldown)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_range_cd(AntiInstantLeaveSettings.MinCooldown, AntiInstantLeaveSettings.MaxCooldown));

                var settings = new AntiInstantLeaveSettings {
                    Enabled = enable,
                    Cooldown = cooldown
                };

                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, gcfg => gcfg.AntiInstantLeaveSettings = settings);

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
                    emb.WithColor(this.ModuleColor);
                    if (enable) {
                        emb.WithLocalizedDescription(TranslationKey.evt_il_enable);
                        emb.AddLocalizedField(TranslationKey.str_cooldown, settings.Cooldown, inline: true);
                    } else {
                        emb.WithLocalizedDescription(TranslationKey.evt_il_disable);
                    }
                });

                await ctx.InfoAsync(this.ModuleColor, enable ? TranslationKey.evt_il_enable : TranslationKey.evt_il_disable);
            }

            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description(TranslationKey.desc_enable)] bool enable)
                => this.ExecuteGroupAsync(ctx, enable, 5);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx)
            {
                return ctx.WithGuildConfigAsync(
                    gcfg => ctx.RespondWithLocalizedEmbedAsync(emb => {
                        emb.WithDescription(gcfg.AntiInstantLeaveSettings.ToEmbedFieldString());
                        emb.WithColor(this.ModuleColor);
                    })
                );
            }
            #endregion

            #region config instantleave cooldown
            [Command("cooldown")]
            [Aliases("setcooldown", "setcool", "cd", "c")]
            public async Task SetCooldownAsync(CommandContext ctx,
                                              [Description(TranslationKey.desc_sens)] short? cooldown = null)
            {
                if (cooldown is null) {
                    await ctx.WithGuildConfigAsync(gcfg => ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_il_cd(gcfg.AntiInstantLeaveCooldown)));
                    return;
                }

                if (cooldown is < AntiInstantLeaveSettings.MinCooldown or > AntiInstantLeaveSettings.MaxCooldown)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_range_cd(AntiInstantLeaveSettings.MinCooldown, AntiInstantLeaveSettings.MaxCooldown));

                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.AntiInstantLeaveCooldown = cooldown.Value;
                });

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
                    emb.WithColor(this.ModuleColor);
                    emb.WithLocalizedDescription(TranslationKey.evt_il_cd(cooldown.Value));
                });

                await ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_il_cd(cooldown.Value));
            }
            #endregion

            #region config instantleave reset
            [Command("reset"), UsesInteractivity]
            [Aliases("default", "def", "s", "rr")]
            public async Task ResetAsync(CommandContext ctx)
            {
                await ctx.WithGuildConfigAsync(gcfg => {
                    return !gcfg.AntiInstantLeaveEnabled ? throw new CommandFailedException(ctx, TranslationKey.cmd_err_reset_il_off) : Task.CompletedTask;
                });

                if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_reset))
                    return;

                var settings = new AntiInstantLeaveSettings();
                await this.ExecuteGroupAsync(ctx, true, settings.Cooldown);
            }
            #endregion
        }
    }
}
