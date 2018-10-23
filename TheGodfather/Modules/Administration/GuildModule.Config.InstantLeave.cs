#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
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
                                                   [Description("Cooldown (join-leave max seconds).")] short cooldown)
                {
                    if (cooldown < 2 || cooldown > 60)
                        throw new CommandFailedException("The cooldown is not in the valid range ([2, 60]).");

                    DatabaseGuildConfig gcfg = await this.ModifyGuildConfigAsync(ctx.Guild.Id, cfg => {
                        cfg.AntiInstantLeaveCooldown = cooldown;
                        cfg.AntiInstantLeaveEnabled = enable;
                    });

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
                            emb.AddField("Instant leave cooldown", gcfg.AntiInstantLeaveCooldown.ToString(), inline: true);
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
                    AntiInstantLeaveSettings settings = (await this.GetGuildConfig(ctx.Guild.Id)).AntiInstantLeaveSettings;
                    if (settings.Enabled)
                        await this.InformAsync(ctx, $"Instant leave watch: {Formatter.Bold("enabled")} with {Formatter.Bold(settings.Cooldown.ToString())}s cooldown");
                    else
                        await this.InformAsync(ctx, $"Instant leave watch: {Formatter.Bold("disabled")}");
                }


                #region COMMAND_INSTANTLEAVE_SENSITIVITY
                [Command("cooldown")]
                [Description("Set the instant leave sensitivity. User will be banned if he leaves within the given time window (in seconds).")]
                [Aliases("setcooldown", "setcool", "cool", "c")]
                [UsageExamples("!guild cfg instaleave cooldown 5")]
                public async Task SetSensitivityAsync(CommandContext ctx,
                                                     [Description("Cooldown (in seconds).")] short cooldown)
                {
                    if (cooldown < 2 || cooldown > 60)
                        throw new CommandFailedException("The cooldown is not in the valid range ([2, 60]).");

                    DatabaseGuildConfig gcfg = await this.ModifyGuildConfigAsync(ctx.Guild.Id, cfg => {
                        cfg.AntiInstantLeaveCooldown = cooldown;
                    });
                    
                    DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                    if (!(logchn is null)) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Color = this.ModuleColor
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Instant leave cooldown changed to", $"{gcfg.AntiInstantLeaveCooldown}s");
                        await logchn.SendMessageAsync(embed: emb.Build());
                    }

                    await this.InformAsync(ctx, $"Instant leave cooldown for this guild has been changed to {Formatter.Bold(gcfg.AntiInstantLeaveCooldown.ToString())}s", important: false);
                }
                #endregion
            }
        }
    }
}
