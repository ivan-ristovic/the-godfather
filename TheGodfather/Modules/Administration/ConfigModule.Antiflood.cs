using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration;

public sealed partial class ConfigModule
{
    [Group("antiflood")]
    [Aliases("antiraid", "ar", "af")]
    public sealed class AntifloodModule : TheGodfatherServiceModule<AntifloodService>
    {
        #region config antiflood
        [GroupCommand][Priority(5)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
            [Description(TranslationKey.desc_enable)] bool enable,
            [Description(TranslationKey.desc_sens)] short sens,
            [Description(TranslationKey.desc_punish_action)] Punishment.Action action = Punishment.Action.Kick,
            [Description(TranslationKey.desc_cooldown)] TimeSpan? cooldown = null)
        {
            if (sens is < AntifloodSettings.MinSensitivity or > AntifloodSettings.MaxSensitivity)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_range_sens(AntifloodSettings.MinSensitivity, AntifloodSettings.MaxSensitivity));

            cooldown ??= TimeSpan.FromSeconds(10);
            if (cooldown.Value.TotalSeconds is < AntifloodSettings.MinCooldown or > AntifloodSettings.MaxCooldown)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_range_cd(AntifloodSettings.MinCooldown, AntifloodSettings.MaxCooldown));

            var settings = new AntifloodSettings {
                Action = action,
                Cooldown = (short)cooldown.Value.TotalSeconds,
                Enabled = enable,
                Sensitivity = sens
            };

            await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, gcfg => gcfg.AntifloodSettings = settings);

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
                emb.WithColor(this.ModuleColor);
                if (enable) {
                    emb.WithLocalizedDescription(TranslationKey.evt_af_enable);
                    emb.AddLocalizedField(TranslationKey.str_sensitivity, settings.Sensitivity, true);
                    emb.AddLocalizedField(TranslationKey.str_cooldown, settings.Cooldown, true);
                    emb.AddLocalizedField(TranslationKey.str_punish_action, settings.Action.Humanize(), true);
                } else {
                    emb.WithLocalizedDescription(TranslationKey.evt_af_disable);
                }
            });

            await ctx.InfoAsync(this.ModuleColor, enable ? TranslationKey.evt_af_enable : TranslationKey.evt_af_disable);
        }

        [GroupCommand][Priority(4)]
        public Task ExecuteGroupAsync(CommandContext ctx,
            [Description(TranslationKey.desc_enable)] bool enable,
            [Description(TranslationKey.desc_punish_action)] Punishment.Action action,
            [Description(TranslationKey.desc_sens)] short sens = 5,
            [Description(TranslationKey.desc_cooldown)] TimeSpan? cooldown = null)
            => this.ExecuteGroupAsync(ctx, enable, sens, action, cooldown);

        [GroupCommand][Priority(3)]
        public Task ExecuteGroupAsync(CommandContext ctx,
            [Description(TranslationKey.desc_enable)] bool enable,
            [Description(TranslationKey.desc_punish_action)] Punishment.Action action,
            [Description(TranslationKey.desc_cooldown)] TimeSpan? cooldown = null,
            [Description(TranslationKey.desc_sens)] short sens = 5)
            => this.ExecuteGroupAsync(ctx, enable, sens, action, cooldown);

        [GroupCommand][Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx,
            [Description(TranslationKey.desc_enable)] bool enable,
            [Description(TranslationKey.desc_cooldown)] TimeSpan? cooldown,
            [Description(TranslationKey.desc_punish_action)] Punishment.Action action = Punishment.Action.Kick,
            [Description(TranslationKey.desc_sens)] short sens = 5)
            => this.ExecuteGroupAsync(ctx, enable, sens, action, cooldown);

        [GroupCommand][Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
            [Description(TranslationKey.desc_enable)] bool enable)
            => this.ExecuteGroupAsync(ctx, enable, 5);

        [GroupCommand][Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx)
        {
            return ctx.WithGuildConfigAsync(
                gcfg => ctx.RespondWithLocalizedEmbedAsync(emb => {
                    emb.WithDescription(gcfg.AntifloodSettings.ToEmbedFieldString());
                    emb.WithColor(this.ModuleColor);
                })
            );
        }
        #endregion

        #region config antiflood action
        [Command("action")]
        [Aliases("setaction", "setact", "act", "a")]
        public async Task SetActionAsync(CommandContext ctx,
            [Description(TranslationKey.desc_punish_action)] Punishment.Action? action = null)
        {
            if (action is null) {
                await ctx.WithGuildConfigAsync(gcfg => ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_af_action(gcfg.AntifloodAction.Humanize())));
                return;
            }

            await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.AntifloodAction = action.Value;
            });

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedDescription(TranslationKey.evt_af_action(action.Value.Humanize()));
            });

            await ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_af_action(action.Value.Humanize()));
        }
        #endregion

        #region config antiflood sensitivity
        [Command("sensitivity")]
        [Aliases("setsensitivity", "setsens", "sens", "s")]
        public async Task SetSensitivityAsync(CommandContext ctx,
            [Description(TranslationKey.desc_sens)] short? sens = null)
        {
            if (sens is null) {
                await ctx.WithGuildConfigAsync(gcfg => ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_af_sens(gcfg.AntifloodSensitivity)));
                return;
            }

            if (sens is < AntifloodSettings.MinSensitivity or > AntifloodSettings.MaxSensitivity)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_range_sens(AntifloodSettings.MinSensitivity, AntifloodSettings.MaxSensitivity));

            await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.AntifloodSensitivity = sens.Value;
            });

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedDescription(TranslationKey.evt_af_sens(sens.Value));
            });

            await ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_af_sens(sens.Value));
        }
        #endregion

        #region config antiflood cooldown
        [Command("cooldown")]
        [Aliases("setcooldown", "setcool", "cd", "c")]
        public async Task SetCooldownAsync(CommandContext ctx,
            [Description(TranslationKey.desc_cooldown)] TimeSpan? cooldown = null)
        {
            if (cooldown is null) {
                await ctx.WithGuildConfigAsync(gcfg => ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_af_cd(gcfg.AntifloodCooldown)));
                return;
            }

            if (cooldown.Value.TotalSeconds is < AntifloodSettings.MinCooldown or > AntifloodSettings.MaxCooldown)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_range_cd(AntifloodSettings.MinCooldown, AntifloodSettings.MaxCooldown));

            await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.AntifloodCooldown = (short)cooldown.Value.TotalSeconds;
            });

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedDescription(TranslationKey.evt_af_cd(cooldown.Value.TotalSeconds));
            });

            await ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_af_cd(cooldown.Value));
        }
        #endregion

        #region config antiflood reset
        [Command("reset")][UsesInteractivity]
        [Aliases("default", "def", "s", "rr")]
        public async Task ResetAsync(CommandContext ctx)
        {
            await ctx.WithGuildConfigAsync(gcfg => {
                return !gcfg.AntifloodEnabled ? throw new CommandFailedException(ctx, TranslationKey.cmd_err_reset_af_off) : Task.CompletedTask;
            });

            if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_reset))
                return;

            var settings = new AntifloodSettings();
            await this.ExecuteGroupAsync(ctx, true, settings.Action, settings.Sensitivity, TimeSpan.FromSeconds(settings.Cooldown));
        }
        #endregion
    }
}