#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System.Threading.Tasks;

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
            [Group("instantleave")]
            [Description("Automatically bans users which leave in certain timespan after joining.")]
            [Aliases("joinleave", "instaleave", "il", "jl")]
            [UsageExamples("!guild cfg instaleave",
                           "!guild cfg instaleave on",
                           "!guild cfg instaleave on 5")]
            public class InstantLeaveModule : TheGodfatherServiceModule<AntiInstantLeaveService>
            {

                public InstantLeaveModule(AntiInstantLeaveService service, SharedData shared, DBService db)
                    : base(service, shared, db)
                {
                    this.ModuleColor = DiscordColor.IndianRed;
                }


                [GroupCommand, Priority(2)]
                public async Task ExecuteGroupAsync(CommandContext ctx,
                                                   [Description("Enable?")] bool enable,
                                                   [Description("Sensitivity (join-leave max seconds).")] short sensitivity)
                {
                    if (sensitivity < 2 || sensitivity > 60)
                        throw new CommandFailedException("The sensitivity is not in the valid range ([2, 60]).");

                    AntiInstantLeaveSettings settings = await this.Database.GetAntiInstantLeaveSettingsAsync(ctx.Guild.Id);
                    settings.Enabled = enable;
                    settings.Sensitivity = sensitivity;

                    DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                    if (logchn != null) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Description = $"AntiInstantLeave {(enable ? "enabled" : "disabled")}",
                            Color = this.ModuleColor
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        if (enable) {
                            emb.AddField("Instant leave sensitivity", settings.Sensitivity.ToString(), inline: true);
                        }
                        await logchn.SendMessageAsync(embed: emb.Build());
                    }

                    await this.InformAsync(ctx, $"{Formatter.Bold(enable ? "Enabled" : "Disabled")} instant leave actions.", important: false);
                }

                [GroupCommand, Priority(1)]
                public Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("Enable?")] bool enable)
                    => this.ExecuteGroupAsync(ctx, enable, 3);

                [GroupCommand, Priority(0)]
                public async Task ExecuteGroupAsync(CommandContext ctx)
                {
                    AntiInstantLeaveSettings settings = await this.Database.GetAntiInstantLeaveSettingsAsync(ctx.Guild.Id);
                    await this.InformAsync(ctx, $"Instant leave watch for this guild is {Formatter.Bold(settings.Enabled ? "enabled" : "disabled")} with senssitivity: {Formatter.Bold(settings.Sensitivity.ToString())}");
                }

                
                #region COMMAND_INSTANTLEAVE_SENSITIVITY
                [Command("sensitivity")]
                [Description("Set the instant leave sensitivity. User will be banned if he leaves in the given sensitivity time window (in seconds).")]
                [Aliases("setsensitivity", "setsens", "sens", "s")]
                [UsageExamples("!guild cfg instaleave sensitivity 9")]
                public async Task SetSensitivityAsync(CommandContext ctx,
                                                     [Description("Sensitivity.")] short sensitivity)
                {
                    if (sensitivity < 2 || sensitivity > 60)
                        throw new CommandFailedException("The sensitivity is not in the valid range ([2, 60]).");

                    AntiInstantLeaveSettings settings = await this.Database.GetAntiInstantLeaveSettingsAsync(ctx.Guild.Id);
                    settings.Sensitivity = sensitivity;

                    DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                    if (logchn != null) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Color = this.ModuleColor
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Instant leave sensitivity changed to", $"{settings.Sensitivity}s");
                        await logchn.SendMessageAsync(embed: emb.Build());
                    }

                    await this.InformAsync(ctx, $"Instant leave sensitivity for this guild has been changed to {Formatter.Bold(settings.Sensitivity.ToString())}s", important: false);
                }
                #endregion
            }
        }
    }
}
