using System;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
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


            #region confing antiflood
            [GroupCommand, Priority(5)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("desc-enable")] bool enable,
                                               [Description("desc-sens")] short sensitivity,
                                               [Description("desc-punish-action")] PunishmentAction action = PunishmentAction.Kick,
                                               [Description("desc-cooldown")] TimeSpan? cooldown = null)
            {
                if (sensitivity < 2 || sensitivity > 20)
                    throw new CommandFailedException("The sensitivity is not in the valid range ([2, 20]).");

                if (cooldown?.TotalSeconds < 5 || cooldown?.TotalSeconds > 60)
                    throw new CommandFailedException("The cooldown timespan is not in the valid range ([5, 60] seconds).");

                var settings = new AntifloodSettings {
                    Action = action,
                    Cooldown = (short?)cooldown?.TotalSeconds ?? 10,
                    Enabled = enable,
                    Sensitivity = sensitivity
                };

                await ctx.Services.GetService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, gcfg => gcfg.AntifloodSettings = settings);

                DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
                if (!(logchn is null)) {
                    var emb = new DiscordEmbedBuilder {
                        Title = "Guild config changed",
                        Description = $"Antiflood {(enable ? "enabled" : "disabled")}",
                        Color = this.ModuleColor
                    };
                    emb.AddField("User responsible", ctx.User.Mention, inline: true);
                    emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                    if (enable) {
                        emb.AddField("Antiflood sensitivity", settings.Sensitivity.ToString(), inline: true);
                        emb.AddField("Antiflood cooldown", settings.Cooldown.ToString(), inline: true);
                        emb.AddField("Antiflood action", settings.Action.ToTypeString(), inline: true);
                    }
                    await logchn.SendMessageAsync(embed: emb.Build());
                }

                await this.InformAsync(ctx, $"{Formatter.Bold(enable ? "Enabled" : "Disabled")} antiflood actions.", important: false);
            }

            [GroupCommand, Priority(4)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-enable")] bool enable,
                                         [Description("desc-punish-action")] PunishmentAction action,
                                         [Description("desc-sens")] short sensitivity = 5,
                                         [Description("desc-cooldown")] TimeSpan? cooldown = null)
                => this.ExecuteGroupAsync(ctx, enable, sensitivity, action, cooldown);

            [GroupCommand, Priority(3)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-enable")] bool enable,
                                         [Description("desc-punish-action")] PunishmentAction action,
                                         [Description("desc-cooldown")] TimeSpan? cooldown = null,
                                         [Description("desc-sens")] short sensitivity = 5)
                => this.ExecuteGroupAsync(ctx, enable, sensitivity, action, cooldown);

            [GroupCommand, Priority(2)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-enable")] bool enable,
                                         [Description("desc-cooldown")] TimeSpan? cooldown,
                                         [Description("desc-punish-action")] PunishmentAction action = PunishmentAction.Kick,
                                         [Description("desc-sens")] short sensitivity = 5)
                => this.ExecuteGroupAsync(ctx, enable, sensitivity, action, cooldown);

            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-enable")] bool enable)
                => this.ExecuteGroupAsync(ctx, enable, 5, PunishmentAction.Kick, null);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx)
            {
                return ctx.WithGuildSettingsAsync(gcfg => {
                    LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
                    return ctx.InfoAsync("fmt-antiflood", gcfg.AntifloodSettings.ToEmbedFieldString(ctx.Guild.Id, lcs));
                });
            }
            #endregion

            #region COMMAND_ANTIFLOOD_ACTION
            [Command("action")]
            [Description("Set the action to execute on the users when they flood/raid the guild.")]
            [Aliases("setaction", "a")]
            public async Task SetActionAsync(CommandContext ctx,
                                            [Description("desc-punish-action")] PunishmentAction action)
            {
                GuildConfig gcfg = await ctx.Services.GetService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.AntifloodAction = action;
                });

                DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
                if (!(logchn is null)) {
                    var emb = new DiscordEmbedBuilder {
                        Title = "Guild config changed",
                        Color = this.ModuleColor
                    };
                    emb.AddField("User responsible", ctx.User.Mention, inline: true);
                    emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                    emb.AddField("Antiflood action changed to", gcfg.AntifloodAction.ToTypeString());
                    await logchn.SendMessageAsync(embed: emb.Build());
                }

                await this.InformAsync(ctx, $"Antiflood action for this guild has been changed to {Formatter.Bold(gcfg.AntifloodAction.ToTypeString())}", important: false);
            }
            #endregion

            #region COMMAND_ANTIFLOOD_SENSITIVITY
            [Command("sensitivity")]
            [Description("Set the antiflood sensitivity. Antiflood action will be executed if the specified " +
                         "amount of users join the guild in the given cooldown period.")]
            [Aliases("setsensitivity", "setsens", "sens", "s")]

            public async Task SetSensitivityAsync(CommandContext ctx,
                                                 [Description("desc-sens")] short sensitivity)
            {
                if (sensitivity < 2 || sensitivity > 20)
                    throw new CommandFailedException("The sensitivity is not in the valid range ([2, 20]).");

                GuildConfig gcfg = await ctx.Services.GetService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.AntifloodSensitivity = sensitivity;
                });

                DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
                if (!(logchn is null)) {
                    var emb = new DiscordEmbedBuilder {
                        Title = "Guild config changed",
                        Color = this.ModuleColor
                    };
                    emb.AddField("User responsible", ctx.User.Mention, inline: true);
                    emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                    emb.AddField("Antiflood sensitivity changed to", $"Max {gcfg.AntifloodSensitivity} users per {gcfg.AntifloodCooldown}s");
                    await logchn.SendMessageAsync(embed: emb.Build());
                }

                await this.InformAsync(ctx, $"Antiflood sensitivity for this guild has been changed to {Formatter.Bold(gcfg.AntifloodSensitivity.ToString())} users per {gcfg.AntifloodCooldown}s", important: false);
            }
            #endregion

            #region COMMAND_ANTIFLOOD_COOLDOWN
            [Command("cooldown")]
            [Description("Set the antiflood sensitivity. Antiflood action will be executed if the specified " +
                         "amount of users join the guild in the given cooldown period.")]
            [Aliases("setcooldown", "setcool", "cool", "c")]

            public async Task SetCooldownAsync(CommandContext ctx,
                                              [Description("desc-cooldown")] TimeSpan cooldown)
            {
                if (cooldown.TotalSeconds < 5 || cooldown.TotalSeconds > 60)
                    throw new CommandFailedException("The cooldown timespan is not in the valid range ([5, 60] seconds).");

                GuildConfig gcfg = await ctx.Services.GetService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.AntifloodCooldown = (short)cooldown.TotalSeconds;
                });

                DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
                if (!(logchn is null)) {
                    var emb = new DiscordEmbedBuilder {
                        Title = "Guild config changed",
                        Color = this.ModuleColor
                    };
                    emb.AddField("User responsible", ctx.User.Mention, inline: true);
                    emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                    emb.AddField("Antiflood cooldown changed to", $"{gcfg.AntifloodCooldown}s");
                    await logchn.SendMessageAsync(embed: emb.Build());
                }

                await this.InformAsync(ctx, $"Antiflood cooldown for this guild has been changed to {gcfg.AntifloodCooldown}s", important: false);
            }
            #endregion
        }
    }
}
