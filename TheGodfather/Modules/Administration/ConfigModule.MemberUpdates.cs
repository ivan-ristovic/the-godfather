using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration;

public sealed partial class ConfigModule : TheGodfatherServiceModule<GuildConfigService>
{
    #region config welcome
    [Group("welcome")]
    [Aliases("enter", "join", "wlc", "wm", "w")]
    public sealed class ConfigModuleWelcome : TheGodfatherServiceModule<GuildConfigService>
    {
        #region config welcome
        [GroupCommand][Priority(1)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
            [Description(TranslationKey.desc_welcome)] bool enable,
            [Description(TranslationKey.desc_welcome_chn)] DiscordChannel? chn = null)
        {
            if (enable) {
                if (chn is null)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_chn_none);
                await this.WelcomeChannelAsync(ctx, chn);
            } else {
                await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.WelcomeChannelId = default);

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
                    emb.WithColor(this.ModuleColor);
                    emb.AddLocalizedField(TranslationKey.str_memupd_w, TranslationKey.str_off, inline: true);
                });

                await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_cfg_welcome_off);
            }
        }

        [GroupCommand][Priority(0)]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            GuildConfig gcfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
            DiscordChannel? wchn = gcfg.WelcomeChannelId != default ? ctx.Guild.GetChannel(gcfg.WelcomeChannelId) : null;
            if (wchn is null)
                await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_cfg_welcome_get_off);
            else
                await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_cfg_welcome_get_on(wchn.Mention));
        }
        #endregion

        #region config welcome channel
        [Command("channel")]
        [Aliases("chn", "ch", "c")]
        public async Task WelcomeChannelAsync(CommandContext ctx,
            [Description(TranslationKey.desc_welcome_chn)] DiscordChannel? wchn = null)
        {
            wchn ??= ctx.Channel;

            if (wchn.Type != ChannelType.Text)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chn_type_text);

            await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.WelcomeChannelId = wchn.Id);

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField(TranslationKey.str_memupd_wc, wchn.Mention, true);
            });

            await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_memupd_wc(wchn.Mention));
        }
        #endregion

        #region config welcome message
        [Command("message")]
        [Aliases("msg", "m")]
        public async Task WelcomeMessageAsync(CommandContext ctx,
            [RemainingText][Description(TranslationKey.desc_welcome_msg)] string message)
        {
            if (string.IsNullOrWhiteSpace(message) || message.Length is < 3 or > GuildConfig.MemberUpdateMessageLimit)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_memupd_msg(GuildConfig.MemberUpdateMessageLimit));

            await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.WelcomeMessage = message);

            message = Formatter.BlockCode(Formatter.Strip(message));
            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField(TranslationKey.str_memupd_wm, message, true);
            });

            await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_memupd_wm(message));
        }
        #endregion
    }
    #endregion

    #region config leave
    [Group("leave")]
    [Aliases("quit", "lv", "lm", "l")]
    public class ConfigModuleLeave : TheGodfatherServiceModule<GuildConfigService>
    {
        #region config leave
        [GroupCommand][Priority(1)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
            [Description(TranslationKey.desc_leave)] bool enable,
            [Description(TranslationKey.desc_leave_chn)] DiscordChannel? chn = null)
        {
            if (enable) {
                if (chn is null)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_chn_none);
                await this.LeaveChanneAsync(ctx, chn);
            } else {
                await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.LeaveChannelId = default);

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
                    emb.WithColor(this.ModuleColor);
                    emb.AddLocalizedField(TranslationKey.str_memupd_l, TranslationKey.str_off, inline: true);
                });

                await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_cfg_leave_off);
            }
        }

        [GroupCommand][Priority(0)]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            GuildConfig gcfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
            DiscordChannel? lchn = gcfg.LeaveChannelId != default ? ctx.Guild.GetChannel(gcfg.LeaveChannelId) : null;
            if (lchn is null)
                await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_cfg_leave_get_off);
            else
                await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_cfg_leave_get_on(lchn.Mention));
        }
        #endregion

        #region config leave channel
        [Command("channel")]
        [Aliases("chn", "ch", "c")]
        public async Task LeaveChanneAsync(CommandContext ctx,
            [Description(TranslationKey.desc_leave_chn)] DiscordChannel? lchn = null)
        {
            lchn ??= ctx.Channel;

            if (lchn.Type != ChannelType.Text)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chn_type_text);

            await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.WelcomeChannelId = lchn.Id);

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField(TranslationKey.str_memupd_lc, lchn.Mention, true);
            });

            await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_memupd_lc(lchn.Mention));
        }
        #endregion

        #region config leave message
        [Command("message")]
        [Aliases("msg", "m")]
        public async Task WelcomeMessageAsync(CommandContext ctx,
            [RemainingText][Description(TranslationKey.desc_leave_msg)] string message)
        {
            if (string.IsNullOrWhiteSpace(message) || message.Length is < 3 or > GuildConfig.MemberUpdateMessageLimit)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_memupd_msg(GuildConfig.MemberUpdateMessageLimit));

            await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.WelcomeMessage = message);

            message = Formatter.BlockCode(Formatter.Strip(message));
            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField(TranslationKey.str_memupd_lm, message, true);
            });

            await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_memupd_lm(message));
        }
        #endregion
    }
    #endregion
}