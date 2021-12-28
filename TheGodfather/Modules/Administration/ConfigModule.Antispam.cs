using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration
{
    public sealed partial class ConfigModule
    {
        [Group("antispam")]
        [Aliases("as")]
        public sealed class AntispamModule : TheGodfatherServiceModule<AntispamService>
        {
            #region config antispam
            [GroupCommand, Priority(3)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description(TranslationKey.desc_enable)] bool enable,
                                               [Description(TranslationKey.desc_sens)] short sens,
                                               [Description(TranslationKey.desc_punish_action)] Punishment.Action action = Punishment.Action.TemporaryMute)
            {
                if (sens is < AntispamSettings.MinSensitivity or > AntispamSettings.MaxSensitivity)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_range_sens(AntispamSettings.MinSensitivity, AntispamSettings.MaxSensitivity));

                var settings = new AntispamSettings {
                    Action = action,
                    Enabled = enable,
                    Sensitivity = sens
                };

                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, gcfg => gcfg.AntispamSettings = settings);

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
                    emb.WithColor(this.ModuleColor);
                    if (enable) {
                        emb.WithLocalizedDescription(TranslationKey.evt_as_enable);
                        emb.AddLocalizedField(TranslationKey.str_sensitivity, settings.Sensitivity, inline: true);
                        emb.AddLocalizedField(TranslationKey.str_punish_action, settings.Action.Humanize(), inline: true);
                    } else {
                        emb.WithLocalizedDescription(TranslationKey.evt_as_disable);
                    }
                });

                await ctx.InfoAsync(enable ? TranslationKey.evt_as_enable : TranslationKey.evt_as_disable);
            }

            [GroupCommand, Priority(2)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description(TranslationKey.desc_enable)] bool enable,
                                         [Description(TranslationKey.desc_punish_action)] Punishment.Action action,
                                         [Description(TranslationKey.desc_sens)] short sens = 5)
                => this.ExecuteGroupAsync(ctx, enable, sens, action);

            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description(TranslationKey.desc_enable)] bool enable)
                => this.ExecuteGroupAsync(ctx, enable, 5, Punishment.Action.TemporaryMute);

            [GroupCommand, Priority(0)]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                IReadOnlyList<ExemptedSpamEntity> exempts = await this.Service.GetExemptsAsync(ctx.Guild.Id);
                string? exemptString = await exempts.FormatExemptionsAsync(ctx.Client);
                await ctx.WithGuildConfigAsync(gcfg => {
                    return ctx.RespondWithLocalizedEmbedAsync(emb => {
                        emb.WithLocalizedTitle(TranslationKey.str_antispam);
                        emb.WithDescription(gcfg.AntispamSettings.ToEmbedFieldString());
                        emb.WithColor(this.ModuleColor);
                        if (exemptString is { })
                            emb.AddLocalizedField(TranslationKey.str_exempts, exemptString, inline: true);
                    });
                });
            }
            #endregion

            #region config antispam action
            [Command("action")]
            [Aliases("setaction", "setact", "act", "a")]
            public async Task SetActionAsync(CommandContext ctx,
                                            [Description(TranslationKey.desc_punish_action)] Punishment.Action? action = null)
            {
                if (action is null) {
                    await ctx.WithGuildConfigAsync(gcfg => ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_as_action(gcfg.AntispamAction.Humanize())));
                    return;
                }

                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.AntispamAction = action.Value;
                });

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
                    emb.WithColor(this.ModuleColor);
                    emb.WithLocalizedDescription(TranslationKey.evt_as_action(action.Value.Humanize()));
                });

                await ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_as_action(action.Value.Humanize()));
            }
            #endregion

            #region config antispam sensitivity
            [Command("sensitivity")]
            [Aliases("setsensitivity", "setsens", "sens", "s")]
            public async Task SetSensitivityAsync(CommandContext ctx,
                                                 [Description(TranslationKey.desc_sens)] short? sens = null)
            {
                if (sens is null) {
                    await ctx.WithGuildConfigAsync(gcfg => ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_as_sens(gcfg.AntispamSensitivity)));
                    return;
                }

                if (sens is < AntispamSettings.MinSensitivity or > AntispamSettings.MaxSensitivity)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_range_sens(AntispamSettings.MinSensitivity, AntispamSettings.MaxSensitivity));

                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.AntispamSensitivity = sens.Value;
                });

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
                    emb.WithColor(this.ModuleColor);
                    emb.WithLocalizedDescription(TranslationKey.evt_as_sens(sens.Value));
                });

                await ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_as_sens(sens.Value));
            }
            #endregion

            #region config antispam reset
            [Command("reset"), UsesInteractivity]
            [Aliases("default", "def", "s", "rr")]
            public async Task ResetAsync(CommandContext ctx)
            {
                await ctx.WithGuildConfigAsync(gcfg => {
                    return !gcfg.AntispamEnabled ? throw new CommandFailedException(ctx, TranslationKey.cmd_err_reset_as_off) : Task.CompletedTask;
                });

                if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_reset))
                    return;

                var settings = new AntispamSettings();
                await this.ExecuteGroupAsync(ctx, true, settings.Action, settings.Sensitivity);
            }
            #endregion

            #region config antispam exempt
            [Command("exempt"), Priority(2)]
            [Aliases("ex", "exc")]
            public async Task ExemptAsync(CommandContext ctx,
                                         [Description(TranslationKey.desc_exempt_user)] params DiscordMember[] members)
            {
                if (members is null || !members.Any())
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_exempt);

                await this.Service.ExemptAsync(ctx.Guild.Id, ExemptedEntityType.Member, members.SelectIds());
                await ctx.InfoAsync(this.ModuleColor);
            }

            [Command("exempt"), Priority(1)]
            public async Task ExemptAsync(CommandContext ctx,
                                         [Description(TranslationKey.desc_exempt_role)] params DiscordRole[] roles)
            {
                if (roles is null || !roles.Any())
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_exempt);

                await this.Service.ExemptAsync(ctx.Guild.Id, ExemptedEntityType.Role, roles.SelectIds());
                await ctx.InfoAsync(this.ModuleColor);
            }

            [Command("exempt"), Priority(0)]
            public async Task ExemptAsync(CommandContext ctx,
                                         [Description(TranslationKey.desc_exempt_chn)] params DiscordChannel[] channels)
            {
                if (channels is null || !channels.Any())
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_exempt);

                await this.Service.ExemptAsync(ctx.Guild.Id, ExemptedEntityType.Channel, channels.SelectIds());
                await ctx.InfoAsync(this.ModuleColor);
            }
            #endregion

            #region config antispam unexempt
            [Command("unexempt"), Priority(2)]
            [Aliases("unex", "uex")]
            public async Task UnxemptAsync(CommandContext ctx,
                                          [Description(TranslationKey.desc_unexempt_user)] params DiscordMember[] members)
            {
                if (members is null || !members.Any())
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_exempt);

                await this.Service.UnexemptAsync(ctx.Guild.Id, ExemptedEntityType.Member, members.SelectIds());
                await ctx.InfoAsync(this.ModuleColor);
            }

            [Command("unexempt"), Priority(1)]
            public async Task UnxemptAsync(CommandContext ctx,
                                          [Description(TranslationKey.desc_unexempt_role)] params DiscordRole[] roles)
            {
                if (roles is null || !roles.Any())
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_exempt);

                await this.Service.UnexemptAsync(ctx.Guild.Id, ExemptedEntityType.Role, roles.SelectIds());
                await ctx.InfoAsync(this.ModuleColor);
            }

            [Command("unexempt"), Priority(0)]
            public async Task UnxemptAsync(CommandContext ctx,
                                          [Description(TranslationKey.desc_unexempt_chn)] params DiscordChannel[] channels)
            {
                if (channels is null || !channels.Any())
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_exempt);

                await this.Service.UnexemptAsync(ctx.Guild.Id, ExemptedEntityType.Channel, channels.SelectIds());
                await ctx.InfoAsync(this.ModuleColor);
            }
            #endregion
        }
    }
}
