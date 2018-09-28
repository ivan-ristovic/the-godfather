#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Administration
{
    public partial class GuildModule
    {
        public partial class GuildConfigModule
        {
            [Group("antispam")]
            [Description("Prevents users from posting more than specified amount of same messages.")]
            [Aliases("as")]
            [UsageExamples("!guild cfg antispam",
                           "!guild cfg antispam on",
                           "!guild cfg antispam on mute",
                           "!guild cfg antispam on 5",
                           "!guild cfg antispam on 6 kick")]
            public class AntispamModule : TheGodfatherServiceModule<AntispamService>
            {

                public AntispamModule(AntispamService service, SharedData shared, DBService db)
                    : base(service, shared, db)
                {
                    this.ModuleColor = DiscordColor.DarkRed;
                }


                [GroupCommand, Priority(3)]
                public async Task ExecuteGroupAsync(CommandContext ctx,
                                                   [Description("Enable?")] bool enable,
                                                   [Description("Sensitivity (max repeated messages).")] short sensitivity,
                                                   [Description("Action type.")] PunishmentActionType action = PunishmentActionType.Mute)
                {
                    if (sensitivity < 3 || sensitivity > 10)
                        throw new CommandFailedException("The sensitivity is not in the valid range ([3, 10]).");

                    CachedGuildConfig gcfg = this.Shared.GetGuildConfig(ctx.Guild.Id);
                    gcfg.AntispamSettings.Enabled = enable;
                    gcfg.AntispamSettings.Action = action;
                    gcfg.AntispamSettings.Sensitivity = sensitivity;

                    await this.Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg);

                    DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                    if (logchn != null) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Description = $"Antispam {(enable ? "enabled" : "disabled")}",
                            Color = this.ModuleColor
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        if (enable) {
                            emb.AddField("Antispam sensitivity", gcfg.AntispamSettings.Sensitivity.ToString(), inline: true);
                            emb.AddField("Antispam action", gcfg.AntispamSettings.Action.ToTypeString(), inline: true);
                        }
                        await logchn.SendMessageAsync(embed: emb.Build());
                    }

                    await this.InformAsync(ctx, $"{Formatter.Bold(gcfg.AntispamSettings.Enabled ? "Enabled" : "Disabled")} antispam actions.", important: false);
                }

                [GroupCommand, Priority(2)]
                public Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("Enable?")] bool enable,
                                             [Description("Action type.")] PunishmentActionType action,
                                             [Description("Sensitivity (max repeated messages).")] short sensitivity = 5)
                    => this.ExecuteGroupAsync(ctx, enable, sensitivity, action);

                [GroupCommand, Priority(1)]
                public Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("Enable?")] bool enable)
                    => this.ExecuteGroupAsync(ctx, enable, 5, PunishmentActionType.Mute);

                [GroupCommand, Priority(0)]
                public Task ExecuteGroupAsync(CommandContext ctx)
                {
                    CachedGuildConfig gcfg = this.Shared.GetGuildConfig(ctx.Guild.Id);
                    return this.InformAsync(ctx, $"Antispam watch for this guild is {Formatter.Bold(gcfg.AntispamSettings.Enabled ? "enabled" : "disabled")}");
                }


                #region COMMAND_ANTISPAM_ACTION
                [Command("action")]
                [Description("Set the action to execute when the antispam quota is hit.")]
                [Aliases("setaction", "a")]
                [UsageExamples("!guild cfg antispam action mute",
                               "!guild cfg antispam action temporaryban")]
                public async Task SetActionAsync(CommandContext ctx,
                                                [Description("Action type.")] PunishmentActionType action)
                {
                    CachedGuildConfig gcfg = this.Shared.GetGuildConfig(ctx.Guild.Id);
                    gcfg.AntispamSettings.Action = action;

                    await this.Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg);

                    DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                    if (logchn != null) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Color = this.ModuleColor
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Antispam action changed to", gcfg.AntispamSettings.Action.ToTypeString());
                        await logchn.SendMessageAsync(embed: emb.Build());
                    }

                    await this.InformAsync(ctx, $"Antispam action for this guild has been changed to {Formatter.Bold(gcfg.AntispamSettings.Action.ToTypeString())}", important: false);
                }
                #endregion

                #region COMMAND_ANTISPAM_SENSITIVITY
                [Command("sensitivity")]
                [Description("Set the antispam sensitivity - max amount of repeated messages before an action is taken.")]
                [Aliases("setsensitivity", "setsens", "sens", "s")]
                [UsageExamples("!guild cfg antispam sensitivity 9")]
                public async Task SetSensitivityAsync(CommandContext ctx,
                                                     [Description("Sensitivity (max repeated messages).")] short sensitivity)
                {
                    if (sensitivity < 3 || sensitivity > 10)
                        throw new CommandFailedException("The sensitivity is not in the valid range ([4, 10]).");

                    CachedGuildConfig gcfg = this.Shared.GetGuildConfig(ctx.Guild.Id);
                    gcfg.AntispamSettings.Sensitivity = sensitivity;

                    await this.Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg);

                    DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                    if (logchn != null) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Color = this.ModuleColor
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Antispam sensitivity changed to", $"Max {gcfg.AntispamSettings.Sensitivity} msgs per 5s");
                        await logchn.SendMessageAsync(embed: emb.Build());
                    }

                    await this.InformAsync(ctx, $"Antispam sensitivity for this guild has been changed to {Formatter.Bold(gcfg.AntispamSettings.Sensitivity.ToString())} maximum repeated messages.", important: false);
                }
                #endregion
            }
        }
    }
}
