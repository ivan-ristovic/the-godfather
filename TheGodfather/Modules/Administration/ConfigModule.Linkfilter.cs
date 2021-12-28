using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Administration;

public sealed partial class ConfigModule
{
    [Group("linkfilter")]
    [Aliases("lf")]
    public sealed class LinkfilterModule : TheGodfatherServiceModule<LinkfilterService>
    {
        #region config linkfilter
        [GroupCommand][Priority(1)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
            [Description(TranslationKey.desc_enable)] bool enable)
        {
            await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.LinkfilterEnabled = enable;
            });
            await ctx.InfoAsync(this.ModuleColor);
            await this.LogConfigChangeAsync(ctx, TranslationKey.str_lf, enable);
        }

        [GroupCommand][Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx)
        {
            LinkfilterSettings settings = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).LinkfilterSettings;
            if (!settings.Enabled)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_lf_off);

            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.str_lf);
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField(TranslationKey.str_lf_invite, settings.BlockDiscordInvites ? TranslationKey.str_on : TranslationKey.str_off, true);
                emb.AddLocalizedField(TranslationKey.str_lf_ddos, settings.BlockBooterWebsites ? TranslationKey.str_on : TranslationKey.str_off, true);
                emb.AddLocalizedField(TranslationKey.str_lf_gore, settings.BlockDisturbingWebsites ? TranslationKey.str_on : TranslationKey.str_off, true);
                emb.AddLocalizedField(TranslationKey.str_lf_ip, settings.BlockIpLoggingWebsites ? TranslationKey.str_on : TranslationKey.str_off, true);
                emb.AddLocalizedField(TranslationKey.str_lf_urlshort, settings.BlockDisturbingWebsites ? TranslationKey.str_on : TranslationKey.str_off, true);
            });
        }
        #endregion

        #region config linkfilter booters
        [Command("booters")][Priority(1)]
        [Aliases("ddos", "boot", "dos")]
        public async Task BootersAsync(CommandContext ctx,
            [Description(TranslationKey.desc_enable)] bool enable)
        {
            await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.LinkfilterBootersEnabled = enable;
            });
            await ctx.InfoAsync(this.ModuleColor);
            await this.LogConfigChangeAsync(ctx, TranslationKey.str_lf_ddos, enable);
        }

        [Command("booters")][Priority(0)]
        public Task BootersAsync(CommandContext ctx)
        {
            CachedGuildConfig gcfg = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
            return ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_lf_ddos(gcfg.LinkfilterSettings.BlockBooterWebsites));
        }
        #endregion

        #region config linkfilter invites
        [Command("invites")][Priority(1)]
        [Aliases("invite", "inv", "i")]
        public async Task InvitesAsync(CommandContext ctx,
            [Description(TranslationKey.desc_enable)] bool enable)
        {
            await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.LinkfilterDiscordInvitesEnabled = enable;
            });
            await ctx.InfoAsync(this.ModuleColor);
            await this.LogConfigChangeAsync(ctx, TranslationKey.str_lf_invite, enable);
        }

        [Command("invites")][Priority(0)]
        public Task InvitesAsync(CommandContext ctx)
        {
            CachedGuildConfig gcfg = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
            return ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_lf_invite(gcfg.LinkfilterSettings.BlockDiscordInvites));
        }
        #endregion

        #region config linkfilter shocksites
        [Command("shocksites")][Priority(1)]
        [Aliases("disturbingsites", "shock", "disturbing", "gore")]
        public async Task DisturbingSitesAsync(CommandContext ctx,
            [Description(TranslationKey.desc_enable)] bool enable)
        {
            await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.LinkfilterDisturbingWebsitesEnabled = enable;
            });
            await ctx.InfoAsync(this.ModuleColor);
            await this.LogConfigChangeAsync(ctx, TranslationKey.str_lf_gore, enable);
        }

        [Command("shocksites")][Priority(0)]
        public Task DisturbingSitesAsync(CommandContext ctx)
        {
            CachedGuildConfig gcfg = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
            return ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_lf_gore(gcfg.LinkfilterSettings.BlockDisturbingWebsites));
        }
        #endregion

        #region config linkfilter iploggers
        [Command("iploggers")][Priority(1)]
        [Aliases("ip", "loggers")]
        public async Task IpLoggersAsync(CommandContext ctx,
            [Description(TranslationKey.desc_enable)] bool enable)
        {
            await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.LinkfilterIpLoggersEnabled = enable;
            });
            await ctx.InfoAsync(this.ModuleColor);
            await this.LogConfigChangeAsync(ctx, TranslationKey.str_lf_ip, enable);
        }

        [Command("iploggers")][Priority(0)]
        public Task IpLoggersAsync(CommandContext ctx)
        {
            CachedGuildConfig gcfg = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
            return ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_lf_ip(gcfg.LinkfilterSettings.BlockIpLoggingWebsites));
        }
        #endregion

        #region config linkfilter shorteners
        [Command("shorteners")][Priority(1)]
        [Aliases("urlshort", "shortenurl", "urlshorteners")]
        public async Task ShortenersAsync(CommandContext ctx,
            [Description(TranslationKey.desc_enable)] bool enable)
        {
            await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.LinkfilterUrlShortenersEnabled = enable;
            });
            await ctx.InfoAsync(this.ModuleColor);
            await this.LogConfigChangeAsync(ctx, TranslationKey.str_lf_urlshort, enable);
        }

        [Command("shorteners")][Priority(0)]
        public Task ShortenersAsync(CommandContext ctx)
        {
            CachedGuildConfig gcfg = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
            return ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_lf_urlshort(gcfg.LinkfilterSettings.BlockUrlShorteners));
        }
        #endregion


        #region internals
        private Task LogConfigChangeAsync(CommandContext ctx, TranslationKey key, bool value)
        {
            return ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField(key, value ? TranslationKey.str_on : TranslationKey.str_off);
            });
        }
        #endregion
    }
}