using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Administration;

[Group("config")][Module(ModuleType.Administration)]
[Aliases("configuration", "configure", "settings", "cfg")]
[RequireGuild][RequireUserPermissions(Permissions.ManageGuild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed partial class ConfigModule : TheGodfatherServiceModule<GuildConfigService>
{
    #region config
    [GroupCommand]
    public async Task ExecuteGroupAsync(CommandContext ctx)
    {
        GuildConfig gcfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
        await ctx.RespondAsync(gcfg.ToDiscordEmbed(ctx.Guild, this.Localization));
    }
    #endregion

    #region config setup
    [Command("setup")][UsesInteractivity]
    [Aliases("wizard")]
    public async Task SetupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_setup_chn)] DiscordChannel? channel = null)
    {
        channel ??= ctx.Channel;

        var gcfg = new GuildConfig();
        await channel.LocalizedEmbedAsync(this.Localization, TranslationKey.str_setup);
        await Task.Delay(TimeSpan.FromSeconds(10));

        await this.SetupPrefixAsync(gcfg, ctx, channel);
        await this.SetupLoggingAsync(gcfg, ctx, channel);
        await this.SetupBackupAsync(gcfg, ctx, channel);

        gcfg.ReactionResponse = await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_verbose, channel, false);
        gcfg.SuggestionsEnabled = await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_suggestions, channel, false);
        gcfg.ActionHistoryEnabled = await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_ah, channel, false);
        gcfg.SilentLevelUpEnabled = await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_lvlup, channel, false);

        await this.SetupMemberUpdateMessagesAsync(gcfg, ctx, channel);
        await this.SetupMuteRoleAsync(gcfg, ctx, channel);
        await this.SetupLinkfilterAsync(gcfg, ctx, channel);
        await this.SetupAntispamAsync(gcfg, ctx, channel);
        await this.SetupAntiMentionAsync(gcfg, ctx, channel);
        await this.SetupRatelimitAsync(gcfg, ctx, channel);
        await this.SetupAntifloodAsync(gcfg, ctx, channel);
        await this.SetupAntiInstantLeaveAsync(gcfg, ctx, channel);
        await this.SetupCurrencyAsync(gcfg, ctx, channel);

        await channel.SendMessageAsync(gcfg.ToDiscordEmbed(ctx.Guild, this.Localization));

        if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_review, channel)) {
            await this.ApplySettingsAsync(ctx, gcfg);
            await channel.EmbedAsync(this.Localization.GetString(ctx.Guild.Id, TranslationKey.str_done), Emojis.CheckMarkSuccess);
        } else {
            await channel.InformFailureAsync(this.Localization.GetString(ctx.Guild.Id, TranslationKey.str_aborting));
        }
    }
    #endregion

    #region config level
    [Command("levelup")][Priority(1)]
    [Aliases("lvlup", "lvl")]
    public async Task LevelUpAsync(CommandContext ctx,
        [Description(TranslationKey.desc_lvlup_s)] bool enable)
    {
        await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => {
            cfg.SilentLevelUpEnabled = !enable;
        });

        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_lvlup_s, enable ? TranslationKey.str_on : TranslationKey.str_off, true);
        });

        await ctx.InfoAsync(this.ModuleColor, enable ? TranslationKey.str_cfg_lvlup_on : TranslationKey.str_cfg_lvlup_off);
    }

    [Command("levelup")][Priority(0)]
    public async Task LevelUpAsync(CommandContext ctx)
    {
        GuildConfig gcfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
        await ctx.InfoAsync(this.ModuleColor, gcfg.SilentLevelUpEnabled ? TranslationKey.str_cfg_lvlup_get_on : TranslationKey.str_cfg_lvlup_get_off);
    }
    #endregion

    #region config silent
    [Command("silent")][Priority(1)]
    [Aliases("reactionresponse", "silentresponse", "s", "rr")]
    public async Task SilentResponseAsync(CommandContext ctx,
        [Description(TranslationKey.desc_replies_s)] bool enable)
    {
        await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => {
            cfg.ReactionResponse = enable;
        });

        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_replies_s, enable ? TranslationKey.str_on : TranslationKey.str_off, true);
        });

        await ctx.InfoAsync(this.ModuleColor, enable ? TranslationKey.str_cfg_silent_on : TranslationKey.str_cfg_silent_off);
    }

    [Command("silent")][Priority(0)]
    public Task SilentResponseAsync(CommandContext ctx)
    {
        CachedGuildConfig gcfg = this.Service.GetCachedConfig(ctx.Guild.Id);
        return ctx.InfoAsync(this.ModuleColor, gcfg.ReactionResponse ? TranslationKey.str_cfg_silent_get_on : TranslationKey.str_cfg_silent_get_off);
    }
    #endregion

    #region config verbose
    [Command("verbose")][Priority(1)]
    [Aliases("fullresponse", "verboseresponse", "v", "vr")]
    public Task VerboseResponseAsync(CommandContext ctx,
        [Description(TranslationKey.desc_replies_v)] bool enable)
        => this.SilentResponseAsync(ctx, !enable);

    [Command("verbose")][Priority(0)]
    public Task VerboseResponseAsync(CommandContext ctx)
        => this.SilentResponseAsync(ctx);
    #endregion

    #region config currency
    [Command("currency")][Priority(1)]
    [Aliases("setcurrency", "curr", "$", "$$", "$$$")]
    public async Task CurrencyAsync(CommandContext ctx,
        [Description(TranslationKey.desc_currency)] string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length > GuildConfig.CurrencyLimit)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_currency(GuildConfig.CurrencyLimit));

        await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.Currency = currency);

        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_currency, currency, true);
        });

        await this.CurrencyAsync(ctx);
    }

    [Command("currency")][Priority(0)]
    public Task CurrencyAsync(CommandContext ctx)
    {
        CachedGuildConfig gcfg = this.Service.GetCachedConfig(ctx.Guild.Id);
        return ctx.InfoAsync(this.ModuleColor, Emojis.MoneyBag, TranslationKey.str_currency_get(gcfg.Currency));
    }
    #endregion

    #region config suggestions
    [Command("suggestions")][Priority(1)]
    [Aliases("suggestion", "cmdsug", "sugg", "sug", "help")]
    public async Task SuggestionsAsync(CommandContext ctx,
        [Description(TranslationKey.desc_suggestions)] bool enable)
    {
        await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.SuggestionsEnabled = enable);

        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_suggestions, enable ? TranslationKey.str_on : TranslationKey.str_off, true);
        });

        await ctx.InfoAsync(this.ModuleColor, enable ? TranslationKey.str_cfg_suggest_on : TranslationKey.str_cfg_suggest_off);
    }

    [Command("suggestions")][Priority(0)]
    public Task SuggestionsAsync(CommandContext ctx)
    {
        CachedGuildConfig gcfg = this.Service.GetCachedConfig(ctx.Guild.Id);
        return ctx.InfoAsync(this.ModuleColor, gcfg.SuggestionsEnabled ? TranslationKey.str_cfg_suggest_get_on : TranslationKey.str_cfg_suggest_get_off);
    }
    #endregion

    #region config actionhistory
    [Command("actionhistory")][Priority(1)]
    [Aliases("history", "ah")]
    public async Task ActionHistoryAsync(CommandContext ctx,
        [Description(TranslationKey.desc_actionhistory)] bool enable)
    {
        await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.ActionHistoryEnabled = enable);

        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_actionhistory, enable ? TranslationKey.str_on : TranslationKey.str_off, true);
        });

        await ctx.InfoAsync(this.ModuleColor, enable ? TranslationKey.str_cfg_ah_on : TranslationKey.str_cfg_ah_off);
    }

    [Command("actionhistory")][Priority(0)]
    public async Task ActionHistoryAsync(CommandContext ctx)
    {
        GuildConfig gcfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
        await ctx.InfoAsync(this.ModuleColor, gcfg.ActionHistoryEnabled ? TranslationKey.str_cfg_ah_get_on : TranslationKey.str_cfg_ah_get_off);
    }
    #endregion

    #region config muterole
    [Command("muterole")]
    [Aliases("mr", "muterl", "mrl")]
    public async Task GetOrSetMuteRoleAsync(CommandContext ctx,
        [Description(TranslationKey.desc_muterole)] DiscordRole? muteRole = null)
    {
        if (muteRole is null) {
            GuildConfig cfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
            if (cfg.MuteRoleId == 0) {
                await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_cfg_muterole_none);
                return;
            }

            muteRole = ctx.Guild.GetRole(cfg.MuteRoleId);
            if (muteRole is null) {
                await ctx.FailAsync(TranslationKey.err_muterole_404);
                return;
            }
        } else {
            await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.MuteRoleId = muteRole.Id);
        }

        await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_muterole(muteRole.Name));
    }
    #endregion

    #region config tempmute
    [Command("tempmute")]
    [Aliases("tm", "tmute", "tmpmute", "tmpm")]
    public async Task GetOrSetMuteRoleCooldownAsync(CommandContext ctx,
        [Description(TranslationKey.desc_cooldown)] TimeSpan? cooldown = null)
    {
        if (cooldown is null) {
            GuildConfig cfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
            cooldown = cfg.TempMuteCooldown;
        } else {
            if (cooldown.Value.TotalSeconds is < GuildConfig.MinTempMuteCooldown or > GuildConfig.MaxTempMuteCooldown)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_range_cd(GuildConfig.MinTempMuteCooldown, GuildConfig.MaxTempMuteCooldown));

            await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.TempMuteCooldownDb = cooldown);
        }

        await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_tm_cooldown(cooldown.Value.ToDurationString()));
    }
    #endregion

    #region config tempban
    [Command("tempban")]
    [Aliases("tb", "tban", "tmpban", "tmpb")]
    public async Task GetOrSetMuteRoleAsync(CommandContext ctx,
        [Description(TranslationKey.desc_cooldown)] TimeSpan? cooldown = null)
    {
        if (cooldown is null) {
            GuildConfig cfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
            cooldown = cfg.TempMuteCooldown;
        } else {
            if (cooldown.Value.TotalSeconds is < GuildConfig.MinTempBanCooldown or > GuildConfig.MaxTempBanCooldown)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_range_cd(GuildConfig.MinTempBanCooldown, GuildConfig.MaxTempBanCooldown));

            await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.TempBanCooldownDb = cooldown);
        }

        await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_tb_cooldown(cooldown.Value.ToDurationString()));
    }
    #endregion

    #region config reset
    [Command("reset")][UsesInteractivity]
    [Aliases("default", "def", "s", "rr")]
    public async Task ResetAsync(CommandContext ctx)
    {
        if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_reset))
            return;

        await this.ApplySettingsAsync(ctx, new GuildConfig());

        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.evt_cfg_reset);
            emb.WithColor(this.ModuleColor);
        });

        await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_cfg_reset);
    }
    #endregion


    #region internals
    private async Task SetupPrefixAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
    {
        if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_prefix, channel, false)) {
            await channel.LocalizedEmbedAsync(this.Localization, TranslationKey.q_setup_prefix_new(GuildConfig.PrefixLimit));
            InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User, m => m.Content.Length <= GuildConfig.PrefixLimit);
            gcfg.Prefix = mctx.TimedOut ? throw new CommandFailedException(ctx, TranslationKey.str_timeout) : mctx.Result.Content;
        }
    }

    private async Task SetupLoggingAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
    {
        if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_log, channel, false)) {
            await channel.LocalizedEmbedAsync(this.Localization, TranslationKey.q_setup_log_chn);
            DiscordChannel? logchn = await ctx.Client.GetInteractivity().WaitForChannelMentionAsync(channel, ctx.User);
            gcfg.LogChannelId = logchn?.Id ?? default;
        }
    }

    private async Task SetupBackupAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
    {
        if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_bak, channel, false)) {
            gcfg.BackupEnabled = true;
            if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_bak_ex, channel, false)) {
                await channel.LocalizedEmbedAsync(this.Localization, TranslationKey.q_setup_bak_ex_list);
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
        await GetChannelIdAndMessageAsync(true);
        await GetChannelIdAndMessageAsync(false);


        async Task GetChannelIdAndMessageAsync(bool welcome)
        {
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();

            if (await ctx.WaitForBoolReplyAsync(welcome ? TranslationKey.q_setup_memupd_wm : TranslationKey.q_setup_memupd_lm, channel, false)) {
                await channel.LocalizedEmbedAsync(this.Localization, TranslationKey.q_setup_memupd_chn);

                DiscordChannel? chn = await interactivity.WaitForChannelMentionAsync(channel, ctx.User);
                if (chn is not null && chn.IsTextOrNewsChannel()) {
                    if (welcome)
                        gcfg.WelcomeChannelId = chn?.Id ?? default;
                    else
                        gcfg.LeaveChannelId = chn?.Id ?? default;
                }

                if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_memupd_msg, channel, false)) {
                    await channel.LocalizedEmbedAsync(this.Localization, TranslationKey.q_setup_memupd_msg_new);
                    InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User, m => m.Content.Length <= 128);
                    if (mctx.TimedOut) {
                        throw new CommandFailedException(ctx, TranslationKey.str_timeout);
                    }

                    if (welcome)
                        gcfg.WelcomeMessage = mctx.Result.Content;
                    else
                        gcfg.LeaveMessage = mctx.Result.Content;
                }
            }
        }
    }

    private async Task SetupMuteRoleAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
    {
        DiscordRole? muteRole = null;

        if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_muterole, channel, false)) {
            await channel.LocalizedEmbedAsync(this.Localization, TranslationKey.q_setup_muterole_new);
            InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User, m => m.MentionedRoles.Count == 1);
            muteRole = mctx.TimedOut ? throw new CommandFailedException(ctx, TranslationKey.str_timeout) : mctx.Result.MentionedRoles.FirstOrDefault();
        }

        try {
            muteRole ??= await ctx.Services.GetRequiredService<AntispamService>().GetOrCreateMuteRoleAsync(ctx.Guild);
        } catch (UnauthorizedException) {
            await channel.InformFailureAsync(this.Localization.GetString(ctx.Guild.Id, TranslationKey.cmd_err_role_403));
        }

        gcfg.MuteRoleId = muteRole?.Id ?? 0;
    }

    private async Task SetupLinkfilterAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
    {
        if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_lf, channel, false)) {
            gcfg.LinkfilterSettings.Enabled = true;
            gcfg.LinkfilterSettings.BlockDiscordInvites = await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_lf_invites, channel, false);
            gcfg.LinkfilterSettings.BlockBooterWebsites = await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_lf_ddos, channel, false);
            gcfg.LinkfilterSettings.BlockIpLoggingWebsites = await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_lf_ip, channel, false);
            gcfg.LinkfilterSettings.BlockDisturbingWebsites = await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_lf_gore, channel, false);
            gcfg.LinkfilterSettings.BlockUrlShorteners = await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_lf_urlshort, channel, false);
        }
    }

    private async Task SetupRatelimitAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
    {
        if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_rl, channel, false)) {
            gcfg.RatelimitEnabled = true;

            if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_rl_action(gcfg.RatelimitAction.Humanize()), channel, false)) {
                await channel.LocalizedEmbedAsync(this.Localization, TranslationKey.q_setup_new_action(Enum.GetNames<Punishment.Action>().JoinWith(", ")));
                Punishment.Action? action = await ctx.Client.GetInteractivity().WaitForPunishmentActionAsync(channel, ctx.User);
                if (action is not null)
                    gcfg.RatelimitAction = action.Value;
            }

            if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_rl_sens(gcfg.RatelimitSensitivity), channel, false)) {
                await channel.LocalizedEmbedAsync(this.Localization, TranslationKey.q_setup_new_sens(RatelimitSettings.MinSensitivity, RatelimitSettings.MaxSensitivity));
                InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                    m => short.TryParse(m.Content, out short sens) && sens >= RatelimitSettings.MinSensitivity && sens <= RatelimitSettings.MaxSensitivity
                );
                gcfg.RatelimitSensitivity = mctx.TimedOut ? throw new CommandFailedException(ctx, TranslationKey.str_timeout) : short.Parse(mctx.Result.Content);
            }
        }
    }

    private async Task SetupAntispamAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
    {
        if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_as, channel, false)) {
            gcfg.AntispamEnabled = true;

            if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_as_action(gcfg.AntispamAction.Humanize()), channel, false)) {
                await channel.LocalizedEmbedAsync(this.Localization, TranslationKey.q_setup_new_action(Enum.GetNames<Punishment.Action>().JoinWith(", ")));
                Punishment.Action? action = await ctx.Client.GetInteractivity().WaitForPunishmentActionAsync(channel, ctx.User);
                if (action is not null)
                    gcfg.AntispamAction = action.Value;
            }

            if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_as_sens(gcfg.AntispamSensitivity), channel, false)) {
                await channel.LocalizedEmbedAsync(this.Localization, TranslationKey.q_setup_new_sens(AntispamSettings.MinSensitivity, AntispamSettings.MaxSensitivity));
                InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                    m => short.TryParse(m.Content, out short sens) && sens >= AntispamSettings.MinSensitivity && sens <= AntispamSettings.MaxSensitivity
                );
                gcfg.AntispamSensitivity = mctx.TimedOut ? throw new CommandFailedException(ctx, TranslationKey.str_timeout) : short.Parse(mctx.Result.Content);
            }
        }
    }

    private async Task SetupAntiMentionAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
    {
        if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_am, channel, false)) {
            gcfg.AntispamEnabled = true;

            if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_am_action(gcfg.AntiMentionAction.Humanize()), channel, false)) {
                await channel.LocalizedEmbedAsync(this.Localization, TranslationKey.q_setup_new_action(Enum.GetNames<Punishment.Action>().JoinWith(", ")));
                Punishment.Action? action = await ctx.Client.GetInteractivity().WaitForPunishmentActionAsync(channel, ctx.User);
                if (action is not null)
                    gcfg.AntiMentionAction = action.Value;
            }

            if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_am_sens(gcfg.AntiMentionSensitivity), channel, false)) {
                await channel.LocalizedEmbedAsync(this.Localization, TranslationKey.q_setup_new_sens(AntispamSettings.MinSensitivity, AntispamSettings.MaxSensitivity));
                InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                    m => short.TryParse(m.Content, out short sens) && sens >= AntispamSettings.MinSensitivity && sens <= AntispamSettings.MaxSensitivity
                );
                gcfg.AntiMentionSensitivity = mctx.TimedOut ? throw new CommandFailedException(ctx, TranslationKey.str_timeout) : short.Parse(mctx.Result.Content);
            }
        }
    }

    private async Task SetupAntifloodAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
    {
        if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_af, channel, false)) {
            gcfg.AntifloodEnabled = true;

            if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_af_action(gcfg.AntifloodAction.Humanize()), channel, false)) {
                await channel.LocalizedEmbedAsync(this.Localization, TranslationKey.q_setup_new_action(Enum.GetNames<Punishment.Action>().JoinWith(", ")));
                Punishment.Action? action = await ctx.Client.GetInteractivity().WaitForPunishmentActionAsync(channel, ctx.User);
                if (action is not null)
                    gcfg.AntifloodAction = action.Value;
            }

            if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_af_sens(gcfg.AntifloodSensitivity), channel, false)) {
                await channel.LocalizedEmbedAsync(this.Localization, TranslationKey.q_setup_new_sens(AntifloodSettings.MinSensitivity, AntifloodSettings.MaxSensitivity));
                InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                    m => short.TryParse(m.Content, out short sens) && sens >= AntifloodSettings.MinSensitivity && sens <= AntifloodSettings.MaxSensitivity
                );
                gcfg.AntifloodSensitivity = mctx.TimedOut ? throw new CommandFailedException(ctx, TranslationKey.str_timeout) : short.Parse(mctx.Result.Content);
            }

            if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_af_cd(gcfg.AntifloodCooldown), channel, false)) {
                await channel.LocalizedEmbedAsync(this.Localization, TranslationKey.q_setup_new_cd(AntifloodSettings.MinCooldown, AntifloodSettings.MaxCooldown));
                InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                    m => short.TryParse(m.Content, out short cd) && cd >= AntifloodSettings.MinCooldown && cd <= AntifloodSettings.MaxCooldown
                );
                gcfg.AntifloodCooldown = mctx.TimedOut ? throw new CommandFailedException(ctx, TranslationKey.str_timeout) : short.Parse(mctx.Result.Content);
            }
        }
    }

    private async Task SetupAntiInstantLeaveAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
    {
        if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_il, channel, false)) {
            gcfg.AntiInstantLeaveEnabled = true;

            if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_il_cd(gcfg.AntifloodCooldown), channel, false)) {
                await channel.LocalizedEmbedAsync(this.Localization, TranslationKey.q_setup_new_cd(AntiInstantLeaveSettings.MinCooldown, AntiInstantLeaveSettings.MaxCooldown));
                InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User,
                    m => short.TryParse(m.Content, out short cd)
                         && cd >= AntiInstantLeaveSettings.MinCooldown && cd <= AntiInstantLeaveSettings.MaxCooldown
                );
                gcfg.AntiInstantLeaveCooldown = mctx.TimedOut ? throw new CommandFailedException(ctx, TranslationKey.str_timeout) : short.Parse(mctx.Result.Content);
            }
        }
    }

    private async Task SetupCurrencyAsync(GuildConfig gcfg, CommandContext ctx, DiscordChannel channel)
    {
        if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_setup_currency, channel, false)) {
            await channel.LocalizedEmbedAsync(this.Localization, TranslationKey.q_setup_currency_new(GuildConfig.CurrencyLimit));
            InteractivityResult<DiscordMessage> mctx = await channel.GetNextMessageAsync(ctx.User, m => m.Content.Length <= GuildConfig.CurrencyLimit);
            gcfg.Currency = mctx.TimedOut ? throw new CommandFailedException(ctx, TranslationKey.str_timeout) : mctx.Result.Content;
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
        await ls.LogAsync(ctx.Guild, gcfg.ToDiscordEmbed(ctx.Guild, this.Localization, true));
    }
    #endregion
}