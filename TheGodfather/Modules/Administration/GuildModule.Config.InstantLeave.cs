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
                    settings.Cooldown = sensitivity;

                    await this.Database.SetAntiInstantLeaveSettingsAsync(ctx.Guild.Id, settings);

                    DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                    if (!(logchn is null)) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Description = $"AntiInstantLeave {(enable ? "enabled" : "disabled")}",
                            Color = this.ModuleColor
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        if (enable) {
                            emb.AddField("Instant leave sensitivity", settings.Cooldown.ToString(), inline: true);
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
                    if (settings.Enabled)
                        await this.InformAsync(ctx, $"Instant leave watch: {Formatter.Bold("enabled")} with {Formatter.Bold(settings.Cooldown.ToString())}s cooldown");
                    else
                        await this.InformAsync(ctx, $"Instant leave watch: {Formatter.Bold("disabled")}");
                }


                #region COMMAND_INSTANTLEAVE_COOLDOWN
                [Command("sensitivity")]
                [Description("Set the instant leave cooldown. User will be banned if he leaves in the given sensitivity time window (in seconds).")]
                [Aliases("setsensitivity", "setsens", "sens", "s")]
                [UsageExamples("!guild cfg instaleave cooldown 9")]
                public async Task SetSensitivityAsync(CommandContext ctx,
                                                     [Description("Cooldown (in seconds).")] short cooldown)
                {
                    if (cooldown < 2 || cooldown > 60)
                        throw new CommandFailedException("The sensitivity is not in the valid range ([2, 60]).");

                    AntiInstantLeaveSettings settings = await this.Database.GetAntiInstantLeaveSettingsAsync(ctx.Guild.Id);
                    settings.Cooldown = cooldown;

                    await this.Database.SetAntiInstantLeaveSettingsAsync(ctx.Guild.Id, settings);

                    DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                    if (!(logchn is null)) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Color = this.ModuleColor
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Instant leave cooldown changed to", $"{settings.Cooldown}s");
                        await logchn.SendMessageAsync(embed: emb.Build());
                    }

                    await this.InformAsync(ctx, $"Instant leave cooldown for this guild has been changed to {Formatter.Bold(settings.Cooldown.ToString())}s", important: false);
                }
                #endregion
            }
        }
    }
}
