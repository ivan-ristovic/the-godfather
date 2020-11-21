using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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
        [Group("ratelimit")]
        [Aliases("rl")]
        public class RatelimitModule : TheGodfatherServiceModule<RatelimitService>
        {
            public RatelimitModule(RatelimitService service)
                : base(service) { }


            #region config ratelimit
            [GroupCommand, Priority(3)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("desc-enable")] bool enable,
                                               [Description("desc-sens")] short sens,
                                               [Description("desc-punish-action")] PunishmentAction action = PunishmentAction.TemporaryMute)
            {
                if (sens < RatelimitSettings.MinSensitivity || sens > RatelimitSettings.MaxSensitivity)
                    throw new CommandFailedException(ctx, "cmd-err-range-sens", RatelimitSettings.MinSensitivity, RatelimitSettings.MaxSensitivity);

                var settings = new RatelimitSettings {
                    Action = action,
                    Enabled = enable,
                    Sensitivity = sens
                };

                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, gcfg => gcfg.RatelimitSettings = settings);

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-cfg-upd");
                    emb.WithColor(this.ModuleColor);
                    if (enable) {
                        emb.WithLocalizedDescription("evt-rl-enabled");
                        emb.AddLocalizedTitleField("str-sensitivity", settings.Sensitivity, inline: true);
                        emb.AddLocalizedTitleField("str-punish-action", settings.Action.ToTypeString(), inline: true);
                    } else {
                        emb.WithLocalizedDescription("evt-rl-disabled");
                    }
                });

                await ctx.InfoAsync(enable ? "evt-rl-enabled" : "evt-rl-disabled");
            }

            [GroupCommand, Priority(2)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-enable")] bool enable,
                                         [Description("desc-punish-action")] PunishmentAction action,
                                         [Description("desc-sens")] short sens = 5)
                => this.ExecuteGroupAsync(ctx, enable, sens, action);

            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-enable")] bool enable)
                => this.ExecuteGroupAsync(ctx, enable, 5, PunishmentAction.Kick);

            [GroupCommand, Priority(0)]
#pragma warning disable CA1822 // Mark members as static
            public Task ExecuteGroupAsync(CommandContext ctx)
