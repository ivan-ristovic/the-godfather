#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Administration
{
    public partial class GuildModule
    {
        public partial class ConfigModule : TheGodfatherBaseModule
        {
            [Group("linkfilter"), Module(ModuleType.Administration)]
            [Description("Linkfilter configuration.")]
            [Aliases("lf", "linkf", "linkremove", "filterlinks")]
            [UsageExample("!guild cfg linkfilter")]
            public class LinkFilter : TheGodfatherBaseModule
            {

                public LinkFilter(SharedData shared, DBService db) : base(shared, db) { }


                [GroupCommand]
                public async Task ExecuteGroupAsync(CommandContext ctx)
                {
                    var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                    if (gcfg.LinkfilterEnabled) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Linkfilter modules for this guild",
                            Color = DiscordColor.Green
                        };
                        emb.AddField("Discord invites filter", gcfg.BlockDiscordInvites ? "enabled" : "disabled", inline: true);
                        emb.AddField("DDoS/Booter websites filter", gcfg.BlockBooterWebsites ? "enabled" : "disabled", inline: true);
                        emb.AddField("Disturbing websites filter", gcfg.BlockDisturbingWebsites ? "enabled" : "disabled", inline: true);
                        emb.AddField("IP logging websites filter", gcfg.BlockIpLoggingWebsites ? "enabled" : "disabled", inline: true);
                        emb.AddField("URL shortening websites filter", gcfg.BlockDisturbingWebsites ? "enabled" : "disabled", inline: true);

                        await ctx.RespondAsync(embed: emb.Build())
                            .ConfigureAwait(false);
                    } else {
                        await ctx.RespondWithIconEmbedAsync($"Link filtering for this guild is: {Formatter.Bold("disabled")}!")
                            .ConfigureAwait(false);
                    }
                }


                #region COMMAND_LINKFILTER_ENABLE
                [Command("enable"), Module(ModuleType.Administration)]
                [Description("Enables link filtering for this guild.")]
                [Aliases("on")]
                [UsageExample("!guild cfg linkfilter on")]
                public async Task EnableAsync(CommandContext ctx)
                {
                    var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                    gcfg.LinkfilterEnabled = true;
                    await Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg)
                        .ConfigureAwait(false);

                    var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                    .ConfigureAwait(false);
                    if (logchn != null) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Color = DiscordColor.Brown
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Linkfilter", gcfg.LinkfilterEnabled ? "on" : "off", inline: true);
                        await logchn.SendMessageAsync(embed: emb.Build())
                            .ConfigureAwait(false);
                    }

                    await ctx.RespondWithIconEmbedAsync("Enabled link filtering!")
                        .ConfigureAwait(false);
                }
                #endregion

                #region COMMAND_LINKFILTER_DISABLE
                [Command("disable"), Module(ModuleType.Administration)]
                [Description("Disables link filtering for this guild.")]
                [Aliases("off")]
                [UsageExample("!guild cfg linkfilter off")]
                public async Task DisableAsync(CommandContext ctx)
                {
                    var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                    gcfg.LinkfilterEnabled = false;
                    await Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg)
                        .ConfigureAwait(false);

                    var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                    .ConfigureAwait(false);
                    if (logchn != null) {
                        var emb = new DiscordEmbedBuilder() {
                            Title = "Guild config changed",
                            Color = DiscordColor.Brown
                        };
                        emb.AddField("User responsible", ctx.User.Mention, inline: true);
                        emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                        emb.AddField("Link filtering", gcfg.SuggestionsEnabled ? "on" : "off", inline: true);
                        await logchn.SendMessageAsync(embed: emb.Build())
                            .ConfigureAwait(false);
                    }

                    await ctx.RespondWithIconEmbedAsync("Disabled link filtering!")
                        .ConfigureAwait(false);
                }
                #endregion


                #region GROUP_CONFIG_LINKFILTER_BOOTERS
                [Group("booters"), Module(ModuleType.Administration)]
                [Description("Enable or disable DDoS/Booter website filtering.")]
                [Aliases("ddos", "boot", "dos")]
                [UsageExample("!guild cfg linkfilter booters")]
                public class Booters : TheGodfatherBaseModule
                {

                    public Booters(SharedData shared, DBService db) : base(shared, db) { }


                    [GroupCommand]
                    public async Task ExecuteGroupAsync(CommandContext ctx)
                    {
                        var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                        await ctx.RespondWithIconEmbedAsync($"DDoS/Booter website filtering for this guild is: {Formatter.Bold(gcfg.BlockBooterWebsites ? "enabled" : "disabled")}!")
                            .ConfigureAwait(false);
                    }


                    #region COMMAND_LINKFILTER_BOOTERS_ENABLE
                    [Command("enable"), Module(ModuleType.Administration)]
                    [Description("Enables DDoS/Booter website filtering for this guild.")]
                    [Aliases("on")]
                    [UsageExample("!guild cfg linkfilter booters on")]
                    public async Task EnableAsync(CommandContext ctx)
                    {
                        var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                        gcfg.BlockBooterWebsites = true;
                        await Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg)
                            .ConfigureAwait(false);

                        var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                        .ConfigureAwait(false);
                        if (logchn != null) {
                            var emb = new DiscordEmbedBuilder() {
                                Title = "Guild config changed",
                                Color = DiscordColor.Brown
                            };
                            emb.AddField("User responsible", ctx.User.Mention, inline: true);
                            emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                            emb.AddField("DDoS/Booter website filtering", gcfg.BlockBooterWebsites ? "on" : "off", inline: true);
                            await logchn.SendMessageAsync(embed: emb.Build())
                                .ConfigureAwait(false);
                        }

                        await ctx.RespondWithIconEmbedAsync("Enabled DoS/Booter website filtering!")
                            .ConfigureAwait(false);
                    }
                    #endregion

                    #region COMMAND_LINKFILTER_BOOTERS_DISABLE
                    [Command("disable"), Module(ModuleType.Administration)]
                    [Description("Disables DoS/Booter website filtering for this guild.")]
                    [Aliases("off")]
                    [UsageExample("!guild cfg linkfilter booters off")]
                    public async Task DisableAsync(CommandContext ctx)
                    {
                        var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                        gcfg.BlockBooterWebsites = false;
                        await Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg)
                            .ConfigureAwait(false);

                        var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                        .ConfigureAwait(false);
                        if (logchn != null) {
                            var emb = new DiscordEmbedBuilder() {
                                Title = "Guild config changed",
                                Color = DiscordColor.Brown
                            };
                            emb.AddField("User responsible", ctx.User.Mention, inline: true);
                            emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                            emb.AddField("DoS/Booter website filtering", gcfg.BlockBooterWebsites ? "on" : "off", inline: true);
                            await logchn.SendMessageAsync(embed: emb.Build())
                                .ConfigureAwait(false);
                        }

                        await ctx.RespondWithIconEmbedAsync("Disabled DoS/Booter website filtering!")
                            .ConfigureAwait(false);
                    }
                    #endregion

                }
                #endregion

                #region GROUP_CONFIG_LINKFILTER_INVITES
                [Group("invites"), Module(ModuleType.Administration)]
                [Description("Enable or disable Discord invite filters.")]
                [Aliases("invite", "inv", "i")]
                [UsageExample("!guild cfg linkfilter invites")]
                public class Invites : TheGodfatherBaseModule
                {

                    public Invites(SharedData shared, DBService db) : base(shared, db) { }


                    [GroupCommand]
                    public async Task ExecuteGroupAsync(CommandContext ctx)
                    {
                        var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                        await ctx.RespondWithIconEmbedAsync($"Invite link filtering for this guild is: {Formatter.Bold(gcfg.BlockDiscordInvites ? "enabled" : "disabled")}!")
                            .ConfigureAwait(false);
                    }


                    #region COMMAND_LINKFILTER_INVITES_ENABLE
                    [Command("enable"), Module(ModuleType.Administration)]
                    [Description("Enables Discord invite filtering for this guild.")]
                    [Aliases("on")]
                    [UsageExample("!guild cfg linkfilter invites on")]
                    public async Task EnableAsync(CommandContext ctx)
                    {
                        var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                        gcfg.BlockDiscordInvites = true;
                        await Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg)
                            .ConfigureAwait(false);

                        var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                        .ConfigureAwait(false);
                        if (logchn != null) {
                            var emb = new DiscordEmbedBuilder() {
                                Title = "Guild config changed",
                                Color = DiscordColor.Brown
                            };
                            emb.AddField("User responsible", ctx.User.Mention, inline: true);
                            emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                            emb.AddField("Invite filtering", gcfg.BlockDiscordInvites ? "on" : "off", inline: true);
                            await logchn.SendMessageAsync(embed: emb.Build())
                                .ConfigureAwait(false);
                        }

                        await ctx.RespondWithIconEmbedAsync("Enabled Discord invites filtering!")
                            .ConfigureAwait(false);
                    }
                    #endregion

                    #region COMMAND_LINKFILTER_INVITES_DISABLE
                    [Command("disable"), Module(ModuleType.Administration)]
                    [Description("Disables Discord invite filtering for this guild.")]
                    [Aliases("off")]
                    [UsageExample("!guild cfg linkfilter invites off")]
                    public async Task DisableAsync(CommandContext ctx)
                    {
                        var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                        gcfg.BlockDiscordInvites = false;
                        await Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg)
                            .ConfigureAwait(false);

                        var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                        .ConfigureAwait(false);
                        if (logchn != null) {
                            var emb = new DiscordEmbedBuilder() {
                                Title = "Guild config changed",
                                Color = DiscordColor.Brown
                            };
                            emb.AddField("User responsible", ctx.User.Mention, inline: true);
                            emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                            emb.AddField("Invite filtering", gcfg.BlockDiscordInvites ? "on" : "off", inline: true);
                            await logchn.SendMessageAsync(embed: emb.Build())
                                .ConfigureAwait(false);
                        }

                        await ctx.RespondWithIconEmbedAsync("Disabled Discord invites filtering!")
                            .ConfigureAwait(false);
                    }
                    #endregion
                }
                #endregion

                #region GROUP_CONFIG_LINKFILTER_DISTURBING
                [Group("disturbingsites"), Module(ModuleType.Administration)]
                [Description("Enable or disable shock website filtering.")]
                [Aliases("disturbing", "shock", "shocksites")]
                [UsageExample("!guild cfg linkfilter disturbing")]
                public class DisturbingSites : TheGodfatherBaseModule
                {

                    public DisturbingSites(SharedData shared, DBService db) : base(shared, db) { }


                    [GroupCommand]
                    public async Task ExecuteGroupAsync(CommandContext ctx)
                    {
                        var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                        await ctx.RespondWithIconEmbedAsync($"Shock website filtering for this guild is: {Formatter.Bold(gcfg.BlockDisturbingWebsites ? "enabled" : "disabled")}!")
                            .ConfigureAwait(false);
                    }


                    #region COMMAND_LINKFILTER_DISTURBING_ENABLE
                    [Command("enable"), Module(ModuleType.Administration)]
                    [Description("Enables shock website filtering for this guild.")]
                    [Aliases("on")]
                    [UsageExample("!guild cfg linkfilter disturbing on")]
                    public async Task EnableAsync(CommandContext ctx)
                    {
                        var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                        gcfg.BlockDisturbingWebsites = true;
                        await Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg)
                            .ConfigureAwait(false);

                        var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                        .ConfigureAwait(false);
                        if (logchn != null) {
                            var emb = new DiscordEmbedBuilder() {
                                Title = "Guild config changed",
                                Color = DiscordColor.Brown
                            };
                            emb.AddField("User responsible", ctx.User.Mention, inline: true);
                            emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                            emb.AddField("Disturbing website filtering", gcfg.BlockDisturbingWebsites ? "on" : "off", inline: true);
                            await logchn.SendMessageAsync(embed: emb.Build())
                                .ConfigureAwait(false);
                        }

                        await ctx.RespondWithIconEmbedAsync("Enabled disturbing website filtering!")
                            .ConfigureAwait(false);
                    }
                    #endregion

                    #region COMMAND_LINKFILTER_DISTURBING_DISABLE
                    [Command("disable"), Module(ModuleType.Administration)]
                    [Description("Disables shock website filtering for this guild.")]
                    [Aliases("off")]
                    [UsageExample("!guild cfg linkfilter disturbing off")]
                    public async Task DisableAsync(CommandContext ctx)
                    {
                        var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                        gcfg.BlockDisturbingWebsites = false;
                        await Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg)
                            .ConfigureAwait(false);

                        var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                        .ConfigureAwait(false);
                        if (logchn != null) {
                            var emb = new DiscordEmbedBuilder() {
                                Title = "Guild config changed",
                                Color = DiscordColor.Brown
                            };
                            emb.AddField("User responsible", ctx.User.Mention, inline: true);
                            emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                            emb.AddField("Disturbing website filtering", gcfg.BlockDisturbingWebsites ? "on" : "off", inline: true);
                            await logchn.SendMessageAsync(embed: emb.Build())
                                .ConfigureAwait(false);
                        }

                        await ctx.RespondWithIconEmbedAsync("Disabled disturbing website filtering!")
                            .ConfigureAwait(false);
                    }
                    #endregion
                }
                #endregion

                #region GROUP_CONFIG_LINKFILTER_IPLOGGERS
                [Group("iploggers"), Module(ModuleType.Administration)]
                [Description("Enable or disable filtering of IP logger websites.")]
                [Aliases("ip", "loggers", "ipleech")]
                [UsageExample("!guild cfg linkfilter iploggers")]
                public class IpLoggers : TheGodfatherBaseModule
                {

                    public IpLoggers(SharedData shared, DBService db) : base(shared, db) { }


                    [GroupCommand]
                    public async Task ExecuteGroupAsync(CommandContext ctx)
                    {
                        var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                        await ctx.RespondWithIconEmbedAsync($"IP logging websites filtering for this guild is: {Formatter.Bold(gcfg.BlockIpLoggingWebsites ? "enabled" : "disabled")}!")
                            .ConfigureAwait(false);
                    }


                    #region COMMAND_LINKFILTER_IPLOGGERS_ENABLE
                    [Command("enable"), Module(ModuleType.Administration)]
                    [Description("Enables IP logger websites filtering for this guild.")]
                    [Aliases("on")]
                    [UsageExample("!guild cfg linkfilter iploggers on")]
                    public async Task EnableAsync(CommandContext ctx)
                    {
                        var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                        gcfg.BlockIpLoggingWebsites = true;
                        await Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg)
                            .ConfigureAwait(false);

                        var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                        .ConfigureAwait(false);
                        if (logchn != null) {
                            var emb = new DiscordEmbedBuilder() {
                                Title = "Guild config changed",
                                Color = DiscordColor.Brown
                            };
                            emb.AddField("User responsible", ctx.User.Mention, inline: true);
                            emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                            emb.AddField("IP logging website filtering", gcfg.BlockIpLoggingWebsites ? "on" : "off", inline: true);
                            await logchn.SendMessageAsync(embed: emb.Build())
                                .ConfigureAwait(false);
                        }

                        await ctx.RespondWithIconEmbedAsync("Enabled IP logging website filtering!")
                            .ConfigureAwait(false);
                    }
                    #endregion

                    #region COMMAND_LINKFILTER_IPLOGGERS_DISABLE
                    [Command("disable"), Module(ModuleType.Administration)]
                    [Description("Disables IP logging website filtering for this guild.")]
                    [Aliases("off")]
                    [UsageExample("!guild cfg linkfilter iploggers off")]
                    public async Task DisableAsync(CommandContext ctx)
                    {
                        var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                        gcfg.BlockIpLoggingWebsites = false;
                        await Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg)
                            .ConfigureAwait(false);

                        var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                        .ConfigureAwait(false);
                        if (logchn != null) {
                            var emb = new DiscordEmbedBuilder() {
                                Title = "Guild config changed",
                                Color = DiscordColor.Brown
                            };
                            emb.AddField("User responsible", ctx.User.Mention, inline: true);
                            emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                            emb.AddField("IP logging website filtering", gcfg.BlockIpLoggingWebsites ? "on" : "off", inline: true);
                            await logchn.SendMessageAsync(embed: emb.Build())
                                .ConfigureAwait(false);
                        }

                        await ctx.RespondWithIconEmbedAsync("Disabled IP logging website filtering!")
                            .ConfigureAwait(false);
                    }
                    #endregion
                }
                #endregion

                #region GROUP_CONFIG_LINKFILTER_SHORTENERS
                [Group("shorteners"), Module(ModuleType.Administration)]
                [Description("Enable or disable filtering of URL shortener websites.")]
                [Aliases("urlshort", "shortenurl", "urlshorteners")]
                [UsageExample("!guild cfg linkfilter shorteners")]
                public class Shorteners : TheGodfatherBaseModule
                {

                    public Shorteners(SharedData shared, DBService db) : base(shared, db) { }


                    [GroupCommand]
                    public async Task ExecuteGroupAsync(CommandContext ctx)
                    {
                        var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                        await ctx.RespondWithIconEmbedAsync($"URL shortening websites filtering for this guild is: {Formatter.Bold(gcfg.BlockUrlShorteners ? "enabled" : "disabled")}!")
                            .ConfigureAwait(false);
                    }


                    #region COMMAND_LINKFILTER_SHORTENERS_ENABLE
                    [Command("enable"), Module(ModuleType.Administration)]
                    [Description("Enables URL shortener websites filtering for this guild.")]
                    [Aliases("on")]
                    [UsageExample("!guild cfg linkfilter shorteners on")]
                    public async Task EnableAsync(CommandContext ctx)
                    {
                        var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                        gcfg.BlockUrlShorteners = true;
                        await Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg)
                            .ConfigureAwait(false);

                        var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                        .ConfigureAwait(false);
                        if (logchn != null) {
                            var emb = new DiscordEmbedBuilder() {
                                Title = "Guild config changed",
                                Color = DiscordColor.Brown
                            };
                            emb.AddField("User responsible", ctx.User.Mention, inline: true);
                            emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                            emb.AddField("URL shortener website filtering", gcfg.BlockUrlShorteners ? "on" : "off", inline: true);
                            await logchn.SendMessageAsync(embed: emb.Build())
                                .ConfigureAwait(false);
                        }

                        await ctx.RespondWithIconEmbedAsync("Enabled URL shortener website filtering!")
                            .ConfigureAwait(false);
                    }
                    #endregion

                    #region COMMAND_LINKFILTER_SHORTENERS_DISABLE
                    [Command("disable"), Module(ModuleType.Administration)]
                    [Description("Disables URL shortener website filtering for this guild.")]
                    [Aliases("off")]
                    [UsageExample("!guild cfg linkfilter shorteners off")]
                    public async Task DisableAsync(CommandContext ctx)
                    {
                        var gcfg = Shared.GetGuildConfig(ctx.Guild.Id);
                        gcfg.BlockUrlShorteners = false;
                        await Database.UpdateGuildSettingsAsync(ctx.Guild.Id, gcfg)
                            .ConfigureAwait(false);

                        var logchn = await Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild.Id)
                        .ConfigureAwait(false);
                        if (logchn != null) {
                            var emb = new DiscordEmbedBuilder() {
                                Title = "Guild config changed",
                                Color = DiscordColor.Brown
                            };
                            emb.AddField("User responsible", ctx.User.Mention, inline: true);
                            emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                            emb.AddField("URL shortener website filtering", gcfg.BlockUrlShorteners ? "on" : "off", inline: true);
                            await logchn.SendMessageAsync(embed: emb.Build())
                                .ConfigureAwait(false);
                        }

                        await ctx.RespondWithIconEmbedAsync("Disabled URL shortener website filtering!")
                            .ConfigureAwait(false);
                    }
                    #endregion
                }
                #endregion
            }
        }
    }
}
