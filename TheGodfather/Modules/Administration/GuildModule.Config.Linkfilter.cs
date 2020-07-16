#region USING_DIRECTIVES
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services.Common;
#endregion

namespace TheGodfather.Modules.Administration
{
    public partial class GuildModule
    {
        public partial class GuildConfigModule
        {
            [Group("linkfilter")]
            [Description("Linkfilter configuration. Group call prints current configuration, or enables/disables linkfilter if specified.")]
            [Aliases("lf", "linkf", "linkremove", "filterlinks")]

            public class LinkfilterModule : TheGodfatherModule
            {

                public LinkfilterModule(DbContextBuilder db)
                    : base(db)
                {

                }


                [GroupCommand, Priority(1)]
                public async Task ExecuteGroupAsync(CommandContext ctx,
                                                   [Description("Enable?")] bool enable)
                {
                    GuildConfig gcfg = await ctx.Services.GetService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                        cfg.LinkfilterEnabled = enable;
                    });
                    await this.InformAsync(ctx, $"{(enable ? "Enabled" : "Disabled")} link filtering!", important: false);
                    await this.LogConfigChangeAsync(ctx, "Linkfilter", gcfg.LinkfilterEnabled);
                }

                [GroupCommand, Priority(0)]
                public Task ExecuteGroupAsync(CommandContext ctx)
                {
                    LinkfilterSettings settings = ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).LinkfilterSettings;
                    if (settings.Enabled) {
                        var emb = new DiscordEmbedBuilder {
                            Title = "Linkfilter modules for this guild",
                            Color = this.ModuleColor
                        };
                        emb.AddField("Discord invites filter", settings.BlockDiscordInvites ? "enabled" : "disabled", inline: true);
                        emb.AddField("DDoS/Booter websites filter", settings.BlockBooterWebsites ? "enabled" : "disabled", inline: true);
                        emb.AddField("Disturbing websites filter", settings.BlockDisturbingWebsites ? "enabled" : "disabled", inline: true);
                        emb.AddField("IP logging websites filter", settings.BlockIpLoggingWebsites ? "enabled" : "disabled", inline: true);
                        emb.AddField("URL shortening websites filter", settings.BlockDisturbingWebsites ? "enabled" : "disabled", inline: true);

                        return ctx.RespondAsync(embed: emb.Build());
                    } else {
                        return this.InformAsync(ctx, $"Link filtering for this guild is: {Formatter.Bold("disabled")}!");
                    }
                }


                #region COMMAND_LINKFILTER_BOOTERS
                [Command("booters"), Priority(1)]
                [Description("Enable or disable DDoS/Booter website filtering.")]
                [Aliases("ddos", "boot", "dos")]

