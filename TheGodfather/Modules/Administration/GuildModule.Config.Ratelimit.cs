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
            [Description("Prevents users from posting more than specified messages in short period of time.")]
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
                    if (sensitivity < 4)
                        throw new CommandFailedException("The sensitivity is too high. Note that lower the value, the less messages people are allowed to send. The value you entered is too low. Valid range: [4, 10]");

                    if (sensitivity > 10)
                        sensitivity = 10;

                    CachedGuildConfig gcfg = this.Shared.GetGuildConfig(ctx.Guild.Id);
                    gcfg.RatelimitEnabled = enable;
                    gcfg.RatelimitAction = action;
                    gcfg.RatelimitSensitivity = sensitivity;

                    await this.Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg);

                    DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                    if (logchn != null) {
                        var emb = new DiscordEmbedBuilder() {
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

                    await InformAsync(ctx, $"{Formatter.Bold(gcfg.LoggingEnabled ? "Enabled" : "Disabled")} ratelimit actions.", important: false);
                }

                [GroupCommand, Priority(2)]
                public Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("Enable?")] bool enable ,
                                             [Description("Action type.")] PunishmentActionType action,
                                             [Description("Sensitivity (messages per 5s to trigger action).")] short sensitivity = 5)
                    => ExecuteGroupAsync(ctx, enable, sensitivity, action);

                [GroupCommand, Priority(1)]
                public Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("Enable?")] bool enable)
                    => ExecuteGroupAsync(ctx, enable, 5, PunishmentActionType.Mute);

                [GroupCommand, Priority(0)]
                public Task ExecuteGroupAsync(CommandContext ctx)
                {
                    CachedGuildConfig gcfg = this.Shared.GetGuildConfig(ctx.Guild.Id);
                    return InformAsync(ctx, $"Ratelimit watch for this guild is {Formatter.Bold(gcfg.RatelimitEnabled ? "enabled" : "disabled")}", important: false);

                }


                // TODO command sensitivity
                // TODO command action
            }
        }
    }
}
