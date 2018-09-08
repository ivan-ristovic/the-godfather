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
            [UsageExamples("!guild cfg logging",
                           "!guild cfg logging on #log",
                           "!guild cfg logging off")]
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
                    if (logchn != null) {
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
                        IReadOnlyList<ExemptedEntity> exempted = await this.Database.GetAllExemptedEntitiesAsync(ctx.Guild.Id);
                        var sb = new StringBuilder();
                        foreach (ExemptedEntity exempt in exempted.OrderBy(e => e.Type))
                            sb.AppendLine($"{exempt.Type.ToUserFriendlyString()} exempted: {exempt.Id}");

                        await this.InformAsync(ctx, $"Action logging for this guild is {Formatter.Bold("enabled")} at {ctx.Guild.GetChannel(gcfg.LogChannelId)?.Mention ?? "(unknown)"}!\n{sb.ToString()}");
                    } else {
                        await this.InformAsync(ctx, $"Action logging for this guild is {Formatter.Bold("disabled")}!");
                    }
                }


                #region COMMAND_LOG_EXEMPT
                [Command("exempt"), Priority(2)]
                [Description("Disable the logs for some entities (users, channels, etc).")]
                [Aliases("ex", "exc")]
                [UsageExamples("!guild cfg exempt @Someone",
                               "!guild cfg exempt #spam",
                               "!guild cfg exempt Category")]
                public async Task ExemptAsync(CommandContext ctx,
                                             [Description("User to exempt.")] DiscordUser user)
                {
                    await this.Database.ExemptAsync(ctx.Guild.Id, user.Id, EntityType.Member);
                    await this.InformAsync(ctx, $"Successfully exempted user {Formatter.Bold(user.Username)}", important: false);
                }

                [Command("exempt"), Priority(1)]
                public async Task ExemptAsync(CommandContext ctx,
                                             [Description("Role to exempt.")] DiscordRole role)
                {
                    await this.Database.ExemptAsync(ctx.Guild.Id, role.Id, EntityType.Role);
                    await this.InformAsync(ctx, $"Successfully exempted role {Formatter.Bold(role.Name)}", important: false);
                }

                [Command("exempt"), Priority(0)]
                public async Task ExemptAsync(CommandContext ctx,
                                             [Description("Channel to exempt.")] DiscordChannel channel = null)
                {
                    channel = channel ?? ctx.Channel;
                    await this.Database.ExemptAsync(ctx.Guild.Id, channel.Id, EntityType.Channel);
                    await this.InformAsync(ctx, $"Successfully exempted channel {Formatter.Bold(channel.Name)}", important: false);
                }
                #endregion

                #region COMMAND_LOG_UNEXEMPT
                [Command("unexempt"), Priority(2)]
                [Description("Remove an exempted entity and allow logging for actions regarding that entity.")]
                [Aliases("unex", "uex")]
                [UsageExamples("!guild cfg unexempt @Someone",
                               "!guild cfg unexempt #spam",
                               "!guild cfg unexempt Category")]
                public async Task UnxemptAsync(CommandContext ctx,
                                              [Description("User to unexempt.")] DiscordUser user)
                {
                    await this.Database.UnexemptAsync(ctx.Guild.Id, user.Id, EntityType.Member);
                    await this.InformAsync(ctx, $"Successfully unexempted user {Formatter.Bold(user.Username)}", important: false);
                }

                [Command("unexempt"), Priority(1)]
                public async Task UnxemptAsync(CommandContext ctx,
                                              [Description("Role to unexempt.")] DiscordRole role)
                {
                    await this.Database.UnexemptAsync(ctx.Guild.Id, role.Id, EntityType.Role);
                    await this.InformAsync(ctx, $"Successfully unexempted role {Formatter.Bold(role.Name)}", important: false);
                }

                [Command("unexempt"), Priority(0)]
                public async Task UnxemptAsync(CommandContext ctx,
                                              [Description("Channel to unexempt.")] DiscordChannel channel = null)
                {
                    channel = channel ?? ctx.Channel;
                    await this.Database.UnexemptAsync(ctx.Guild.Id, channel.Id, EntityType.Channel);
                    await this.InformAsync(ctx, $"Successfully unexempted channel {Formatter.Bold(channel.Name)}", important: false);
                }
                #endregion
            }
        }
    }
}
