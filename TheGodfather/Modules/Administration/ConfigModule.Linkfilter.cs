using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Administration
{
    public sealed partial class ConfigModule
    {
        [Group("linkfilter")]
        [Aliases("lf")]
        public sealed class LinkfilterModule : TheGodfatherServiceModule<LinkfilterService>
        {
            #region config linkfilter
            [GroupCommand, Priority(1)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("desc-enable")] bool enable)
            {
                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.LinkfilterEnabled = enable;
                });
                await ctx.InfoAsync(this.ModuleColor);
                await this.LogConfigChangeAsync(ctx, "str-lf", enable);
            }

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx)
            {
                LinkfilterSettings settings = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).LinkfilterSettings;
                if (!settings.Enabled)
                    throw new CommandFailedException(ctx, "cmd-err-lf-off");

                return ctx.RespondWithLocalizedEmbedAsync(emb => {
                    emb.WithLocalizedTitle("str-lf");
                    emb.WithColor(this.ModuleColor);
                    emb.AddField("str-lf-invite", settings.BlockDiscordInvites ? "str-on" : "str-off", inline: true);
                    emb.AddField("str-lf-ddos", settings.BlockBooterWebsites ? "str-on" : "str-off", inline: true);
                    emb.AddField("str-lf-gore", settings.BlockDisturbingWebsites ? "str-on" : "str-off", inline: true);
                    emb.AddField("str-lf-ip", settings.BlockIpLoggingWebsites ? "str-on" : "str-off", inline: true);
                    emb.AddField("str-lf-urlshort", settings.BlockDisturbingWebsites ? "str-on" : "str-off", inline: true);
                });
            }
            #endregion

            #region config linkfilter booters
            [Command("booters"), Priority(1)]
            [Aliases("ddos", "boot", "dos")]
            public async Task BootersAsync(CommandContext ctx,
                                          [Description("desc-enable")] bool enable)
            {
                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.LinkfilterBootersEnabled = enable;
                });
                await ctx.InfoAsync(this.ModuleColor);
                await this.LogConfigChangeAsync(ctx, "str-lf-ddos", enable);
            }

            [Command("booters"), Priority(0)]
            public Task BootersAsync(CommandContext ctx)
            {
                CachedGuildConfig gcfg = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
                return ctx.InfoAsync(this.ModuleColor, "fmt-lf-ddos", gcfg.LinkfilterSettings.BlockBooterWebsites);
            }
            #endregion

            #region config linkfilter invites
            [Command("invites"), Priority(1)]
            [Aliases("invite", "inv", "i")]
            public async Task InvitesAsync(CommandContext ctx,
                                          [Description("desc-enable")] bool enable)
            {
                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.LinkfilterDiscordInvitesEnabled = enable;
                });
                await ctx.InfoAsync(this.ModuleColor);
                await this.LogConfigChangeAsync(ctx, "str-lf-invite", enable);
            }

            [Command("invites"), Priority(0)]
            public Task InvitesAsync(CommandContext ctx)
            {
                CachedGuildConfig gcfg = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
                return ctx.InfoAsync(this.ModuleColor, "fmt-lf-invite", gcfg.LinkfilterSettings.BlockDiscordInvites);
            }
            #endregion

            #region config linkfilter shocksites
            [Command("shocksites"), Priority(1)]
            [Aliases("disturbingsites", "shock", "disturbing", "gore")]
            public async Task DisturbingSitesAsync(CommandContext ctx,
                                                  [Description("desc-enable")] bool enable)
            {
                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.LinkfilterDisturbingWebsitesEnabled = enable;
                });
                await ctx.InfoAsync(this.ModuleColor);
                await this.LogConfigChangeAsync(ctx, "str-lf-gore", enable);
            }

            [Command("shocksites"), Priority(0)]
            public Task DisturbingSitesAsync(CommandContext ctx)
            {
                CachedGuildConfig gcfg = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
                return ctx.InfoAsync(this.ModuleColor, "fmt-lf-gore", gcfg.LinkfilterSettings.BlockDisturbingWebsites);
            }
            #endregion

            #region config linkfilter iploggers
            [Command("iploggers"), Priority(1)]
            [Aliases("ip", "loggers")]
            public async Task IpLoggersAsync(CommandContext ctx,
                                            [Description("desc-enable")] bool enable)
            {
                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.LinkfilterIpLoggersEnabled = enable;
                });
                await ctx.InfoAsync(this.ModuleColor);
                await this.LogConfigChangeAsync(ctx, "str-lf-ip", enable);
            }

            [Command("iploggers"), Priority(0)]
            public Task IpLoggersAsync(CommandContext ctx)
            {
                CachedGuildConfig gcfg = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
                return ctx.InfoAsync(this.ModuleColor, "fmt-lf-ip", gcfg.LinkfilterSettings.BlockIpLoggingWebsites);
            }
            #endregion

            #region config linkfilter shorteners
            [Command("shorteners"), Priority(1)]
            [Aliases("urlshort", "shortenurl", "urlshorteners")]
            public async Task ShortenersAsync(CommandContext ctx,
                                             [Description("desc-enable")] bool enable)
            {
                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.LinkfilterUrlShortenersEnabled = enable;
                });
                await ctx.InfoAsync(this.ModuleColor);
                await this.LogConfigChangeAsync(ctx, "str-lf-urlshort", enable);
            }

            [Command("shorteners"), Priority(0)]
            public Task ShortenersAsync(CommandContext ctx)
            {
                CachedGuildConfig gcfg = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
                return ctx.InfoAsync(this.ModuleColor, "fmt-lf-urlshort", gcfg.LinkfilterSettings.BlockUrlShorteners);
            }
            #endregion


            #region internals
            private Task LogConfigChangeAsync(CommandContext ctx, string key, bool value)
            {
                return ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-cfg-upd");
                    emb.WithColor(this.ModuleColor);
                    emb.AddLocalizedField(key, value ? "str-on" : "str-off");
                });
            }
            #endregion
        }
    }
}
