using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Administration
{
    [Group("config"), Module(ModuleType.Administration)]
    [Aliases("configuration", "configure", "settings", "cfg")]
    [RequireGuild, RequireUserPermissions(Permissions.ManageGuild)]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public sealed partial class ConfigModule : TheGodfatherServiceModule<GuildConfigService>
    {
        #region config
        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            GuildConfig gcfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
            await ctx.RespondAsync(embed: gcfg.ToDiscordEmbed(ctx.Guild, this.Localization));
        }
        #endregion

        #region config setup
        [Command("setup"), UsesInteractivity]
        [Aliases("wizard")]
        public async Task SetupAsync(CommandContext ctx,
                                    [Description("desc-setup-chn")] DiscordChannel? channel = null)
        {
            channel ??= ctx.Channel;

            var gcfg = new GuildConfig();
            await channel.LocalizedEmbedAsync(this.Localization, "str-setup");
            await Task.Delay(TimeSpan.FromSeconds(10));

            await this.SetupPrefixAsync(gcfg, ctx, channel);
            await this.SetupLoggingAsync(gcfg, ctx, channel);
            await this.SetupBackupAsync(gcfg, ctx, channel);

            gcfg.ReactionResponse = await ctx.WaitForBoolReplyAsync("q-setup-verbose", channel, false);
            gcfg.SuggestionsEnabled = await ctx.WaitForBoolReplyAsync("q-setup-suggestions", channel, false);
            gcfg.ActionHistoryEnabled = await ctx.WaitForBoolReplyAsync("q-setup-ah", channel, false);
            gcfg.SilentLevelUpEnabled = await ctx.WaitForBoolReplyAsync("q-setup-lvlup", channel, false);

            await this.SetupMemberUpdateMessagesAsync(gcfg, ctx, channel);
            await this.SetupMuteRoleAsync(gcfg, ctx, channel);
            await this.SetupLinkfilterAsync(gcfg, ctx, channel);
            await this.SetupAntispamAsync(gcfg, ctx, channel);
            await this.SetupAntiMentionAsync(gcfg, ctx, channel);
            await this.SetupRatelimitAsync(gcfg, ctx, channel);
            await this.SetupAntifloodAsync(gcfg, ctx, channel);
            await this.SetupAntiInstantLeaveAsync(gcfg, ctx, channel);
            await this.SetupCurrencyAsync(gcfg, ctx, channel);

            await channel.SendMessageAsync(embed: gcfg.ToDiscordEmbed(ctx.Guild, this.Localization));

            if (await ctx.WaitForBoolReplyAsync("q-setup-review", channel: channel)) {
                await this.ApplySettingsAsync(ctx, gcfg);
                await channel.EmbedAsync(this.Localization.GetString(ctx.Guild.Id, "str-done"), Emojis.CheckMarkSuccess);
            } else {
                await channel.InformFailureAsync(this.Localization.GetString(ctx.Guild.Id, "str-aborting"));
            }
        }
        #endregion

        #region config level
        [Command("levelup"), Priority(1)]
        [Aliases("lvlup", "lvl")]
        public async Task LevelUpAsync(CommandContext ctx,
                                      [Description("desc-lvlup-s")] bool enable)
        {
            await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.SilentLevelUpEnabled = !enable;
            });

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle("evt-cfg-upd");
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField("str-lvlup-s", enable ? "str-on" : "str-off", inline: true);
            });

            await ctx.InfoAsync(this.ModuleColor, enable ? "str-cfg-lvlup-on" : "str-cfg-lvlup-off");
        }

        [Command("levelup"), Priority(0)]
        public async Task LevelUpAsync(CommandContext ctx)
        {
            GuildConfig gcfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
            await ctx.InfoAsync(this.ModuleColor, gcfg.SilentLevelUpEnabled ? "str-cfg-lvlup-get-on" : "str-cfg-lvlup-get-off");
        }
        #endregion

        #region config silent
        [Command("silent"), Priority(1)]
        [Aliases("reactionresponse", "silentresponse", "s", "rr")]
        public async Task SilentResponseAsync(CommandContext ctx,
                                             [Description("desc-replies-s")] bool enable)
        {
            await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.ReactionResponse = enable;
            });

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle("evt-cfg-upd");
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField("str-replies-s", enable ? "str-on" : "str-off", inline: true);
            });

            await ctx.InfoAsync(this.ModuleColor, enable ? "str-cfg-silent-on" : "str-cfg-silent-off");
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

        #region config currency
        [Command("currency"), Priority(1)]
        [Aliases("setcurrency", "curr", "$", "$$", "$$$")]
        public async Task CurrencyAsync(CommandContext ctx,
                                       [Description("desc-currency")] string currency)
        {
            if (string.IsNullOrWhiteSpace(currency) || currency.Length > GuildConfig.CurrencyLimit)
                throw new CommandFailedException(ctx, "cmd-err-currency");

            await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.Currency = currency);

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle("evt-cfg-upd");
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField("str-currency", currency, inline: true);
            });

            await this.CurrencyAsync(ctx);
        }

        [Command("currency"), Priority(0)]
        public Task CurrencyAsync(CommandContext ctx)
        {
            CachedGuildConfig gcfg = this.Service.GetCachedConfig(ctx.Guild.Id);
            return ctx.InfoAsync(this.ModuleColor, Emojis.MoneyBag, "str-currency-get", gcfg.Currency);
        }
        #endregion

        #region config suggestions
        [Command("suggestions"), Priority(1)]
        [Aliases("suggestion", "cmdsug", "sugg", "sug", "help")]
        public async Task SuggestionsAsync(CommandContext ctx,
                                          [Description("desc-suggestions")] bool enable)
        {
            await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.SuggestionsEnabled = enable);

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

        #region config actionhistory
        [Command("actionhistory"), Priority(1)]
        [Aliases("history", "ah")]
        public async Task ActionHistoryAsync(CommandContext ctx,
                                            [Description("desc-actionhistory")] bool enable)
        {
            await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.ActionHistoryEnabled = enable);

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle("evt-cfg-upd");
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField("str-actionhistory", enable ? "str-on" : "str-off", inline: true);
            });

            await ctx.InfoAsync(this.ModuleColor, enable ? "str-cfg-ah-on" : "str-cfg-ah-off");
        }

        [Command("actionhistory"), Priority(0)]
        public async Task ActionHistoryAsync(CommandContext ctx)
        {
            GuildConfig gcfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
            await ctx.InfoAsync(this.ModuleColor, gcfg.ActionHistoryEnabled ? "str-cfg-ah-get-on" : "str-cfg-ah-get-off");
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


        #region internals
        private async Task SetupPrefixAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            if (await ctx.WaitForBoolReplyAsync("q-setup-prefix", channel: channel, reply: false)) {
                await channel.LocalizedEmbedAsync(this.Localization, "q-setup-prefix-new", GuildConfig.PrefixLimit);
                InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User, m => m.Content.Length <= GuildConfig.PrefixLimit);
                gcfg.Prefix = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : mctx.Result.Content;
            }
        }

        private async Task SetupLoggingAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            if (await ctx.WaitForBoolReplyAsync("q-setup-log", channel: channel, reply: false)) {
                await channel.LocalizedEmbedAsync(this.Localization, "q-setup-log-chn");
                DiscordChannel? logchn = await ctx.Client.GetInteractivity().WaitForChannelMentionAsync(channel, ctx.User);
                gcfg.LogChannelId = logchn?.Id ?? default;
            }
        }

        private async Task SetupBackupAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            if (await ctx.WaitForBoolReplyAsync("q-setup-bak", channel: channel, reply: false)) {
                gcfg.BackupEnabled = true;
                if (await ctx.WaitForBoolReplyAsync("q-setup-bak-ex", channel: channel, reply: false)) {
                    await channel.LocalizedEmbedAsync(this.Localization, "q-setup-bak-ex-list");
                    InteractivityResult<DiscordMessage> res = await ctx.Client.GetInteractivity().WaitForMessageAsync(
                        msg => msg.Author == ctx.User && msg.Channel == channel && msg.MentionedChannels.Any()
                    );
                    if (!res.TimedOut) {
                        BackupService bs = ctx.Services.GetRequiredService<BackupService>();
                        await bs.ExemptAsync(ctx.Guild.Id, res.Result.MentionedChannels.SelectIds());
                    }
                }
            }
        }

        private async Task SetupMemberUpdateMessagesAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            await GetChannelIdAndMessageAsync(welcome: true);
            await GetChannelIdAndMessageAsync(welcome: false);


            async Task GetChannelIdAndMessageAsync(bool welcome)
            {
                InteractivityExtension interactivity = ctx.Client.GetInteractivity();

                if (await ctx.WaitForBoolReplyAsync(welcome ? "q-setup-memupd-wm" : "q-setup-memupd-lm", channel: channel, reply: false)) {
                    await channel.LocalizedEmbedAsync(this.Localization, "q-setup-memupd-chn");

                    DiscordChannel? chn = await interactivity.WaitForChannelMentionAsync(channel, ctx.User);
                    if (chn is { } && chn.Type == ChannelType.Text) {
                        if (welcome)
                            gcfg.WelcomeChannelId = chn?.Id ?? default;
                        else
                            gcfg.LeaveChannelId = chn?.Id ?? default;
                    }

                    if (await ctx.WaitForBoolReplyAsync("q-setup-memupd-msg", channel: channel, reply: false)) {
                        await channel.LocalizedEmbedAsync(this.Localization, "q-setup-memupd-msg-new");
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

        private async Task SetupMuteRoleAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            DiscordRole? muteRole = null;

            if (await ctx.WaitForBoolReplyAsync("q-setup-muterole", channel: channel, reply: false)) {
                await channel.LocalizedEmbedAsync(this.Localization, "q-setup-muterole-new");
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

        private async Task SetupLinkfilterAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
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

        private async Task SetupRatelimitAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            if (await ctx.WaitForBoolReplyAsync("q-setup-rl", channel: channel, reply: false)) {
                gcfg.RatelimitEnabled = true;

                if (await ctx.WaitForBoolReplyAsync("q-setup-rl-action", channel: channel, reply: false, args: gcfg.RatelimitAction.Humanize())) {
                    await channel.LocalizedEmbedAsync(this.Localization, "q-setup-new-action", args: Enum.GetNames<Punishment.Action>().JoinWith(", "));
                    Punishment.Action? action = await ctx.Client.GetInteractivity().WaitForPunishmentActionAsync(channel, ctx.User);
                    if (action is { })
                        gcfg.RatelimitAction = action.Value;
                }

                if (await ctx.WaitForBoolReplyAsync("q-setup-rl-sens", channel: channel, reply: false, args: gcfg.RatelimitSensitivity)) {
                    await channel.LocalizedEmbedAsync(this.Localization, "q-setup-new-sens", RatelimitSettings.MinSensitivity, RatelimitSettings.MaxSensitivity);
                    InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                        m => short.TryParse(m.Content, out short sens) && sens >= RatelimitSettings.MinSensitivity && sens <= RatelimitSettings.MaxSensitivity
                    );
                    gcfg.RatelimitSensitivity = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : short.Parse(mctx.Result.Content);
                }
            }
        }

        private async Task SetupAntispamAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            if (await ctx.WaitForBoolReplyAsync("q-setup-as", channel: channel, reply: false)) {
                gcfg.AntispamEnabled = true;

                if (await ctx.WaitForBoolReplyAsync("q-setup-as-action", channel: channel, reply: false, args: gcfg.AntispamAction.Humanize())) {
                    await channel.LocalizedEmbedAsync(this.Localization, "q-setup-new-action", Enum.GetNames<Punishment.Action>().JoinWith(", "));
                    Punishment.Action? action = await ctx.Client.GetInteractivity().WaitForPunishmentActionAsync(channel, ctx.User);
                    if (action is { })
                        gcfg.AntispamAction = action.Value;
                }

                if (await ctx.WaitForBoolReplyAsync("q-setup-as-sens", channel: channel, reply: false, args: gcfg.AntispamSensitivity)) {
                    await channel.LocalizedEmbedAsync(this.Localization, "q-setup-new-sens", AntispamSettings.MinSensitivity, AntispamSettings.MaxSensitivity);
                    InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                        m => short.TryParse(m.Content, out short sens) && sens >= AntispamSettings.MinSensitivity && sens <= AntispamSettings.MaxSensitivity
                    );
                    gcfg.AntispamSensitivity = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : short.Parse(mctx.Result.Content);
                }
            }
        }

        private async Task SetupAntiMentionAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            if (await ctx.WaitForBoolReplyAsync("q-setup-am", channel: channel, reply: false)) {
                gcfg.AntispamEnabled = true;

                if (await ctx.WaitForBoolReplyAsync("q-setup-am-action", channel: channel, reply: false, args: gcfg.AntiMentionAction.Humanize())) {
                    await channel.LocalizedEmbedAsync(this.Localization, "q-setup-new-action", Enum.GetNames<Punishment.Action>().JoinWith(", "));
                    Punishment.Action? action = await ctx.Client.GetInteractivity().WaitForPunishmentActionAsync(channel, ctx.User);
                    if (action is { })
                        gcfg.AntiMentionAction = action.Value;
                }

                if (await ctx.WaitForBoolReplyAsync("q-setup-am-sens", channel: channel, reply: false, args: gcfg.AntiMentionSensitivity)) {
                    await channel.LocalizedEmbedAsync(this.Localization, "q-setup-new-sens", AntispamSettings.MinSensitivity, AntispamSettings.MaxSensitivity);
                    InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                        m => short.TryParse(m.Content, out short sens) && sens >= AntispamSettings.MinSensitivity && sens <= AntispamSettings.MaxSensitivity
                    );
                    gcfg.AntiMentionSensitivity = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : short.Parse(mctx.Result.Content);
                }
            }
        }

        private async Task SetupAntifloodAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            if (await ctx.WaitForBoolReplyAsync("q-setup-af", channel: channel, reply: false)) {
                gcfg.AntifloodEnabled = true;

                if (await ctx.WaitForBoolReplyAsync("q-setup-af-action", channel: channel, reply: false, args: gcfg.AntifloodAction.Humanize())) {
                    await channel.LocalizedEmbedAsync(this.Localization, "q-setup-new-action", Enum.GetNames<Punishment.Action>().JoinWith(", "));
                    Punishment.Action? action = await ctx.Client.GetInteractivity().WaitForPunishmentActionAsync(channel, ctx.User);
                    if (action is { })
                        gcfg.AntifloodAction = action.Value;
                }

                if (await ctx.WaitForBoolReplyAsync("q-setup-af-sens", channel: channel, reply: false, args: gcfg.AntifloodSensitivity)) {
                    await channel.LocalizedEmbedAsync(this.Localization, "q-setup-new-sens", AntifloodSettings.MinSensitivity, AntifloodSettings.MaxSensitivity);
                    InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                        m => short.TryParse(m.Content, out short sens) && sens >= AntifloodSettings.MinSensitivity && sens <= AntifloodSettings.MaxSensitivity
                    );
                    gcfg.AntifloodSensitivity = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : short.Parse(mctx.Result.Content);
                }

                if (await ctx.WaitForBoolReplyAsync("q-setup-af-cd", channel: channel, reply: false, args: gcfg.AntifloodCooldown)) {
                    await channel.LocalizedEmbedAsync(this.Localization, "q-setup-new-cd", AntifloodSettings.MinCooldown, AntifloodSettings.MaxCooldown);
                    InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                        m => short.TryParse(m.Content, out short cd) && cd >= AntifloodSettings.MinCooldown && cd <= AntifloodSettings.MaxCooldown
                    );
                    gcfg.AntifloodCooldown = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : short.Parse(mctx.Result.Content);
                }
            }
        }

        private async Task SetupAntiInstantLeaveAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            if (await ctx.WaitForBoolReplyAsync("q-setup-il", channel: channel, reply: false)) {
                gcfg.AntiInstantLeaveEnabled = true;

                if (await ctx.WaitForBoolReplyAsync("q-setup-il-cd", channel: channel, reply: false, args: gcfg.AntifloodCooldown)) {
                    await channel.LocalizedEmbedAsync(this.Localization, "q-setup-new-cd", AntiInstantLeaveSettings.MinCooldown, AntiInstantLeaveSettings.MaxCooldown);
                    InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                        m => short.TryParse(m.Content, out short cd)
                          && cd >= AntiInstantLeaveSettings.MinCooldown && cd <= AntiInstantLeaveSettings.MaxCooldown
                    );
                    gcfg.AntiInstantLeaveCooldown = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : short.Parse(mctx.Result.Content);
                }
            }
        }

        private async Task SetupCurrencyAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
        {
            if (await ctx.WaitForBoolReplyAsync("q-setup-currency", channel: channel, reply: false)) {
                await channel.LocalizedEmbedAsync(this.Localization, "q-setup-currency-new", GuildConfig.CurrencyLimit);
                InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User, m => m.Content.Length <= GuildConfig.CurrencyLimit);
                gcfg.Currency = mctx.TimedOut ? throw new CommandFailedException(ctx, "str-timeout") : mctx.Result.Content;
            }
        }

        private async Task ApplySettingsAsync(CommandContext ctx, GuildConfig gcfg)
        {
            await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.ActionHistoryEnabled = gcfg.ActionHistoryEnabled;
                cfg.AntifloodSettings = gcfg.AntifloodSettings;
                cfg.AntiInstantLeaveSettings = gcfg.AntiInstantLeaveSettings;
                cfg.BackupEnabled = gcfg.BackupEnabled;
                cfg.CachedConfig = gcfg.CachedConfig;
                cfg.LeaveChannelId = gcfg.LeaveChannelId;
                cfg.LeaveMessage = gcfg.LeaveMessage;
                cfg.MuteRoleId = gcfg.MuteRoleId;
                cfg.SilentLevelUpEnabled = gcfg.SilentLevelUpEnabled;
                cfg.WelcomeChannelId = gcfg.WelcomeChannelId;
                cfg.WelcomeMessage = gcfg.WelcomeMessage;
            });

            LoggingService ls = ctx.Services.GetRequiredService<LoggingService>();
            await ls.LogAsync(ctx.Guild, gcfg.ToDiscordEmbed(ctx.Guild, this.Localization, update: true));
        }
        #endregion
    }
}

