using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Administration
{
    [Group("config")]
    [Aliases("configuration", "configure", "settings", "cfg")]
    [RequireGuild, RequireUserPermissions(Permissions.ManageGuild)]
    public partial class ConfigModule : TheGodfatherServiceModule<GuildConfigService>
    {
        public ConfigModule(GuildConfigService service)
            : base(service) { }


        #region config
        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            GuildConfig gcfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
            await ctx.RespondAsync(embed: gcfg.ToDiscordEmbed(ctx.Guild, ctx.Services.GetRequiredService<LocalizationService>()));
        }
        #endregion

        #region config setup
        [Command("setup"), UsesInteractivity]
        [Aliases("wizard")]
        public async Task SetupAsync(CommandContext ctx,
                                    [Description("desc-setup-chn")] DiscordChannel? channel = null)
        {
            channel ??= ctx.Channel;

            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();

            var gcfg = new GuildConfig();
            await channel.LocalizedEmbedAsync(lcs, "str-setup");
            await Task.Delay(TimeSpan.FromSeconds(10));

            await SetupPrefixAsync(gcfg, ctx, channel);
            await SetupLoggingAsync(gcfg, ctx, channel);

            gcfg.ReactionResponse = await ctx.WaitForBoolReplyAsync("q-setup-verbose", channel, false);
            gcfg.SuggestionsEnabled = await ctx.WaitForBoolReplyAsync("q-setup-suggestions", channel, false);

            await SetupMemberUpdateMessagesAsync(gcfg, ctx, channel);
            await SetupMuteRoleAsync(gcfg, ctx, channel);
            await SetupLinkfilterAsync(gcfg, ctx, channel);
            await SetupAntispamAsync(gcfg, ctx, channel);
            await SetupRatelimitAsync(gcfg, ctx, channel);
            await SetupAntifloodAsync(gcfg, ctx, channel);
            await SetupAntiInstantLeaveAsync(gcfg, ctx, channel);
            await SetupCurrencyAsync(gcfg, ctx, channel);

            await channel.SendMessageAsync(embed: gcfg.ToDiscordEmbed(ctx.Guild, lcs));

            if (await ctx.WaitForBoolReplyAsync("q-setup-review", channel: channel)) {
                await this.ApplySettingsAsync(ctx, gcfg);
                await channel.EmbedAsync(lcs.GetString(ctx.Guild.Id, "str-done"), Emojis.CheckMarkSuccess);
            } else {
                await channel.InformFailureAsync(lcs.GetString(ctx.Guild.Id, "str-aborting"));
            }
        }
        #endregion

        #region config silent
        [Command("silent"), Priority(1)]
        [Aliases("reactionresponse", "silentresponse", "s", "rr")]
        public async Task SilentResponseAsync(CommandContext ctx,
                                             [Description("desc-replies-s")] bool enable)
        {
            GuildConfig gcfg = await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.ReactionResponse = enable;
            });

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle("evt-cfg-upd");
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField("str-silent", enable ? "str-on" : "str-off", inline: true);
            });

            await ctx.InfoAsync(this.ModuleColor, enable ?  "str-cfg-silent-on" : "str-cfg-silent-off");
        }

        [Command("silent"), Priority(0)]
        public Task SilentResponseAsync(CommandContext ctx)
        {
            CachedGuildConfig gcfg = this.Service.GetCachedConfig(ctx.Guild.Id);
            return ctx.InfoAsync(this.ModuleColor, gcfg.ReactionResponse ? "str-cfg-silent-get-on" : "str-cfg-silent-get-off");
        }
        #endregion

        #region config verbose
        [Command("verbose"), Priority(1)]
        [Aliases("fullresponse", "verboseresponse", "v", "vr")]
        public Task VerboseResponseAsync(CommandContext ctx,
                                        [Description("desc-replies-v")] bool enable)
            => this.SilentResponseAsync(ctx, !enable);

        [Command("verbose"), Priority(0)]
        public Task VerboseResponseAsync(CommandContext ctx)
            => this.SilentResponseAsync(ctx);
        #endregion

        #region config suggestions
        [Command("suggestions"), Priority(1)]
        [Aliases("suggestion", "cmdsug", "sugg", "sug", "help")]
        public async Task SuggestionsAsync(CommandContext ctx,
                                          [Description("desc-suggestions")] bool enable)
        {
            GuildConfig gcfg = await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.SuggestionsEnabled = enable);

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle("evt-cfg-upd");
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField("str-suggestions", enable ? "str-on" : "str-off", inline: true);
            });

            await ctx.InfoAsync(this.ModuleColor, enable ? "str-cfg-suggest-on" : "str-cfg-suggest-off");
        }

        [Command("suggestions"), Priority(0)]
        public Task SuggestionsAsync(CommandContext ctx)
        {
            CachedGuildConfig gcfg = this.Service.GetCachedConfig(ctx.Guild.Id);
            return ctx.InfoAsync(this.ModuleColor, gcfg.SuggestionsEnabled ? "str-cfg-suggest-get-on" : "str-cfg-suggest-get-off");
        }
        #endregion

        #region config muterole
        [Command("muterole")]
        [Aliases("mr", "muterl", "mrl")]
        public async Task GetOrSetMuteRoleAsync(CommandContext ctx,
                                               [Description("desc-muterole")] DiscordRole? muteRole = null)
        {
            if (muteRole is null) {
                GuildConfig cfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
                if (cfg.MuteRoleId == 0) {
                    await ctx.InfoAsync(this.ModuleColor, "str-cfg-muterole-none");
                    return;
                }

                muteRole = ctx.Guild.GetRole(cfg.MuteRoleId);
                if (muteRole is null) { 
                    await ctx.FailAsync("err-muterole-404");
                    return;
                }
            } else {
                await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.MuteRoleId = muteRole.Id);
            }
    
            await ctx.InfoAsync(this.ModuleColor, "fmt-muterole", muteRole.Name);
        }
        #endregion

        #region config reset
        [Command("reset"), UsesInteractivity]
        [Aliases("default", "def", "s", "rr")]
        public async Task ResetAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("q-setup-reset"))
                return;

            await this.ApplySettingsAsync(ctx, new GuildConfig());

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle("evt-cfg-reset");
                emb.WithColor(this.ModuleColor);
            });

            await ctx.InfoAsync(this.ModuleColor, "str-cfg-reset");
        }
        #endregion


        #region Helpers
        private static async Task SetupPrefixAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            if (await ctx.WaitForBoolReplyAsync("q-setup-prefix", channel: channel, reply: false)) {
                await channel.LocalizedEmbedAsync(ctx.Services.GetRequiredService<LocalizationService>(), "q-setup-prefix-new");
                InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User, m => m.Content.Length <= 8);
                gcfg.Prefix = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : mctx.Result.Content;
            }
        }

        private static async Task SetupLoggingAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            if (await ctx.WaitForBoolReplyAsync("q-setup-log", channel: channel, reply: false)) {
                await channel.LocalizedEmbedAsync(ctx.Services.GetRequiredService<LocalizationService>(), "q-setup-log-chn");
                DiscordChannel? logchn = await ctx.Client.GetInteractivity().WaitForChannelMentionAsync(channel, ctx.User);
                gcfg.LogChannelId = logchn?.Id ?? 0;
            }
        }

        private static async Task SetupMemberUpdateMessagesAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            await GetChannelIdAndMessageAsync(welcome: true);
            await GetChannelIdAndMessageAsync(welcome: false);


            async Task GetChannelIdAndMessageAsync(bool welcome)
            {
                LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
                InteractivityExtension interactivity = ctx.Client.GetInteractivity();

                if (await ctx.WaitForBoolReplyAsync(welcome ? "q-setup-memupd-wm" : "q-setup-memupd-lm", channel: channel, reply: false)) {
                    await channel.LocalizedEmbedAsync(lcs, "q-setup-memupd-chn");

                    DiscordChannel? chn = await interactivity.WaitForChannelMentionAsync(channel, ctx.User);
                    if (chn is { } && chn.Type == ChannelType.Text) {
                        if (welcome)
                            gcfg.WelcomeChannelId = chn?.Id ?? 0;
                        else
                            gcfg.LeaveChannelId = chn?.Id ?? 0;
                    }

                    if (await ctx.WaitForBoolReplyAsync("q-setup-memupd-msg", channel: channel, reply: false)) {
                        await channel.LocalizedEmbedAsync(lcs, "q-setup-memupd-msg-new");
                        InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User, m => m.Content.Length <= 128);
                        if (mctx.TimedOut) {
                            throw new CommandFailedException(ctx, "str-timeout");
                        } else {
                            if (welcome)
                                gcfg.WelcomeMessage = mctx.Result.Content;
                            else
                                gcfg.LeaveMessage = mctx.Result.Content;
                        }
                    }
                }
            }
        }

        private static async Task SetupMuteRoleAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            DiscordRole? muteRole = null;

            if (await ctx.WaitForBoolReplyAsync("q-setup-muterole", channel: channel, reply: false)) {
                await channel.LocalizedEmbedAsync(ctx.Services.GetRequiredService<LocalizationService>(), "q-setup-muterole-new");
                InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User, m => m.MentionedRoles.Count == 1);
                muteRole = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : mctx.Result.MentionedRoles.FirstOrDefault();
            }

            try {
                muteRole ??= await ctx.Services.GetRequiredService<AntispamService>().GetOrCreateMuteRoleAsync(ctx.Guild);
            } catch (UnauthorizedException) {
                await channel.InformFailureAsync("cmd-err-role-403");
            }

            gcfg.MuteRoleId = muteRole?.Id ?? 0;
        }

        private static async Task SetupLinkfilterAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            if (await ctx.WaitForBoolReplyAsync("q-setup-lf", channel: channel, reply: false)) {
                gcfg.LinkfilterSettings.Enabled = true;
                gcfg.LinkfilterSettings.BlockDiscordInvites = await ctx.WaitForBoolReplyAsync("q-setup-lf-invites", channel: channel, reply: false);
                gcfg.LinkfilterSettings.BlockBooterWebsites = await ctx.WaitForBoolReplyAsync("q-setup-lf-ddos", channel: channel, reply: false);
                gcfg.LinkfilterSettings.BlockIpLoggingWebsites = await ctx.WaitForBoolReplyAsync("q-setup-lf-ip", channel: channel, reply: false);
                gcfg.LinkfilterSettings.BlockDisturbingWebsites = await ctx.WaitForBoolReplyAsync("q-setup-lf-gore", channel: channel, reply: false);
                gcfg.LinkfilterSettings.BlockUrlShorteners = await ctx.WaitForBoolReplyAsync("q-setup-lf-urlshort", channel: channel, reply: false);
            }
        }

        private static async Task SetupRatelimitAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
            if (await ctx.WaitForBoolReplyAsync("q-setup-rl", channel: channel, reply: false)) {
                gcfg.RatelimitEnabled = true;

                if (await ctx.WaitForBoolReplyAsync("q-setup-rl-action", channel: channel, reply: false, args: gcfg.RatelimitAction.ToTypeString())) {
                    await channel.LocalizedEmbedAsync(lcs, "q-setup-rl-action-new", args: Enum.GetNames<PunishmentAction>().Separate(", "));
                    PunishmentAction? action = await ctx.Client.GetInteractivity().WaitForPunishmentActionAsync(channel, ctx.User);
                    if (action is { })
                        gcfg.RatelimitAction = action.Value;
                }

                if (await ctx.WaitForBoolReplyAsync("q-setup-rl-sens", channel: channel, reply: false, args: gcfg.RatelimitSensitivity)) {
                    await channel.LocalizedEmbedAsync(lcs, "q-setup-rl-sens-new");
                    InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                        m => short.TryParse(m.Content, out short sens) && sens >= 4 && sens <= 10
                    );
                    gcfg.RatelimitSensitivity = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : short.Parse(mctx.Result.Content);
                }
            }
        }

        private static async Task SetupAntispamAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
            if (await ctx.WaitForBoolReplyAsync("q-setup-as", channel: channel, reply: false)) {
                gcfg.AntispamEnabled = true;

                if (await ctx.WaitForBoolReplyAsync("q-setup-as-action", channel: channel, reply: false, args: gcfg.AntispamAction.ToTypeString())) {
                    await channel.LocalizedEmbedAsync(lcs, "q-setup-as-action-new", args: Enum.GetNames<PunishmentAction>().Separate(", "));
                    PunishmentAction? action = await ctx.Client.GetInteractivity().WaitForPunishmentActionAsync(channel, ctx.User);
                    if (action is { })
                        gcfg.AntispamAction = action.Value;
                }

                if (await ctx.WaitForBoolReplyAsync("q-setup-as-sens", channel: channel, reply: false, args: gcfg.AntispamSensitivity)) {
                    await channel.LocalizedEmbedAsync(lcs, "q-setup-as-sens-new");
                    InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                        m => short.TryParse(m.Content, out short sens) && sens >= 3 && sens <= 10
                    );
                    gcfg.AntispamSensitivity = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : short.Parse(mctx.Result.Content);
                }
            }
        }

        private static async Task SetupAntifloodAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
            if (await ctx.WaitForBoolReplyAsync("q-setup-af", channel: channel, reply: false)) {
                gcfg.AntifloodEnabled = true;

                if (await ctx.WaitForBoolReplyAsync("q-setup-af-action", channel: channel, reply: false, args: gcfg.AntifloodAction.ToTypeString())) {
                    await channel.LocalizedEmbedAsync(lcs, "q-setup-af-action-new", args: Enum.GetNames<PunishmentAction>().Separate(", "));
                    PunishmentAction? action = await ctx.Client.GetInteractivity().WaitForPunishmentActionAsync(channel, ctx.User);
                    if (action is { })
                        gcfg.AntifloodAction = action.Value;
                }

                if (await ctx.WaitForBoolReplyAsync("q-setup-af-sens", channel: channel, reply: false, args: gcfg.AntifloodSensitivity)) {
                    await channel.LocalizedEmbedAsync(lcs, "q-setup-af-sens-new");
                    InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                        m => short.TryParse(m.Content, out short sens) && sens >= 2 && sens <= 20
                    );
                    gcfg.AntifloodSensitivity = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : short.Parse(mctx.Result.Content);
                }

                if (await ctx.WaitForBoolReplyAsync("q-setup-af-cd", channel: channel, reply: false, args: gcfg.AntifloodCooldown)) {
                    await channel.LocalizedEmbedAsync(lcs, "q-setup-af-cd-new");
                    InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                        m => short.TryParse(m.Content, out short sens) && sens >= 5 && sens <= 60
                    );
                    gcfg.AntifloodCooldown = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : short.Parse(mctx.Result.Content);
                }
            }
        }

        private static async Task SetupAntiInstantLeaveAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
            if (await ctx.WaitForBoolReplyAsync("q-setup-il", channel: channel, reply: false)) {
                gcfg.AntiInstantLeaveEnabled = true;

                if (await ctx.WaitForBoolReplyAsync("q-setup-il-cd", channel: channel, reply: false, args: gcfg.AntifloodCooldown)) {
                    await channel.LocalizedEmbedAsync(lcs, "q-setup-il-cd-new");
                    InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                        m => short.TryParse(m.Content, out short sens) && sens >= 2 && sens <= 20
                    );
                    gcfg.AntiInstantLeaveCooldown = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : short.Parse(mctx.Result.Content);
                }
            }
        }

        private static async Task SetupCurrencyAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            if (await ctx.WaitForBoolReplyAsync("q-setup-currency", channel: channel, reply: false)) {
                await channel.LocalizedEmbedAsync(ctx.Services.GetRequiredService<LocalizationService>(), "q-setup-currency-new");
                InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User, m => m.Content.Length <= 30);
                gcfg.Currency = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : mctx.Result.Content;
            }
        }

        private async Task ApplySettingsAsync(CommandContext ctx, GuildConfig gcfg)
        {
            await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.AntifloodSettings = gcfg.AntifloodSettings;
                cfg.AntiInstantLeaveSettings = gcfg.AntiInstantLeaveSettings;
                cfg.CachedConfig = gcfg.CachedConfig;
                cfg.LeaveChannelId = gcfg.LeaveChannelId;
                cfg.LeaveMessage = gcfg.LeaveMessage;
                cfg.MuteRoleId = gcfg.MuteRoleId;
                cfg.WelcomeChannelId = gcfg.WelcomeChannelId;
                cfg.WelcomeMessage = gcfg.WelcomeMessage;
            });

            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
            LoggingService ls = ctx.Services.GetRequiredService<LoggingService>();
            await ls.LogAsync(ctx.Guild, gcfg.ToDiscordEmbed(ctx.Guild, lcs, update: true));
        }
        #endregion
    }
}

