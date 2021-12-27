using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
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
        [Group("antiflood")]
        [Aliases("antiraid", "ar", "af")]
        public sealed class AntifloodModule : TheGodfatherServiceModule<AntifloodService>
        {
            #region config antiflood
            [GroupCommand, Priority(5)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("desc-enable")] bool enable,
                                               [Description("desc-sens")] short sens,
                                               [Description("desc-punish-action")] Punishment.Action action = Punishment.Action.Kick,
                                               [Description("desc-cooldown")] TimeSpan? cooldown = null)
            {
                if (sens is < AntifloodSettings.MinSensitivity or > AntifloodSettings.MaxSensitivity)
                    throw new CommandFailedException(ctx, "cmd-err-range-sens", AntifloodSettings.MinSensitivity, AntifloodSettings.MaxSensitivity);

                cooldown ??= TimeSpan.FromSeconds(10);
                if (cooldown.Value.TotalSeconds is < AntifloodSettings.MinCooldown or > AntifloodSettings.MaxCooldown)
                    throw new CommandFailedException(ctx, "cmd-err-range-cd", AntifloodSettings.MinCooldown, AntifloodSettings.MaxCooldown);

                var settings = new AntifloodSettings {
                    Action = action,
                    Cooldown = (short)cooldown.Value.TotalSeconds,
                    Enabled = enable,
                    Sensitivity = sens
                };

                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, gcfg => gcfg.AntifloodSettings = settings);

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-cfg-upd");
                    emb.WithColor(this.ModuleColor);
                    if (enable) {
                        emb.WithLocalizedDescription("evt-af-enable");
                        emb.AddLocalizedField("str-sensitivity", settings.Sensitivity, inline: true);
                        emb.AddLocalizedField("str-cooldown", settings.Cooldown, inline: true);
                        emb.AddLocalizedField("str-punish-action", settings.Action.Humanize(), inline: true);
                    } else {
                        emb.WithLocalizedDescription("evt-af-disable");
                    }
                });

                await ctx.InfoAsync(this.ModuleColor, enable ? "evt-af-enable" : "evt-af-disable");
            }

            [GroupCommand, Priority(4)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-enable")] bool enable,
                                         [Description("desc-punish-action")] Punishment.Action action,
                                         [Description("desc-sens")] short sens = 5,
                                         [Description("desc-cooldown")] TimeSpan? cooldown = null)
                => this.ExecuteGroupAsync(ctx, enable, sens, action, cooldown);

            [GroupCommand, Priority(3)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-enable")] bool enable,
                                         [Description("desc-punish-action")] Punishment.Action action,
                                         [Description("desc-cooldown")] TimeSpan? cooldown = null,
                                         [Description("desc-sens")] short sens = 5)
                => this.ExecuteGroupAsync(ctx, enable, sens, action, cooldown);

            [GroupCommand, Priority(2)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-enable")] bool enable,
                                         [Description("desc-cooldown")] TimeSpan? cooldown,
                                         [Description("desc-punish-action")] Punishment.Action action = Punishment.Action.Kick,
                                         [Description("desc-sens")] short sens = 5)
                => this.ExecuteGroupAsync(ctx, enable, sens, action, cooldown);

            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-enable")] bool enable)
                => this.ExecuteGroupAsync(ctx, enable, 5, Punishment.Action.Kick, null);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx)
            {
                return ctx.WithGuildConfigAsync(
                    gcfg => ctx.RespondWithLocalizedEmbedAsync(emb => {
                        emb.WithDescription(gcfg.AntifloodSettings.ToEmbedFieldString(ctx.Guild.Id, this.Localization));
                        emb.WithColor(this.ModuleColor);
                    })
                );
            }
            #endregion

            #region config antiflood action
            [Command("action")]
            [Aliases("setaction", "setact", "act", "a")]
            public async Task SetActionAsync(CommandContext ctx,
                                            [Description("desc-punish-action")] Punishment.Action? action = null)
            {
                if (action is null) {
                    await ctx.WithGuildConfigAsync(gcfg => ctx.InfoAsync(this.ModuleColor, "evt-af-action", gcfg.AntifloodAction.Humanize()));
                    return;
                }

                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.AntifloodAction = action.Value;
                });

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-cfg-upd");
                    emb.WithColor(this.ModuleColor);
                    emb.WithLocalizedDescription("evt-af-action", action.Value.Humanize());
                });

                await ctx.InfoAsync(this.ModuleColor, "evt-af-action", action.Value.Humanize());
            }
            #endregion

            #region config antiflood sensitivity
            [Command("sensitivity")]
            [Aliases("setsensitivity", "setsens", "sens", "s")]
            public async Task SetSensitivityAsync(CommandContext ctx,
                                                 [Description("desc-sens")] short? sens = null)
            {
                if (sens is null) {
                    await ctx.WithGuildConfigAsync(gcfg => ctx.InfoAsync(this.ModuleColor, "evt-af-sens", gcfg.AntifloodSensitivity));
                    return;
                }

                if (sens is < AntifloodSettings.MinSensitivity or > AntifloodSettings.MaxSensitivity)
                    throw new CommandFailedException(ctx, "cmd-err-range-sens", AntifloodSettings.MinSensitivity, AntifloodSettings.MaxSensitivity);

                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.AntifloodSensitivity = sens.Value;
                });

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-cfg-upd");
                    emb.WithColor(this.ModuleColor);
                    emb.WithLocalizedDescription("evt-af-sens", sens.Value);
                });

                await ctx.InfoAsync(this.ModuleColor, "evt-af-sens", sens.Value);
            }
            #endregion

            #region config antiflood cooldown
            [Command("cooldown")]
            [Aliases("setcooldown", "setcool", "cd", "c")]
            public async Task SetCooldownAsync(CommandContext ctx,
                                              [Description("desc-cooldown")] TimeSpan? cooldown = null)
            {
                if (cooldown is null) {
                    await ctx.WithGuildConfigAsync(gcfg => ctx.InfoAsync(this.ModuleColor, "evt-af-cd", gcfg.AntifloodCooldown));
                    return;
                }

                if (cooldown.Value.TotalSeconds is < AntifloodSettings.MinCooldown or > AntifloodSettings.MaxCooldown)
                    throw new CommandFailedException(ctx, "cmd-err-range-cd", AntifloodSettings.MinCooldown, AntifloodSettings.MaxCooldown);

                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.AntifloodCooldown = (short)cooldown.Value.TotalSeconds;
                });

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-cfg-upd");
                    emb.WithColor(this.ModuleColor);
                    emb.WithLocalizedDescription("evt-af-cd", cooldown.Value.TotalSeconds);
                });

                await ctx.InfoAsync(this.ModuleColor, "evt-af-cd", cooldown.Value);
            }
            #endregion

            #region config antiflood reset
            [Command("reset"), UsesInteractivity]
            [Aliases("default", "def", "s", "rr")]
            public async Task ResetAsync(CommandContext ctx)
            {
                await ctx.WithGuildConfigAsync(gcfg => {
                    return !gcfg.AntifloodEnabled ? throw new CommandFailedException(ctx, "cmd-err-reset-af-off") : Task.CompletedTask;
                });

                if (!await ctx.WaitForBoolReplyAsync("q-setup-reset"))
                    return;

                var settings = new AntifloodSettings();
                await this.ExecuteGroupAsync(ctx, true, settings.Action, settings.Sensitivity, TimeSpan.FromSeconds(settings.Cooldown));
            }
            #endregion
        }
    }
}