#pragma warning restore CA1822 // Mark members as static
            {
                return ctx.WithGuildConfigAsync(gcfg => {
                    LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
                    return ctx.InfoAsync("fmt-settings-rl", gcfg.RatelimitSettings.ToEmbedFieldString(ctx.Guild.Id, lcs));
                });
            }
            #endregion

            #region config ratelimit action
            [Command("action")]
            [Aliases("setaction", "setact", "act", "a")]
            public async Task SetActionAsync(CommandContext ctx,
                                            [Description("desc-punish-action")] PunishmentAction? action = null)
            {
                if (action is null) {
                    await ctx.WithGuildConfigAsync(gcfg => ctx.InfoAsync("evt-rl-action", gcfg.RatelimitAction.ToTypeString()));
                    return;
                }

                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.RatelimitAction = action.Value;
                });

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-cfg-upd");
                    emb.WithColor(this.ModuleColor);
                    emb.WithLocalizedDescription("evt-rl-action", action.Value.ToTypeString());
                });

                await ctx.InfoAsync("evt-rl-action", action.Value.ToTypeString());
            }
            #endregion

            #region config ratelimit sensitivity
            [Command("sensitivity")]
            [Aliases("setsensitivity", "setsens", "sens", "s")]
            public async Task SetSensitivityAsync(CommandContext ctx,
                                                 [Description("desc-sens")] short? sens = null)
            {
                if (sens is null) {
                    await ctx.WithGuildConfigAsync(gcfg => ctx.InfoAsync("evt-rl-sens", gcfg.RatelimitSensitivity));
                    return;
                }

                if (sens < RatelimitSettings.MinSensitivity || sens > RatelimitSettings.MaxSensitivity)
                    throw new CommandFailedException(ctx, "cmd-err-range-sens", RatelimitSettings.MinSensitivity, RatelimitSettings.MaxSensitivity);

                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.RatelimitSensitivity = sens.Value;
                });

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-cfg-upd");
                    emb.WithColor(this.ModuleColor);
                    emb.WithLocalizedDescription("evt-rl-sens", sens.Value);
                });

                await ctx.InfoAsync("evt-rl-sens", sens.Value);
            }
            #endregion

            #region config ratelimit reset
            [Command("reset"), UsesInteractivity]
            [Aliases("default", "def", "s", "rr")]
            public async Task ResetAsync(CommandContext ctx)
            {
                await ctx.WithGuildConfigAsync(gcfg => {
                    return !gcfg.RatelimitEnabled ? throw new CommandFailedException(ctx, "cmd-err-reset-as-off") : Task.CompletedTask;
                });

                if (!await ctx.WaitForBoolReplyAsync("q-setup-reset"))
                    return;

                var settings = new RatelimitSettings();
                await this.ExecuteGroupAsync(ctx, true, settings.Action, settings.Sensitivity);
            }
            #endregion

            #region config ratelimit exempt
            [Command("exempt"), Priority(2)]
            [Aliases("ex", "exc")]
            public async Task ExemptAsync(CommandContext ctx,
                                         [Description("desc-exempt-user")] params DiscordMember[] members)
            {
                if (members is null || !members.Any())
                    throw new CommandFailedException(ctx, "cmd-err-exempt");

                await this.Service.ExemptAsync(ctx.Guild.Id, ExemptedEntityType.Member, members);
                await ctx.InfoAsync();
            }

            [Command("exempt"), Priority(1)]
            public async Task ExemptAsync(CommandContext ctx,
                                         [Description("desc-exempt-role")] params DiscordRole[] roles)
            {
                if (roles is null || !roles.Any())
                    throw new CommandFailedException(ctx, "cmd-err-exempt");

                await this.Service.ExemptAsync(ctx.Guild.Id, ExemptedEntityType.Role, roles);
                await ctx.InfoAsync();
            }

            [Command("exempt"), Priority(0)]
            public async Task ExemptAsync(CommandContext ctx,
                                         [Description("desc-exempt-chn")] params DiscordChannel[] channels)
            {
                if (channels is null || !channels.Any())
                    throw new CommandFailedException(ctx, "cmd-err-exempt");

                await this.Service.ExemptAsync(ctx.Guild.Id, ExemptedEntityType.Channel, channels);
                await ctx.InfoAsync();
            }
            #endregion

            #region config ratelimit unexempt
            [Command("unexempt"), Priority(2)]
            [Aliases("unex", "uex")]
            public async Task UnxemptAsync(CommandContext ctx,
                                          [Description("desc-unexempt-user")] params DiscordMember[] members)
            {
                if (members is null || !members.Any())
                    throw new CommandFailedException(ctx, "cmd-err-exempt");

                await this.Service.UnexemptAsync(ctx.Guild.Id, ExemptedEntityType.Member, members);
                await ctx.InfoAsync();
            }

            [Command("unexempt"), Priority(1)]
            public async Task UnxemptAsync(CommandContext ctx,
                                          [Description("desc-unexempt-role")] params DiscordRole[] roles)
            {
                if (roles is null || !roles.Any())
                    throw new CommandFailedException(ctx, "cmd-err-exempt");

                await this.Service.UnexemptAsync(ctx.Guild.Id, ExemptedEntityType.Role, roles);
                await ctx.InfoAsync();
            }

            [Command("unexempt"), Priority(0)]
            public async Task UnxemptAsync(CommandContext ctx,
                                          [Description("desc-unexempt-chn")] params DiscordChannel[] channels)
            {
                if (channels is null || !channels.Any())
                    throw new CommandFailedException(ctx, "cmd-err-exempt");

                await this.Service.UnexemptAsync(ctx.Guild.Id, ExemptedEntityType.Channel, channels);
                await ctx.InfoAsync();
            }
            #endregion
        }
    }
}
