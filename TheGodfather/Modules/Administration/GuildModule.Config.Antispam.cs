#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
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

                public AntispamModule(AntispamService service, SharedData shared, DatabaseContextBuilder db)
                    : base(service, shared, db)
                {
                    this.ModuleColor = DiscordColor.DarkRed;
                }


                [GroupCommand, Priority(3)]
                public async Task ExecuteGroupAsync(CommandContext ctx,
                                                   [Description("Enable?")] bool enable,
                                                   [Description("Sensitivity (max repeated messages).")] short sensitivity,
                                                   [Description("Action type.")] PunishmentActionType action = PunishmentActionType.TemporaryMute)
                {
                    if (sensitivity < 3 || sensitivity > 10)
                        throw new CommandFailedException("The sensitivity is not in the valid range ([3, 10]).");

                    DatabaseGuildConfig gcfg = await this.ModifyGuildConfigAsync(ctx.Guild.Id, cfg => {
                        cfg.AntispamEnabled = enable;
                        cfg.AntispamAction = action;
                        cfg.AntispamSensitivity = sensitivity;
                    });

                    DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                    if (!(logchn is null)) {
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
                    => this.ExecuteGroupAsync(ctx, enable, 5, PunishmentActionType.TemporaryMute);

                [GroupCommand, Priority(0)]
                public async Task ExecuteGroupAsync(CommandContext ctx)
                {
                    CachedGuildConfig gcfg = this.Shared.GetGuildConfig(ctx.Guild.Id);

                    if (gcfg.AntispamSettings.Enabled) {
                        var sb = new StringBuilder();
                        sb.Append(Formatter.Bold("Sensitivity: ")).AppendLine(gcfg.AntispamSettings.Sensitivity.ToString());
                        sb.Append(Formatter.Bold("Action: ")).AppendLine(gcfg.AntispamSettings.Action.ToString());
                        
                        sb.AppendLine().Append(Formatter.Bold("Exempts:"));

                        List<DatabaseExemptAntispam> exempted;
                        using (DatabaseContext db = this.Database.CreateContext()) {
                            exempted = await db.AntispamExempts
                                .Where(ee => ee.GuildId == ctx.Guild.Id)
                                .OrderBy(ee => ee.Type)
                                .ToListAsync();
                        }

                        if (exempted.Any()) {
                            sb.AppendLine();
                            foreach (DatabaseExemptAntispam ee in exempted)
                                sb.AppendLine($"{ee.Type.ToUserFriendlyString()}: {ee.Id}");
                        } else {
                            sb.Append(" None");
                        }

                        await this.InformAsync(ctx, $"Antispam watch for this guild is {Formatter.Bold("enabled")}\n\n{sb.ToString()}");
                    } else {
                        await this.InformAsync(ctx, $"Antispam watch for this guild is {Formatter.Bold("disabled")}");
                    }
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
                    DatabaseGuildConfig gcfg = await this.ModifyGuildConfigAsync(ctx.Guild.Id, cfg => {
                        cfg.AntispamSettings.Action = action;
                    });

                    DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                    if (!(logchn is null)) {
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

                    DatabaseGuildConfig gcfg = await this.ModifyGuildConfigAsync(ctx.Guild.Id, cfg => {
                        cfg.AntispamSettings.Sensitivity = sensitivity;
                    });

                    DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                    if (!(logchn is null)) {
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

                #region COMMAND_ANTISPAM_EXEMPT
                [Command("exempt"), Priority(2)]
                [Description("Disable the antispam watch for some entities (users, channels, etc).")]
                [Aliases("ex", "exc")]
                [UsageExamples("!guild cfg antispam exempt @Someone",
                               "!guild cfg antispam exempt #spam",
                               "!guild cfg antispam exempt Role")]
                public async Task ExemptAsync(CommandContext ctx,
                                             [Description("Users to exempt.")] params DiscordUser[] users)
                {
                    if (!users.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    using (DatabaseContext db = this.Database.CreateContext()) {
                        db.AntispamExempts.AddRange(users.Select(u => new DatabaseExemptAntispam() {
                            GuildId = ctx.Guild.Id,
                            Id = u.Id,
                            Type = ExemptedEntityType.Member
                        }));
                        await db.SaveChangesAsync();
                    }

                    this.Service.UpdateExemptsForGuildAsync(ctx.Guild.Id);
                    await this.InformAsync(ctx, "Successfully exempted given users.", important: false);
                }

                [Command("exempt"), Priority(1)]
                public async Task ExemptAsync(CommandContext ctx,
                                             [Description("Roles to exempt.")] params DiscordRole[] roles)
                {
                    if (!roles.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    using (DatabaseContext db = this.Database.CreateContext()) {
                        db.AntispamExempts.AddRange(roles.Select(r => new DatabaseExemptAntispam() {
                            GuildId = ctx.Guild.Id,
                            Id = r.Id,
                            Type = ExemptedEntityType.Role
                        }));
                        await db.SaveChangesAsync();
                    }

                    this.Service.UpdateExemptsForGuildAsync(ctx.Guild.Id);
                    await this.InformAsync(ctx, "Successfully exempted given roles.", important: false);
                }

                [Command("exempt"), Priority(0)]
                public async Task ExemptAsync(CommandContext ctx,
                                             [Description("Channels to exempt.")] params DiscordChannel[] channels)
                {
                    if (!channels.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    using (DatabaseContext db = this.Database.CreateContext()) {
                        db.AntispamExempts.AddRange(channels.Select(c => new DatabaseExemptAntispam() {
                            GuildId = ctx.Guild.Id,
                            Id = c.Id,
                            Type = ExemptedEntityType.Channel
                        }));
                        await db.SaveChangesAsync();
                    }

                    this.Service.UpdateExemptsForGuildAsync(ctx.Guild.Id);
                    await this.InformAsync(ctx, "Successfully exempted given channels.", important: false);
                }
                #endregion

                #region COMMAND_ANTISPAM_UNEXEMPT
                [Command("unexempt"), Priority(2)]
                [Description("Remove an exempted entity and allow antispam watch for that entity.")]
                [Aliases("unex", "uex")]
                [UsageExamples("!guild cfg antispam unexempt @Someone",
                               "!guild cfg antispam unexempt #spam",
                               "!guild cfg antispam unexempt Category")]
                public async Task UnxemptAsync(CommandContext ctx,
                                              [Description("Users to unexempt.")] params DiscordUser[] users)
                {
                    if (!users.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    using (DatabaseContext db = this.Database.CreateContext()) {
                        db.AntispamExempts.RemoveRange(users.Select(u => new DatabaseExemptAntispam() {
                            GuildId = ctx.Guild.Id,
                            Id = u.Id,
                            Type = ExemptedEntityType.Member
                        }));
                        await db.SaveChangesAsync();
                    }

                    this.Service.UpdateExemptsForGuildAsync(ctx.Guild.Id);
                    await this.InformAsync(ctx, $"Successfully unexempted given users.", important: false);
                }

                [Command("unexempt"), Priority(1)]
                public async Task UnxemptAsync(CommandContext ctx,
                                              [Description("Roles to unexempt.")] params DiscordRole[] roles)
                {
                    if (!roles.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    using (DatabaseContext db = this.Database.CreateContext()) {
                        db.AntispamExempts.RemoveRange(roles.Select(r => new DatabaseExemptAntispam() {
                            GuildId = ctx.Guild.Id,
                            Id = r.Id,
                            Type = ExemptedEntityType.Role
                        }));
                        await db.SaveChangesAsync();
                    }
                    this.Service.UpdateExemptsForGuildAsync(ctx.Guild.Id);
                    await this.InformAsync(ctx, $"Successfully unexempted given roles.", important: false);
                }

                [Command("unexempt"), Priority(0)]
                public async Task UnxemptAsync(CommandContext ctx,
                                              [Description("Channels to unexempt.")] params DiscordChannel[] channels)
                {
                    if (!channels.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    using (DatabaseContext db = this.Database.CreateContext()) {
                        db.AntispamExempts.RemoveRange(channels.Select(c => new DatabaseExemptAntispam() {
                            GuildId = ctx.Guild.Id,
                            Id = c.Id,
                            Type = ExemptedEntityType.Channel
                        }));
                        await db.SaveChangesAsync();
                    }

                    this.Service.UpdateExemptsForGuildAsync(ctx.Guild.Id);
                    await this.InformAsync(ctx, $"Successfully unexempted given channel.", important: false);
                }
                #endregion
            }
        }
    }
}
