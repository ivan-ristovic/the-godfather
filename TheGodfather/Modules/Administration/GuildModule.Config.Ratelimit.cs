#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services.Common;
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
            
            public class RatelimitModule : TheGodfatherServiceModule<RatelimitService>
            {

                public RatelimitModule(RatelimitService service, DbContextBuilder db)
                    : base(service, db)
                {
                    
                }


                [GroupCommand, Priority(3)]
                public async Task ExecuteGroupAsync(CommandContext ctx,
                                                   [Description("Enable?")] bool enable,
                                                   [Description("Sensitivity (messages per 5s to trigger action).")] short sensitivity,
                                                   [Description("Action type.")] PunishmentAction action = PunishmentAction.PermanentMute)
                {
                    if (sensitivity < 4 || sensitivity > 10)
                        throw new CommandFailedException("The sensitivity is not in the valid range ([4, 10]).");

                    GuildConfig gcfg = await ctx.Services.GetService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                        cfg.RatelimitEnabled = enable;
                        cfg.RatelimitAction = action;
                        cfg.RatelimitSensitivity = sensitivity;
                    });

                    DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
                    if (!(logchn is null)) {
                        var emb = new DiscordEmbedBuilder {
                            Title = "Guild config changed",
                            Description = $"Ratelimit {(enable ? "enabled" : "disabled")}",
                            Color = this.ModuleColor
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        if (enable) {
                            emb.AddField("Ratelimit sensitivity", gcfg.RatelimitSensitivity.ToString(), inline: true);
                            emb.AddField("Ratelimit action", gcfg.RatelimitAction.ToTypeString(), inline: true);
                        }
                        await logchn.SendMessageAsync(embed: emb.Build());
                    }

                    await this.InformAsync(ctx, $"{Formatter.Bold(gcfg.RatelimitEnabled ? "Enabled" : "Disabled")} ratelimit actions.", important: false);
                }

                [GroupCommand, Priority(2)]
                public Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("Enable?")] bool enable,
                                             [Description("Action type.")] PunishmentAction action,
                                             [Description("Sensitivity (messages per 5s to trigger action).")] short sensitivity = 5)
                    => this.ExecuteGroupAsync(ctx, enable, sensitivity, action);

                [GroupCommand, Priority(1)]
                public Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("Enable?")] bool enable)
                    => this.ExecuteGroupAsync(ctx, enable, 5, PunishmentAction.PermanentMute);

                [GroupCommand, Priority(0)]
                public async Task ExecuteGroupAsync(CommandContext ctx)
                {
                    CachedGuildConfig gcfg = ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);

                    if (gcfg.RatelimitSettings.Enabled) {
                        var sb = new StringBuilder();
                        sb.Append("Sensitivity: ").AppendLine(gcfg.RatelimitSettings.Sensitivity.ToString());
                        sb.Append("Action: ").AppendLine(gcfg.RatelimitSettings.Action.ToString());

                        sb.AppendLine().Append(Formatter.Bold("Exempts:"));

                        List<DatabaseExemptRatelimit> exempted;
                        using (DatabaseContext db = this.Database.CreateContext()) {
                            exempted = await db.RatelimitExempts
                                .Where(ee => ee.GuildId == ctx.Guild.Id)
                                .OrderBy(ee => ee.Type)
                                .ToListAsync();
                        }

                        if (exempted.Any()) {
                            sb.AppendLine();
                            foreach (DatabaseExemptedEntity ee in exempted)
                                sb.AppendLine($"{ee.Type.ToUserFriendlyString()}: {ee.Id}");
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
                
                public async Task SetActionAsync(CommandContext ctx,
                                                [Description("Action type.")] PunishmentAction action)
                {
                    GuildConfig gcfg = await ctx.Services.GetService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                        cfg.RatelimitSettings.Action = action;
                    });

                    DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
                    if (!(logchn is null)) {
                        var emb = new DiscordEmbedBuilder {
                            Title = "Guild config changed",
                            Color = this.ModuleColor
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Ratelimit action changed to", gcfg.RatelimitAction.ToTypeString());
                        await logchn.SendMessageAsync(embed: emb.Build());
                    }

                    await this.InformAsync(ctx, $"Ratelimit action for this guild has been changed to {Formatter.Bold(gcfg.RatelimitAction.ToTypeString())}", important: false);
                }
                #endregion

                #region COMMAND_RATELIMIT_SENSITIVITY
                [Command("sensitivity")]
                [Description("Set the ratelimit sensitivity. Ratelimit will be hit if member sends more messages in 5 seconds than given sensitivity number.")]
                [Aliases("setsensitivity", "setsens", "sens", "s")]
                
                public async Task SetSensitivityAsync(CommandContext ctx,
                                                     [Description("Sensitivity (messages per 5s to trigger action).")] short sensitivity)
                {
                    if (sensitivity < 4 || sensitivity > 10)
                        throw new CommandFailedException("The sensitivity is not in the valid range ([4, 10]).");

                    GuildConfig gcfg = await ctx.Services.GetService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                        cfg.RatelimitSettings.Sensitivity = sensitivity;
                    });

                    DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
                    if (!(logchn is null)) {
                        var emb = new DiscordEmbedBuilder {
                            Title = "Guild config changed",
                            Color = this.ModuleColor
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Ratelimit sensitivity changed to", $"Max {gcfg.RatelimitSensitivity} msgs per 5s");
                        await logchn.SendMessageAsync(embed: emb.Build());
                    }

                    await this.InformAsync(ctx, $"Ratelimit sensitivity for this guild has been changed to {Formatter.Bold(gcfg.RatelimitSensitivity.ToString())} msgs per 5s", important: false);
                }
                #endregion

                #region COMMAND_RATELIMIT_EXEMPT
                [Command("exempt"), Priority(2)]
                [Description("Disable the ratelimit watch for some entities (users, channels, etc).")]
                [Aliases("ex", "exc")]
                
                public async Task ExemptAsync(CommandContext ctx,
                                             [Description("Members to exempt.")] params DiscordMember[] members)
                {
                    if (members is null || !members.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    using (DatabaseContext db = this.Database.CreateContext()) {
                        db.RatelimitExempts.AddExemptions(ctx.Guild.Id, members, ExemptedEntityType.Member);
                        await db.SaveChangesAsync();
                    }

                    this.Service.UpdateExemptsForGuildAsync(ctx.Guild.Id);
                    await this.InformAsync(ctx, "Successfully exempted given users.", important: false);
                }

                [Command("exempt"), Priority(1)]
                public async Task ExemptAsync(CommandContext ctx,
                                             [Description("Roles to exempt.")] params DiscordRole[] roles)
                {
                    if (roles is null || !roles.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    using (DatabaseContext db = this.Database.CreateContext()) {
                        db.RatelimitExempts.AddExemptions(ctx.Guild.Id, roles, ExemptedEntityType.Role);
                        await db.SaveChangesAsync();
                    }

                    this.Service.UpdateExemptsForGuildAsync(ctx.Guild.Id);
                    await this.InformAsync(ctx, "Successfully exempted given roles.", important: false);
                }
                
                [Command("exempt"), Priority(0)]
                public async Task ExemptAsync(CommandContext ctx,
                                             [Description("Channels to exempt.")] params DiscordChannel[] channels)
                {
                    if (channels is null || !channels.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    using (DatabaseContext db = this.Database.CreateContext()) {
                        db.RatelimitExempts.AddExemptions(ctx.Guild.Id, channels, ExemptedEntityType.Channel);
                        await db.SaveChangesAsync();
                    }

                    this.Service.UpdateExemptsForGuildAsync(ctx.Guild.Id);
                    await this.InformAsync(ctx, "Successfully exempted given channels.", important: false);
                }
                #endregion

                #region COMMAND_RATELIMIT_UNEXEMPT
                [Command("unexempt"), Priority(2)]
                [Description("Remove an exempted entity and allow ratelimit watch for that entity.")]
                [Aliases("unex", "uex")]
                
                public async Task UnxemptAsync(CommandContext ctx,
                                              [Description("Members to unexempt.")] params DiscordMember[] members)
                {
                    if (members is null || !members.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    using (DatabaseContext db = this.Database.CreateContext()) {
                        db.RatelimitExempts.RemoveRange(
                            db.RatelimitExempts.Where(ex => ex.GuildId == ctx.Guild.Id && ex.Type == ExemptedEntityType.Member && members.Any(m => m.Id == ex.Id))
                        );
                        await db.SaveChangesAsync();
                    }

                    this.Service.UpdateExemptsForGuildAsync(ctx.Guild.Id);
                    await this.InformAsync(ctx, $"Successfully unexempted given users.", important: false);
                }

                [Command("unexempt"), Priority(1)]
                public async Task UnxemptAsync(CommandContext ctx,
                                              [Description("Roles to unexempt.")] params DiscordRole[] roles)
                {
                    if (roles is null || !roles.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    using (DatabaseContext db = this.Database.CreateContext()) {
                        db.RatelimitExempts.RemoveRange(
                            db.RatelimitExempts.Where(ex => ex.GuildId == ctx.Guild.Id && ex.Type == ExemptedEntityType.Role && roles.Any(r => r.Id == ex.Id))
                        );
                        await db.SaveChangesAsync();
                    }

                    this.Service.UpdateExemptsForGuildAsync(ctx.Guild.Id);
                    await this.InformAsync(ctx, $"Successfully unexempted given roles.", important: false);
                }

                [Command("unexempt"), Priority(0)]
                public async Task UnxemptAsync(CommandContext ctx,
                                              [Description("Channels to unexempt.")] params DiscordChannel[] channels)
                {
                    if (channels is null || !channels.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    using (DatabaseContext db = this.Database.CreateContext()) {
                        db.RatelimitExempts.RemoveRange(
                            db.RatelimitExempts.Where(ex => ex.GuildId == ctx.Guild.Id && ex.Type == ExemptedEntityType.Channel && channels.Any(c => c.Id == ex.Id))
                        );
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