                public async Task BootersAsync(CommandContext ctx,
                                              [Description("Enable?")] bool enable)
                {
                    GuildConfig gcfg = await ctx.Services.GetService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                        cfg.LinkfilterBootersEnabled = enable;
                    });
                    await this.InformAsync(ctx, $"{(enable ? "Enabled" : "Disabled")} DDoS/Booter website filtering!", important: false);
                    await this.LogConfigChangeAsync(ctx, "DDoS/Booter websites filtering", gcfg.LinkfilterBootersEnabled);
                }

                [Command("booters"), Priority(0)]
                public Task BootersAsync(CommandContext ctx)
                {
                    CachedGuildConfig gcfg = ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
                    return this.InformAsync(ctx, $"DDoS/Booter website filtering for this guild is: {Formatter.Bold(gcfg.LinkfilterSettings.BlockBooterWebsites ? "enabled" : "disabled")}!");
                }
                #endregion

                #region COMMAND_LINKFILTER_INVITES
                [Command("invites"), Priority(1)]
                [Description("Enable or disable Discord invite filters.")]
                [Aliases("invite", "inv", "i")]

                public async Task InvitesAsync(CommandContext ctx,
                                              [Description("Enable?")] bool enable)
                {
                    GuildConfig gcfg = await ctx.Services.GetService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                        cfg.LinkfilterDiscordInvitesEnabled = enable;
                    });
                    await this.InformAsync(ctx, $"{(enable ? "Enabled" : "Disabled")} Discord invites filtering!", important: false);
                    await this.LogConfigChangeAsync(ctx, "Discord invites filtering", gcfg.LinkfilterDiscordInvitesEnabled);
                }

                [Command("invites"), Priority(0)]
                public Task InvitesAsync(CommandContext ctx)
                {
                    CachedGuildConfig gcfg = ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
                    return this.InformAsync(ctx, $"Invite link filtering for this guild is: {Formatter.Bold(gcfg.LinkfilterSettings.BlockDiscordInvites ? "enabled" : "disabled")}!");
                }
                #endregion

                #region COMMAND_LINKFILTER_SHOCKSITES
                [Command("disturbingsites"), Priority(1)]
                [Description("Enable or disable shock website filtering.")]
                [Aliases("disturbing", "shock", "shocksites")]

                public async Task DisturbingSitesAsync(CommandContext ctx,
                                                      [Description("Enable?")] bool enable)
                {
                    GuildConfig gcfg = await ctx.Services.GetService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                        cfg.LinkfilterDisturbingWebsitesEnabled = enable;
                    });
                    await this.InformAsync(ctx, $"{(enable ? "Enabled" : "Disabled")} disturbing website filtering!", important: false);
                    await this.LogConfigChangeAsync(ctx, "Disturbing websites filtering", gcfg.LinkfilterDisturbingWebsitesEnabled);
                }

                [Command("disturbingsites"), Priority(0)]
                public Task DisturbingSitesAsync(CommandContext ctx)
                {
                    CachedGuildConfig gcfg = ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
                    return this.InformAsync(ctx, $"Shock website filtering for this guild is: {Formatter.Bold(gcfg.LinkfilterSettings.BlockDisturbingWebsites ? "enabled" : "disabled")}!");
                }
                #endregion

                #region COMMAND_LINKFILTER_IPLOGGERS
                [Command("iploggers"), Priority(1)]
                [Description("Enable or disable filtering of IP logger websites.")]
                [Aliases("ip", "loggers", "ipleech")]

                public async Task IpLoggersAsync(CommandContext ctx,
                                                [Description("Enable?")] bool enable)
                {
                    GuildConfig gcfg = await ctx.Services.GetService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                        cfg.LinkfilterIpLoggersEnabled = enable;
                    });
                    await this.InformAsync(ctx, $"{(enable ? "Enabled" : "Disabled")} IP logging website filtering!", important: false);
                    await this.LogConfigChangeAsync(ctx, "IP logging websites filtering", gcfg.LinkfilterIpLoggersEnabled);
                }

                [Command("iploggers"), Priority(0)]
                public Task IpLoggersAsync(CommandContext ctx)
                {
                    CachedGuildConfig gcfg = ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
                    return this.InformAsync(ctx, $"IP logging websites filtering for this guild is: {Formatter.Bold(gcfg.LinkfilterSettings.BlockIpLoggingWebsites ? "enabled" : "disabled")}!");
                }
                #endregion

                #region COMMAND_LINKFILTER_SHORTENERS
                [Command("shorteners"), Priority(1)]
                [Description("Enable or disable filtering of URL shortener websites.")]
                [Aliases("urlshort", "shortenurl", "urlshorteners")]

                public async Task ShortenersAsync(CommandContext ctx,
                                                 [Description("Enable?")] bool enable)
                {
                    GuildConfig gcfg = await ctx.Services.GetService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                        cfg.LinkfilterUrlShortenersEnabled = enable;
                    });
                    await this.InformAsync(ctx, $"{(enable ? "Enabled" : "Disabled")} URL shortener website filtering!", important: false);
                    await this.LogConfigChangeAsync(ctx, "URL shorteners filtering", gcfg.LinkfilterUrlShortenersEnabled);
                }

                [Command("shorteners"), Priority(0)]
                public Task ShortenersAsync(CommandContext ctx)
                {
                    CachedGuildConfig gcfg = ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
                    return this.InformAsync(ctx, $"URL shortening websites filtering for this guild is: {Formatter.Bold(gcfg.LinkfilterSettings.BlockUrlShorteners ? "enabled" : "disabled")}!");
                }
                #endregion


                #region HELPER_FUNCTIONS
                protected Task LogConfigChangeAsync(CommandContext ctx, string module, bool value)
                {
                    DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
                    if (!(logchn is null)) {
                        var emb = new DiscordEmbedBuilder {
                            Title = "Guild config changed",
                            Color = this.ModuleColor
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField(module, value ? "on" : "off", inline: true);
                        return logchn.SendMessageAsync(embed: emb.Build());
                    } else {
                        return Task.CompletedTask;
                    }
                }
                #endregion
            }
        }
    }
}
