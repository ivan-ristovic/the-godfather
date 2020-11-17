using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration
{
    public partial class ConfigModule : TheGodfatherServiceModule<GuildConfigService>
    {
        #region config welcome
        [Group("welcome")]
        [Aliases("enter", "join", "wlc", "wm", "w")]
        public class ConfigModuleWelcome : TheGodfatherServiceModule<GuildConfigService>
        {
            public ConfigModuleWelcome(GuildConfigService service)
                : base(service) { }


            #region config welcome
            [GroupCommand, Priority(1)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("desc-welcome")] bool enable,
                                               [Description("desc-welcome-chn")] DiscordChannel? chn = null)
            {
                if (enable) {
                    if (chn is null)
                        throw new CommandFailedException(ctx, "cmd-err-chn-none");
                    await this.WelcomeChannelAsync(ctx, chn);
                } else {
                    await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.WelcomeChannelId = default);

                    await ctx.GuildLogAsync(emb => {
                        emb.WithLocalizedTitle("evt-cfg-upd");
                        emb.WithColor(this.ModuleColor);
                        emb.AddLocalizedField("str-memupd-w", "str-off", inline: true);
                    });

                    await ctx.InfoAsync(this.ModuleColor, "str-cfg-welcome-off");
                }
            }

            [GroupCommand, Priority(0)]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                GuildConfig gcfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
                DiscordChannel? wchn = gcfg.WelcomeChannelId != default ? ctx.Guild.GetChannel(gcfg.WelcomeChannelId) : null;
                if (wchn is null) {
                    await ctx.InfoAsync(this.ModuleColor, "str-cfg-welcome-get-off");
                } else {
                    await ctx.InfoAsync(this.ModuleColor, "str-cfg-welcome-get-on", wchn.Mention);
                }
            }
            #endregion

            #region config welcome channel
            [Command("channel")]
            [Aliases("chn", "ch", "c")]
            public async Task WelcomeChannelAsync(CommandContext ctx,
                                                 [Description("desc-welcome-chn")] DiscordChannel? wchn = null)
            {
                wchn ??= ctx.Channel;

                if (wchn.Type != ChannelType.Text)
                    throw new CommandFailedException(ctx, "cmd-err-chn-type-text");

                await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.WelcomeChannelId = wchn.Id);

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-cfg-upd");
                    emb.WithColor(this.ModuleColor);
                    emb.AddLocalizedTitleField("str-memupd-wc", wchn.Mention, inline: true);
                });

                await ctx.InfoAsync(this.ModuleColor, "fmt-memupd-wc", wchn.Mention);
            }
            #endregion

            #region config welcome message
            [Command("message")]
            [Aliases("msg", "m")]
            public async Task WelcomeMessageAsync(CommandContext ctx,
                                                 [RemainingText, Description("desc-welcome-msg")] string message)
            {
                if (!string.IsNullOrWhiteSpace(message) && (message.Length < 3 || message.Length > 120))
                    throw new CommandFailedException(ctx, "cmd-err-memupd-msg");

                await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.WelcomeMessage = message);

                message = Formatter.BlockCode(Formatter.Strip(message));
                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-cfg-upd");
                    emb.WithColor(this.ModuleColor);
                    emb.AddLocalizedTitleField("str-memupd-wm", message, inline: true);
                });

                await ctx.InfoAsync(this.ModuleColor, "fmt-memupd-wm", message);
            }
            #endregion
        }
        #endregion

        #region config leave
        [Group("leave")]
        [Aliases("quit", "lv", "lm", "l")]
        public class ConfigModuleLeave : TheGodfatherServiceModule<GuildConfigService>
        {
            public ConfigModuleLeave(GuildConfigService service)
                : base(service) { }


            #region config leave
            [GroupCommand, Priority(1)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("desc-leave")] bool enable,
                                               [Description("desc-leave-chn")] DiscordChannel? chn = null)
            {
                if (enable) {
                    if (chn is null)
                        throw new CommandFailedException(ctx, "cmd-err-chn-none");
                    await this.LeaveChanneAsync(ctx, chn);
                } else {
                    await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.LeaveChannelId = default);

                    await ctx.GuildLogAsync(emb => {
                        emb.WithLocalizedTitle("evt-cfg-upd");
                        emb.WithColor(this.ModuleColor);
                        emb.AddLocalizedField("str-memupd-l", "str-off", inline: true);
                    });

                    await ctx.InfoAsync(this.ModuleColor, "str-cfg-leave-off");
                }
            }

            [GroupCommand, Priority(0)]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                GuildConfig gcfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
                DiscordChannel? lchn = gcfg.LeaveChannelId != default ? ctx.Guild.GetChannel(gcfg.LeaveChannelId) : null;
                if (lchn is null) {
                    await ctx.InfoAsync(this.ModuleColor, "str-cfg-leave-get-off");
                } else {
                    await ctx.InfoAsync(this.ModuleColor, "str-cfg-leave-get-on", lchn.Mention);
                }
            }
            #endregion

            #region config leave channel
            [Command("channel")]
            [Aliases("chn", "ch", "c")]
            public async Task LeaveChanneAsync(CommandContext ctx,
                                                 [Description("desc-leave-chn")] DiscordChannel? lchn = null)
            {
                lchn ??= ctx.Channel;

                if (lchn.Type != ChannelType.Text)
                    throw new CommandFailedException(ctx, "cmd-err-chn-type-text");

                await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.WelcomeChannelId = lchn.Id);

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-cfg-upd");
                    emb.WithColor(this.ModuleColor);
                    emb.AddLocalizedTitleField("str-memupd-lc", lchn.Mention, inline: true);
                });

                await ctx.InfoAsync(this.ModuleColor, "fmt-memupd-lc", lchn.Mention);
            }
            #endregion

            #region config leave message
            [Command("message")]
            [Aliases("msg", "m")]
            public async Task WelcomeMessageAsync(CommandContext ctx,
                                                 [RemainingText, Description("desc-leave-msg")] string message)
            {
                if (!string.IsNullOrWhiteSpace(message) && (message.Length < 3 || message.Length > 120))
                    throw new CommandFailedException(ctx, "cmd-err-memupd-msg");

                await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.WelcomeMessage = message);

                message = Formatter.BlockCode(Formatter.Strip(message));
                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-cfg-upd");
                    emb.WithColor(this.ModuleColor);
                    emb.AddLocalizedTitleField("str-memupd-lm", message, inline: true);
                });

                await ctx.InfoAsync(this.ModuleColor, "fmt-memupd-lm", message);
            }
            #endregion
        }
        #endregion
    }
}

