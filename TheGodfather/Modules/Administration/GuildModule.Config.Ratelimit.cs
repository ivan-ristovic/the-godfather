#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Administration
{
    public partial class GuildModule
    {
        public partial class GuildConfigModule
        {
            [Group("ratelimit")]
            [Description("Prevents users from posting more than specified amount of messages in 5s.")]
            [Aliases("rl", "rate")]
            [UsageExamples("!guild cfg ratelimit",
                           "!guild cfg ratelimit on",
                           "!guild cfg ratelimit on mute",
                           "!guild cfg ratelimit on 5",
                           "!guild cfg ratelimit on 6 kick")]
            public class RatelimitModule : TheGodfatherServiceModule<RatelimitService>
            {

                public RatelimitModule(RatelimitService service, SharedData shared, DBService db)
                    : base(service, shared, db)
                {
                    this.ModuleColor = DiscordColor.Rose;
                }


                [GroupCommand, Priority(3)]
                public async Task ExecuteGroupAsync(CommandContext ctx,
                                                   [Description("Enable?")] bool enable,
                                                   [Description("Sensitivity (messages per 5s to trigger action).")] short sensitivity,
                                                   [Description("Action type.")] PunishmentActionType action = PunishmentActionType.Mute)
                {
                    if (sensitivity < 4 || sensitivity > 10)
                        throw new CommandFailedException("The sensitivity is not in the valid range ([4, 10]).");

                    CachedGuildConfig gcfg = this.Shared.GetGuildConfig(ctx.Guild.Id);
                    gcfg.RatelimitSettings.Enabled = enable;
                    gcfg.RatelimitSettings.Action = action;
                    gcfg.RatelimitSettings.Sensitivity = sensitivity;

                    await this.Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg);

                    DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                    if (!(logchn is null)) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Description = $"Ratelimit {(enable ? "enabled" : "disabled")}",
                            Color = this.ModuleColor
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        if (enable) {
                            emb.AddField("Ratelimit sensitivity", gcfg.RatelimitSettings.Sensitivity.ToString(), inline: true);
                            emb.AddField("Ratelimit action", gcfg.RatelimitSettings.Action.ToTypeString(), inline: true);
                        }
                        await logchn.SendMessageAsync(embed: emb.Build());
                    }

                    await this.InformAsync(ctx, $"{Formatter.Bold(gcfg.RatelimitSettings.Enabled ? "Enabled" : "Disabled")} ratelimit actions.", important: false);
                }

                [GroupCommand, Priority(2)]
                public Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("Enable?")] bool enable,
                                             [Description("Action type.")] PunishmentActionType action,
                                             [Description("Sensitivity (messages per 5s to trigger action).")] short sensitivity = 5)
                    => this.ExecuteGroupAsync(ctx, enable, sensitivity, action);

                [GroupCommand, Priority(1)]
                public Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("Enable?")] bool enable)
                    => this.ExecuteGroupAsync(ctx, enable, 5, PunishmentActionType.Mute);

                [GroupCommand, Priority(0)]
                public async Task ExecuteGroupAsync(CommandContext ctx)
                {
                    CachedGuildConfig gcfg = this.Shared.GetGuildConfig(ctx.Guild.Id);

                    if (gcfg.RatelimitSettings.Enabled) {
                        var sb = new StringBuilder();
                        sb.Append("Sensitivity: ").AppendLine(gcfg.RatelimitSettings.Sensitivity.ToString());
                        sb.Append("Action: ").AppendLine(gcfg.RatelimitSettings.Action.ToString());

                        sb.AppendLine().Append(Formatter.Bold("Exempts:"));
                        IReadOnlyList<ExemptedEntity> exempted = await this.Database.GetAllRatelimitExemptsAsync(ctx.Guild.Id);
                        if (exempted.Any()) {
                            sb.AppendLine();
                            foreach (ExemptedEntity exempt in exempted.OrderBy(e => e.Type))
                                sb.AppendLine($"{exempt.Type.ToUserFriendlyString()}: {exempt.Id}");
                        } else {
                            sb.Append(" None");
                        }

                        await this.InformAsync(ctx, $"Ratelimit watch for this guild is {Formatter.Bold("enabled")}\n{sb.ToString()}");
                    } else {
                        await this.InformAsync(ctx, $"Ratelimit watch for this guild is {Formatter.Bold("disabled")}");
                    }
                }


                #region COMMAND_RATELIMIT_ACTION
                [Command("action")]
                [Description("Set the action to execute when the ratelimit is hit.")]
                [Aliases("setaction", "a")]
                [UsageExamples("!guild cfg ratelimit action mute",
                               "!guild cfg ratelimit action temporaryban")]
                public async Task SetActionAsync(CommandContext ctx,
                                                [Description("Action type.")] PunishmentActionType action)
                {
                    CachedGuildConfig gcfg = this.Shared.GetGuildConfig(ctx.Guild.Id);
                    gcfg.RatelimitSettings.Action = action;

                    await this.Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg);

                    DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                    if (!(logchn is null)) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Color = this.ModuleColor
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Ratelimit action changed to", gcfg.RatelimitSettings.Action.ToTypeString());
                        await logchn.SendMessageAsync(embed: emb.Build());
                    }

                    await this.InformAsync(ctx, $"Ratelimit action for this guild has been changed to {Formatter.Bold(gcfg.RatelimitSettings.Action.ToTypeString())}", important: false);
                }
                #endregion

                #region COMMAND_RATELIMIT_SENSITIVITY
                [Command("sensitivity")]
                [Description("Set the ratelimit sensitivity. Ratelimit will be hit if member sends more messages in 5 seconds than given sensitivity number.")]
                [Aliases("setsensitivity", "setsens", "sens", "s")]
                [UsageExamples("!guild cfg ratelimit sensitivity 9")]
                public async Task SetSensitivityAsync(CommandContext ctx,
                                                     [Description("Sensitivity (messages per 5s to trigger action).")] short sensitivity)
                {
                    if (sensitivity < 4 || sensitivity > 10)
                        throw new CommandFailedException("The sensitivity is not in the valid range ([4, 10]).");

                    CachedGuildConfig gcfg = this.Shared.GetGuildConfig(ctx.Guild.Id);
                    gcfg.RatelimitSettings.Sensitivity = sensitivity;

                    await this.Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg);

                    DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                    if (!(logchn is null)) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Color = this.ModuleColor
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Ratelimit sensitivity changed to", $"Max {gcfg.RatelimitSettings.Sensitivity} msgs per 5s");
                        await logchn.SendMessageAsync(embed: emb.Build());
                    }

                    await this.InformAsync(ctx, $"Ratelimit sensitivity for this guild has been changed to {Formatter.Bold(gcfg.RatelimitSettings.Sensitivity.ToString())} msgs per 5s", important: false);
                }
                #endregion

                #region COMMAND_RATELIMIT_EXEMPT
                [Command("exempt"), Priority(2)]
                [Description("Disable the ratelimit watch for some entities (users, channels, etc).")]
                [Aliases("ex", "exc")]
                [UsageExamples("!guild cfg ratelimit exempt @Someone",
                               "!guild cfg ratelimit exempt #spam",
                               "!guild cfg ratelimit exempt Role")]
                public async Task ExemptAsync(CommandContext ctx,
                                             [Description("Users to exempt.")] params DiscordUser[] users)
                {
                    if (!users.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    foreach (DiscordUser user in users)
                        await this.Database.ExemptRatelimitAsync(ctx.Guild.Id, user.Id, ExemptedEntityType.Member);

                    await this.Service.UpdateExemptsForGuildAsync(ctx.Guild.Id);
                    await this.InformAsync(ctx, "Successfully exempted given users.", important: false);
                }

                [Command("exempt"), Priority(1)]
                public async Task ExemptAsync(CommandContext ctx,
                                             [Description("Roles to exempt.")] params DiscordRole[] roles)
                {
                    if (!roles.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    foreach (DiscordRole role in roles)
                        await this.Database.ExemptRatelimitAsync(ctx.Guild.Id, role.Id, ExemptedEntityType.Role);

                    await this.Service.UpdateExemptsForGuildAsync(ctx.Guild.Id);
                    await this.InformAsync(ctx, "Successfully exempted given roles.", important: false);
                }
                
                [Command("exempt"), Priority(0)]
                public async Task ExemptAsync(CommandContext ctx,
                                             [Description("Channels to exempt.")] params DiscordChannel[] channels)
                {
                    if (!channels.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    foreach (DiscordChannel channel in channels)
                        await this.Database.ExemptRatelimitAsync(ctx.Guild.Id, channel.Id, ExemptedEntityType.Channel);

                    await this.Service.UpdateExemptsForGuildAsync(ctx.Guild.Id);
                    await this.InformAsync(ctx, "Successfully exempted given channels.", important: false);
                }
                #endregion

                #region COMMAND_ANTISPAM_UNEXEMPT
                [Command("unexempt"), Priority(2)]
                [Description("Remove an exempted entity and allow ratelimit watch for that entity.")]
                [Aliases("unex", "uex")]
                [UsageExamples("!guild cfg ratelimit unexempt @Someone",
                               "!guild cfg ratelimit unexempt #spam",
                               "!guild cfg ratelimit unexempt Category")]
                public async Task UnxemptAsync(CommandContext ctx,
                                              [Description("Users to unexempt.")] params DiscordUser[] users)
                {
                    if (!users.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    foreach (DiscordUser user in users)
                        await this.Database.UnexemptRatelimitAsync(ctx.Guild.Id, user.Id, ExemptedEntityType.Member);

                    await this.Service.UpdateExemptsForGuildAsync(ctx.Guild.Id);
                    await this.InformAsync(ctx, $"Successfully unexempted given users.", important: false);
                }

                [Command("unexempt"), Priority(1)]
                public async Task UnxemptAsync(CommandContext ctx,
                                              [Description("Roles to unexempt.")] params DiscordRole[] roles)
                {
                    if (!roles.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    foreach (DiscordRole role in roles)
                        await this.Database.UnexemptRatelimitAsync(ctx.Guild.Id, role.Id, ExemptedEntityType.Role);

                    await this.Service.UpdateExemptsForGuildAsync(ctx.Guild.Id);
                    await this.InformAsync(ctx, $"Successfully unexempted given roles.", important: false);
                }

                [Command("unexempt"), Priority(0)]
                public async Task UnxemptAsync(CommandContext ctx,
                                              [Description("Channels to unexempt.")] params DiscordChannel[] channels)
                {
                    if (!channels.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    foreach (DiscordChannel channel in channels)
                        await this.Database.UnexemptRatelimitAsync(ctx.Guild.Id, channel.Id, ExemptedEntityType.Channel);

                    await this.Service.UpdateExemptsForGuildAsync(ctx.Guild.Id);
                    await this.InformAsync(ctx, $"Successfully unexempted given channel.", important: false);
                }
                #endregion
            }
        }
    }
}
