using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration
{
    public partial class ConfigModule
    {
        [Group("antiflood")]
        [Aliases("antiraid", "ar", "af")]
        public class AntifloodModule : TheGodfatherServiceModule<AntifloodService>
        {
            public AntifloodModule(AntifloodService service)
                : base(service) { }


            #region config antiflood
            [GroupCommand, Priority(5)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("desc-enable")] bool enable,
                                               [Description("desc-sens")] short sens,
                                               [Description("desc-punish-action")] PunishmentAction action = PunishmentAction.Kick,
                                               [Description("desc-cooldown")] TimeSpan? cooldown = null)
            {
                if (sens < AntifloodSettings.MinSensitivity || sens > AntifloodSettings.MaxSensitivity)
                    throw new CommandFailedException(ctx, "cmd-err-range-sens", AntifloodSettings.MinSensitivity, AntifloodSettings.MaxSensitivity);

                cooldown ??= TimeSpan.FromSeconds(10);
                if (cooldown.Value.TotalSeconds < AntifloodSettings.MinCooldown || cooldown.Value.TotalSeconds > AntifloodSettings.MaxCooldown)
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
                        emb.WithLocalizedDescription("evt-af-enabled");
                        emb.AddLocalizedTitleField("str-sensitivity", settings.Sensitivity, inline: true);
                        emb.AddLocalizedTitleField("str-cooldown", settings.Cooldown, inline: true);
                        emb.AddLocalizedTitleField("str-punish-action", settings.Action.ToTypeString(), inline: true);
                    } else {
                        emb.WithLocalizedDescription("evt-af-disabled");
                    }
                });

                await ctx.InfoAsync(enable ? "evt-af-enabled" : "evt-af-disabled");
            }

            [GroupCommand, Priority(4)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-enable")] bool enable,
                                         [Description("desc-punish-action")] PunishmentAction action,
                                         [Description("desc-sens")] short sens = 5,
                                         [Description("desc-cooldown")] TimeSpan? cooldown = null)
                => this.ExecuteGroupAsync(ctx, enable, sens, action, cooldown);

            [GroupCommand, Priority(3)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-enable")] bool enable,
                                         [Description("desc-punish-action")] PunishmentAction action,
                                         [Description("desc-cooldown")] TimeSpan? cooldown = null,
                                         [Description("desc-sens")] short sens = 5)
                => this.ExecuteGroupAsync(ctx, enable, sens, action, cooldown);

            [GroupCommand, Priority(2)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-enable")] bool enable,
                                         [Description("desc-cooldown")] TimeSpan? cooldown,
                                         [Description("desc-punish-action")] PunishmentAction action = PunishmentAction.Kick,
                                         [Description("desc-sens")] short sens = 5)
                => this.ExecuteGroupAsync(ctx, enable, sens, action, cooldown);

            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-enable")] bool enable)
                => this.ExecuteGroupAsync(ctx, enable, 5, PunishmentAction.Kick, null);

            [GroupCommand, Priority(0)]
#pragma warning disable CA1822 // Mark members as static
            public Task ExecuteGroupAsync(CommandContext ctx)
#pragma warning restore CA1822 // Mark members as static
            {
                return ctx.WithGuildConfigAsync(gcfg => {
                    LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
                    return ctx.InfoAsync("fmt-antiflood", gcfg.AntifloodSettings.ToEmbedFieldString(ctx.Guild.Id, lcs));
                });
            }
            #endregion

            #region config antiflood action
            [Command("action")]
            [Aliases("setaction", "setact", "act", "a")]
            public async Task SetActionAsync(CommandContext ctx,
                                            [Description("desc-punish-action")] PunishmentAction? action = null)
            {
                if (action is null) {
                    await ctx.WithGuildConfigAsync(gcfg => ctx.InfoAsync("evt-af-action", gcfg.AntifloodAction.ToTypeString()));
                    return;
                }

                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.AntifloodAction = action.Value;
                });

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-cfg-upd");
                    emb.WithColor(this.ModuleColor);
                    emb.WithLocalizedDescription("evt-af-action", action.Value.ToTypeString());
                });

                await ctx.InfoAsync("evt-af-action", action.Value.ToTypeString());
            }
            #endregion

            #region config antiflood sensitivity
            [Command("sensitivity")]
            [Aliases("setsensitivity", "setsens", "sens", "s")]
            public async Task SetSensitivityAsync(CommandContext ctx,
                                                 [Description("desc-sens")] short? sens = null)
            {
                if (sens is null) {
                    await ctx.WithGuildConfigAsync(gcfg => ctx.InfoAsync("evt-af-sens", gcfg.AntifloodSensitivity));
                    return;
                }
                
                if (sens < AntifloodSettings.MinSensitivity || sens > AntifloodSettings.MaxSensitivity)
                    throw new CommandFailedException(ctx, "cmd-err-range-sens", AntifloodSettings.MinSensitivity, AntifloodSettings.MaxSensitivity);

                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.AntifloodSensitivity = sens.Value;
                });

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-cfg-upd");
                    emb.WithColor(this.ModuleColor);
                    emb.WithLocalizedDescription("evt-af-sens", sens.Value);
                });

                await ctx.InfoAsync("evt-af-sens", sens.Value);
            }
            #endregion

            #region config antiflood cooldown
            [Command("cooldown")]
            [Aliases("setcooldown", "setcool", "cool", "c")]
            public async Task SetCooldownAsync(CommandContext ctx,
                                              [Description("desc-cooldown")] TimeSpan? cooldown = null)
            {
                if (cooldown is null) {
                    await ctx.WithGuildConfigAsync(gcfg => ctx.InfoAsync("evt-af-cd", gcfg.AntifloodCooldown));
                    return;
                }

                if (cooldown.Value.TotalSeconds < AntifloodSettings.MinCooldown || cooldown.Value.TotalSeconds > AntifloodSettings.MaxCooldown)
                    throw new CommandFailedException(ctx, "cmd-err-range-cd", AntifloodSettings.MinCooldown, AntifloodSettings.MaxCooldown);

                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.AntifloodCooldown = (short)cooldown.Value.TotalSeconds;
                });

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-cfg-upd");
                    emb.WithColor(this.ModuleColor);
                    emb.WithLocalizedDescription("evt-af-cd", cooldown.Value.TotalSeconds);
                });

                await ctx.InfoAsync("evt-af-cd", cooldown.Value);
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
