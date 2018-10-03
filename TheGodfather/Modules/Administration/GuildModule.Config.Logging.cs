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
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Administration
{
    public partial class GuildModule
    {
        public partial class GuildConfigModule
        {
            [Group("logging")]
            [Description("Action logging configuration.")]
            [Aliases("log", "modlog")]
            [UsageExamples("!guild cfg log",
                           "!guild cfg log on #log",
                           "!guild cfg log off")]
            public class LoggingModule : TheGodfatherModule
            {

                public LoggingModule(SharedData shared, DBService db)
                    : base(shared, db)
                {
                    this.ModuleColor = DiscordColor.DarkRed;
                }


                [GroupCommand, Priority(1)]
                public async Task ExecuteGroupAsync(CommandContext ctx,
                                                   [Description("Enable?")] bool enable,
                                                   [Description("Log channel.")] DiscordChannel channel = null)
                {
                    channel = channel ?? ctx.Channel;

                    if (channel.Type != ChannelType.Text)
                        throw new CommandFailedException("Action logging channel must be a text channel.");

                    CachedGuildConfig gcfg = this.Shared.GetGuildConfig(ctx.Guild.Id);
                    gcfg.LogChannelId = enable ? channel.Id : 0;

                    await this.Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg);

                    DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                    if (!(logchn is null)) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Color = this.ModuleColor
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Logging channel set to", gcfg.LogChannelId.ToString(), inline: true);
                        await logchn.SendMessageAsync(embed: emb.Build());
                    }

                    await this.InformAsync(ctx, $"{Formatter.Bold(gcfg.LoggingEnabled ? "Enabled" : "Disabled")} action logs.", important: false);
                }

                [GroupCommand, Priority(0)]
                public async Task ExecuteGroupAsync(CommandContext ctx)
                {
                    CachedGuildConfig gcfg = this.Shared.GetGuildConfig(ctx.Guild.Id);
                    if (gcfg.LoggingEnabled) {
                        var sb = new StringBuilder();
                        sb.Append(Formatter.Bold("Exempts:"));
                        IReadOnlyList<ExemptedEntity> exempted = await this.Database.GetAllLoggingExemptsAsync(ctx.Guild.Id);
                        if (exempted.Any()) {
                            sb.AppendLine();
                            foreach (ExemptedEntity exempt in exempted.OrderBy(e => e.Type))
                                sb.AppendLine($"{exempt.Type.ToUserFriendlyString()}: {exempt.Id}");
                        } else {
                            sb.Append(" None");
                        }
                        await this.InformAsync(ctx, $"Action logging for this guild is {Formatter.Bold("enabled")} at {ctx.Guild.GetChannel(gcfg.LogChannelId)?.Mention ?? "(unknown)"}!\n\n{sb.ToString()}");
                    } else {
                        await this.InformAsync(ctx, $"Action logging for this guild is {Formatter.Bold("disabled")}!");
                    }
                }


                #region COMMAND_LOG_EXEMPT
                [Command("exempt"), Priority(2)]
                [Description("Disable the logs for some entities (users, channels, etc).")]
                [Aliases("ex", "exc")]
                [UsageExamples("!guild cfg log exempt @Someone",
                               "!guild cfg log exempt #spam",
                               "!guild cfg log exempt Role")]
                public async Task ExemptAsync(CommandContext ctx,
                                             [Description("Users to exempt.")] params DiscordUser[] users)
                {
                    if (!users.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    foreach (DiscordUser user in users)
                        await this.Database.ExemptLoggingAsync(ctx.Guild.Id, user.Id, EntityType.Member);

                    await this.InformAsync(ctx, "Successfully exempted given users.", important: false);
                }

                [Command("exempt"), Priority(1)]
                public async Task ExemptAsync(CommandContext ctx,
                                             [Description("Roles to exempt.")] params DiscordRole[] roles)
                {
                    if (!roles.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    foreach (DiscordRole role in roles)
                        await this.Database.ExemptLoggingAsync(ctx.Guild.Id, role.Id, EntityType.Role);

                    await this.InformAsync(ctx, "Successfully exempted given roles.", important: false);
                }

                [Command("exempt"), Priority(0)]
                public async Task ExemptAsync(CommandContext ctx,
                                             [Description("Channels to exempt.")] params DiscordChannel[] channels)
                {
                    if (!channels.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    foreach (DiscordChannel channel in channels)
                        await this.Database.ExemptLoggingAsync(ctx.Guild.Id, channel.Id, EntityType.Channel);

                    await this.InformAsync(ctx, "Successfully exempted given channels.", important: false);
                }
                #endregion

                #region COMMAND_LOG_UNEXEMPT
                [Command("unexempt"), Priority(2)]
                [Description("Remove an exempted entity and allow logging for actions regarding that entity.")]
                [Aliases("unex", "uex")]
                [UsageExamples("!guild cfg log unexempt @Someone",
                               "!guild cfg log unexempt #spam",
                               "!guild cfg log unexempt Role")]
                public async Task UnxemptAsync(CommandContext ctx,
                                              [Description("User to unexempt.")] params DiscordUser[] users)
                {
                    if (!users.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    foreach (DiscordUser user in users)
                        await this.Database.UnexemptLoggingAsync(ctx.Guild.Id, user.Id, EntityType.Member);

                    await this.InformAsync(ctx, "Successfully unexempted given users.", important: false);
                }

                [Command("unexempt"), Priority(1)]
                public async Task UnxemptAsync(CommandContext ctx,
                                              [Description("Roles to unexempt.")] params DiscordRole[] roles)
                {
                    if (!roles.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    foreach (DiscordRole role in roles)
                        await this.Database.UnexemptLoggingAsync(ctx.Guild.Id, role.Id, EntityType.Role);

                    await this.InformAsync(ctx, "Successfully unexempted given roles.", important: false);
                }

                [Command("unexempt"), Priority(0)]
                public async Task UnxemptAsync(CommandContext ctx,
                                              [Description("Channels to unexempt.")] params DiscordChannel[] channels)
                {
                    if (!channels.Any())
                        throw new CommandFailedException("You need to provide users or channels or roles to exempt.");

                    foreach (DiscordChannel channel in channels)
                        await this.Database.UnexemptLoggingAsync(ctx.Guild.Id, channel.Id, EntityType.Channel);

                    await this.InformAsync(ctx, "Successfully unexempted given channels.", important: false);
                }
                #endregion
            }
        }
    }
}
