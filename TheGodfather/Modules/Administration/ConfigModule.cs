using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Administration.Common;
using TimeZoneConverter;

namespace TheGodfather.Modules.Administration;

[Group("config")]
[Alias("configuration", "configure", "settings", "cfg")]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageGuild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class ConfigModule : TheGodfatherModule
{
    private readonly GuildConfigService guildConfig;


    public ConfigModule(LocalizationService localization, GuildConfigService guildConfig)
        : base(localization, new Color(0x3498DB))
    {
        this.guildConfig = guildConfig;
    }


    [Command("")]
    public async Task ExecuteGroupAsync()
    {
        GuildConfig cfg = await this.guildConfig.GetConfigAsync(this.Context.Guild.Id);
        CachedGuildConfig cached = cfg.CachedConfig;
        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithColor(this.ModuleColor);
            emb.WithTitle(this.Context.Guild.Name);
            emb.AddLocalizedField(TranslationKey.str_prefix, cached.Prefix, true);
            emb.AddLocalizedField(TranslationKey.str_locale, cached.Locale, true);
            emb.AddField("Timezone", cached.TimezoneId, true);
            emb.AddLocalizedField(TranslationKey.str_currency, cached.Currency, true);
            emb.AddLocalizedField(TranslationKey.str_replies_s, cached.ReactionResponse ? TranslationKey.str_on : TranslationKey.str_off, true);
            emb.AddLocalizedField(TranslationKey.str_suggestions, cached.SuggestionsEnabled ? TranslationKey.str_on : TranslationKey.str_off, true);
            emb.AddLocalizedField(TranslationKey.str_actionhistory, cfg.ActionHistoryEnabled ? TranslationKey.str_on : TranslationKey.str_off, true);
            emb.AddLocalizedField(TranslationKey.str_lvlup_s, cfg.SilentLevelUpEnabled ? TranslationKey.str_off : TranslationKey.str_on, true);
        });
    }

    [Command("levelup")]
    [Priority(1)]
    [Alias("lvlup", "lvl")]
    public async Task LevelUpAsync(bool enable)
    {
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.SilentLevelUpEnabled = !enable);
        await this.InfoAsync(this.ModuleColor, enable ? TranslationKey.str_cfg_lvlup_on : TranslationKey.str_cfg_lvlup_off);
    }

    [Command("levelup")]
    [Priority(0)]
    public async Task LevelUpAsync()
    {
        GuildConfig cfg = await this.guildConfig.GetConfigAsync(this.Context.Guild.Id);
        await this.InfoAsync(this.ModuleColor, cfg.SilentLevelUpEnabled ? TranslationKey.str_cfg_lvlup_get_on : TranslationKey.str_cfg_lvlup_get_off);
    }

    [Command("silent")]
    [Priority(1)]
    [Alias("reactionresponse", "silentresponse", "s", "rr")]
    public async Task SilentResponseAsync(bool enable)
    {
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.ReactionResponse = enable);
        await this.InfoAsync(this.ModuleColor, enable ? TranslationKey.str_cfg_silent_on : TranslationKey.str_cfg_silent_off);
    }

    [Command("silent")]
    [Priority(0)]
    public Task SilentResponseAsync()
    {
        CachedGuildConfig cfg = this.guildConfig.GetCachedConfig(this.Context.Guild.Id);
        return this.InfoAsync(this.ModuleColor, cfg.ReactionResponse ? TranslationKey.str_cfg_silent_get_on : TranslationKey.str_cfg_silent_get_off);
    }

    [Command("verbose")]
    [Priority(1)]
    [Alias("fullresponse", "verboseresponse", "v", "vr")]
    public Task VerboseResponseAsync(bool enable)
        => this.SilentResponseAsync(!enable);

    [Command("verbose")]
    [Priority(0)]
    public Task VerboseResponseAsync()
        => this.SilentResponseAsync();

    [Command("currency")]
    [Priority(1)]
    [Alias("setcurrency", "curr", "$", "$$", "$$$")]
    public async Task CurrencyAsync([Remainder] string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length > GuildConfig.CurrencyLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_currency(GuildConfig.CurrencyLimit));

        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.Currency = currency);
        await this.CurrencyAsync();
    }

    [Command("currency")]
    [Priority(0)]
    public Task CurrencyAsync()
    {
        CachedGuildConfig cfg = this.guildConfig.GetCachedConfig(this.Context.Guild.Id);
        return this.InfoAsync(this.ModuleColor, Emojis.MoneyBag, TranslationKey.str_currency_get(cfg.Currency));
    }

    [Command("suggestions")]
    [Priority(1)]
    [Alias("suggestion", "cmdsug", "sugg", "sug", "help")]
    public async Task SuggestionsAsync(bool enable)
    {
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.SuggestionsEnabled = enable);
        await this.InfoAsync(this.ModuleColor, enable ? TranslationKey.str_cfg_suggest_on : TranslationKey.str_cfg_suggest_off);
    }

    [Command("suggestions")]
    [Priority(0)]
    public Task SuggestionsAsync()
    {
        CachedGuildConfig cfg = this.guildConfig.GetCachedConfig(this.Context.Guild.Id);
        return this.InfoAsync(this.ModuleColor, cfg.SuggestionsEnabled ? TranslationKey.str_cfg_suggest_get_on : TranslationKey.str_cfg_suggest_get_off);
    }

    [Command("actionhistory")]
    [Priority(1)]
    [Alias("history", "ah")]
    public async Task ActionHistoryAsync(bool enable)
    {
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.ActionHistoryEnabled = enable);
        await this.InfoAsync(this.ModuleColor, enable ? TranslationKey.str_cfg_ah_on : TranslationKey.str_cfg_ah_off);
    }

    [Command("actionhistory")]
    [Priority(0)]
    public async Task ActionHistoryAsync()
    {
        GuildConfig cfg = await this.guildConfig.GetConfigAsync(this.Context.Guild.Id);
        await this.InfoAsync(this.ModuleColor, cfg.ActionHistoryEnabled ? TranslationKey.str_cfg_ah_get_on : TranslationKey.str_cfg_ah_get_off);
    }

    [Command("muterole")]
    [Alias("mr", "muterl", "mrl")]
    public async Task GetOrSetMuteRoleAsync(IRole? muteRole = null)
    {
        if (muteRole is null) {
            GuildConfig cfg = await this.guildConfig.GetConfigAsync(this.Context.Guild.Id);
            if (cfg.MuteRoleId == 0) {
                await this.InfoAsync(this.ModuleColor, TranslationKey.str_cfg_muterole_none);
                return;
            }

            muteRole = this.Context.Guild.GetRole(cfg.MuteRoleId);
            if (muteRole is null) {
                await this.FailAsync(TranslationKey.err_muterole_404);
                return;
            }
        } else {
            await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.MuteRoleId = muteRole.Id);
        }

        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_muterole(muteRole.Name));
    }

    [Command("locale")]
    [Alias("localization", "language", "lang")]
    public async Task LocaleAsync(string locale)
    {
        if (!await this.Localization.SetGuildLocaleAsync(this.Context.Guild.Id, locale))
            throw new LocalizedCommandException(TranslationKey.cmd_err_locale);

        await this.InfoAsync(this.ModuleColor);
    }

    [Command("timezone")]
    [Alias("tz")]
    public async Task TimezoneAsync([Remainder] string timezone)
    {
        if (!await this.Localization.SetGuildTimezoneIdAsync(this.Context.Guild.Id, timezone))
            throw new LocalizedCommandException(TranslationKey.cmd_err_tz);

        TimeZoneInfo tzInfo = this.Localization.GetGuildTimeZone(this.Context.Guild.Id);
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_tz_set(tzInfo.DisplayName, tzInfo.StandardName));
    }

    [Command("timezone current")]
    [Alias("tz current")]
    public Task TimezoneCurrentAsync()
    {
        TimeZoneInfo tzInfo = this.Localization.GetGuildTimeZone(this.Context.Guild.Id);
        return this.InfoAsync(this.ModuleColor, TranslationKey.fmt_tz_curr(tzInfo.DisplayName, tzInfo.StandardName));
    }

    [Command("timezone info")]
    [Alias("tz info")]
    public Task TimezoneInfoAsync([Remainder] string? timezone = null)
    {
        TimeZoneInfo tzInfo = string.IsNullOrWhiteSpace(timezone)
            ? this.Localization.GetGuildTimeZone(this.Context.Guild.Id)
            : TZConvert.GetTimeZoneInfo(timezone);
        DateTimeOffset time = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, tzInfo);
        return this.InfoAsync(this.ModuleColor, TranslationKey.fmt_tz(tzInfo.DisplayName, tzInfo.StandardName, time));
    }

    [Command("timezone list")]
    [Alias("tz list")]
    public Task TimezoneListAsync()
    {
        IEnumerable<string> timezones = TZConvert.KnownWindowsTimeZoneIds.Select(tz => $"(Win) {Bold(tz)}")
            .Concat(TZConvert.KnownIanaTimeZoneNames.Select(tz => $"(IANA) {Bold(tz)}"))
            .Concat(TZConvert.KnownRailsTimeZoneNames.Select(tz => $"(Rails) {Bold(tz)}"));
        return this.RespondPagedAsync(TranslationKey.str_tz_list, timezones, s => s, 8);
    }

    [Command("timezone reset")]
    [Alias("tz reset")]
    public async Task TimezoneResetAsync()
    {
        if (!await this.ConfirmAsync(TranslationKey.q_setup_reset))
            return;
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.TimezoneId = null);
        await this.InfoAsync(this.ModuleColor, TranslationKey.str_cfg_tz_reset);
    }

    [Command("localization")]
    [Alias("locale", "language", "lang", "region")]
    public Task LocalizationAsync()
        => this.InfoAsync(this.ModuleColor, TranslationKey.fmt_locale(this.Localization.GetGuildLocale(this.Context.Guild.Id)));

    [Command("localization set")]
    [Alias("locale set", "language set", "lang set")]
    public Task LocalizationSetAsync(string locale)
        => this.LocaleAsync(locale);

    [Command("localization list")]
    [Alias("locale list", "language list", "lang list")]
    public Task LocalizationListAsync()
        => this.RespondPagedAsync(TranslationKey.str_locales_all, this.Localization.AvailableLocales, s => s);

    [Command("logging")]
    [Alias("log", "modlog")]
    [Priority(1)]
    public async Task LoggingAsync(bool enable, ITextChannel? channel = null)
    {
        channel ??= (ITextChannel)this.Context.Channel;
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.LogChannelId = enable ? channel.Id : 0);
        await this.InfoAsync(this.ModuleColor, enable ? TranslationKey.evt_log_on(channel.Mention) : TranslationKey.evt_log_off);
    }

    [Command("logging")]
    [Priority(0)]
    public async Task LoggingAsync()
    {
        GuildConfig cfg = await this.guildConfig.GetConfigAsync(this.Context.Guild.Id);
        IReadOnlyList<ExemptedLoggingEntity> exempts = await this.Context.Services.GetRequiredService<LoggingService>().GetExemptsAsync(this.Context.Guild.Id);
        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_logging);
            emb.WithLocalizedDescription(cfg.LoggingEnabled ? TranslationKey.evt_log_on(MentionUtils.MentionChannel(cfg.LogChannelId)) : TranslationKey.evt_log_off);
            emb.WithColor(this.ModuleColor);
            if (exempts.Any())
                emb.AddLocalizedField(TranslationKey.str_exempts, FormatExempts(exempts));
        });
    }

    [Command("logging exempt channel")]
    [Alias("log exempt channel", "modlog exempt channel")]
    public Task LoggingExemptChannelsAsync(params ITextChannel[] channels)
        => this.ExemptAsync<LoggingService, ExemptedLoggingEntity>(channels.Select(c => c.Id), ExemptedEntityType.Channel);

    [Command("logging exempt member")]
    [Alias("log exempt member", "modlog exempt member")]
    public Task LoggingExemptMembersAsync(params IGuildUser[] members)
        => this.ExemptAsync<LoggingService, ExemptedLoggingEntity>(members.Select(m => m.Id), ExemptedEntityType.Member);

    [Command("logging exempt role")]
    [Alias("log exempt role", "modlog exempt role")]
    public Task LoggingExemptRolesAsync(params IRole[] roles)
        => this.ExemptAsync<LoggingService, ExemptedLoggingEntity>(roles.Select(r => r.Id), ExemptedEntityType.Role);

    [Command("logging unexempt channel")]
    [Alias("log unexempt channel", "modlog unexempt channel")]
    public Task LoggingUnexemptChannelsAsync(params ITextChannel[] channels)
        => this.UnexemptAsync<LoggingService, ExemptedLoggingEntity>(channels.Select(c => c.Id), ExemptedEntityType.Channel);

    [Command("logging unexempt member")]
    [Alias("log unexempt member", "modlog unexempt member")]
    public Task LoggingUnexemptMembersAsync(params IGuildUser[] members)
        => this.UnexemptAsync<LoggingService, ExemptedLoggingEntity>(members.Select(m => m.Id), ExemptedEntityType.Member);

    [Command("logging unexempt role")]
    [Alias("log unexempt role", "modlog unexempt role")]
    public Task LoggingUnexemptRolesAsync(params IRole[] roles)
        => this.UnexemptAsync<LoggingService, ExemptedLoggingEntity>(roles.Select(r => r.Id), ExemptedEntityType.Role);

    [Command("backup")]
    [Alias("bk", "bak")]
    [Priority(1)]
    public async Task BackupAsync(bool enable)
    {
        BackupService backups = this.Context.Services.GetRequiredService<BackupService>();
        if (enable)
            await backups.EnableAsync(this.Context.Guild.Id);
        else
            await backups.DisableAsync(this.Context.Guild.Id);
        await this.InfoAsync(this.ModuleColor, enable ? TranslationKey.evt_bak_enable : TranslationKey.evt_bak_disable);
    }

    [Command("backup")]
    [Priority(0)]
    public async Task BackupAsync()
    {
        BackupService backups = this.Context.Services.GetRequiredService<BackupService>();
        IReadOnlyList<ExemptedBackupEntity> exempts = await backups.GetExemptsAsync(this.Context.Guild.Id);
        GuildConfig cfg = await this.guildConfig.GetConfigAsync(this.Context.Guild.Id);
        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_backup);
            emb.WithLocalizedDescription(cfg.BackupEnabled ? TranslationKey.str_enabled : TranslationKey.str_disabled);
            emb.WithColor(this.ModuleColor);
            if (exempts.Any())
                emb.AddLocalizedField(TranslationKey.str_exempts, exempts.Select(e => MentionUtils.MentionChannel(e.ChannelId)).JoinWith());
        });
    }

    [Command("backup download")]
    [Alias("backup dl", "backup get", "backup zip")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task BackupDownloadAsync()
    {
        if (!await this.Context.Services.GetRequiredService<BackupService>().WithBackupZipAsync(
                this.Context.Guild.Id,
                stream => this.Context.Channel.SendFileAsync(stream, "backup.zip")))
            throw new LocalizedCommandException(TranslationKey.cmd_err_backup);
    }

    [Command("backup exempt")]
    public async Task BackupExemptAsync(params ITextChannel[] channels)
    {
        await this.Context.Services.GetRequiredService<BackupService>().ExemptAsync(this.Context.Guild.Id, channels.Select(c => c.Id));
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("backup unexempt")]
    public async Task BackupUnexemptAsync(params ITextChannel[] channels)
    {
        await this.Context.Services.GetRequiredService<BackupService>().UnexemptAsync(this.Context.Guild.Id, channels.Select(c => c.Id));
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("welcome")]
    [Alias("enter", "join", "wlc", "wm", "w")]
    [Priority(1)]
    public async Task WelcomeAsync(bool enable, ITextChannel? channel = null)
    {
        if (enable && channel is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chn_none);
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.WelcomeChannelId = enable ? channel!.Id : 0);
        await this.InfoAsync(this.ModuleColor, enable ? TranslationKey.fmt_memupd_wc(channel!.Mention) : TranslationKey.str_cfg_welcome_off);
    }

    [Command("welcome")]
    [Priority(0)]
    public async Task WelcomeAsync()
    {
        GuildConfig cfg = await this.guildConfig.GetConfigAsync(this.Context.Guild.Id);
        await this.InfoAsync(this.ModuleColor, cfg.WelcomeChannelId == 0
            ? TranslationKey.str_cfg_welcome_get_off
            : TranslationKey.str_cfg_welcome_get_on(MentionUtils.MentionChannel(cfg.WelcomeChannelId)));
    }

    [Command("welcome channel")]
    public Task WelcomeChannelAsync(ITextChannel? channel = null)
        => this.WelcomeAsync(true, channel ?? (ITextChannel)this.Context.Channel);

    [Command("welcome message")]
    public async Task WelcomeMessageAsync([Remainder] string message)
    {
        ValidateMemberUpdateMessage(message);
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.WelcomeMessage = message);
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_memupd_wm(Format.Code(Format.Sanitize(message))));
    }

    [Command("leave")]
    [Alias("quit", "lv", "lm", "l")]
    [Priority(1)]
    public async Task LeaveAsync(bool enable, ITextChannel? channel = null)
    {
        if (enable && channel is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chn_none);
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.LeaveChannelId = enable ? channel!.Id : 0);
        await this.InfoAsync(this.ModuleColor, enable ? TranslationKey.fmt_memupd_lc(channel!.Mention) : TranslationKey.str_cfg_leave_off);
    }

    [Command("leave")]
    [Priority(0)]
    public async Task LeaveAsync()
    {
        GuildConfig cfg = await this.guildConfig.GetConfigAsync(this.Context.Guild.Id);
        await this.InfoAsync(this.ModuleColor, cfg.LeaveChannelId == 0
            ? TranslationKey.str_cfg_leave_get_off
            : TranslationKey.str_cfg_leave_get_on(MentionUtils.MentionChannel(cfg.LeaveChannelId)));
    }

    [Command("leave channel")]
    public Task LeaveChannelAsync(ITextChannel? channel = null)
        => this.LeaveAsync(true, channel ?? (ITextChannel)this.Context.Channel);

    [Command("leave message")]
    public async Task LeaveMessageAsync([Remainder] string message)
    {
        ValidateMemberUpdateMessage(message);
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.LeaveMessage = message);
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_memupd_lm(Format.Code(Format.Sanitize(message))));
    }

    [Command("linkfilter")]
    [Alias("lf")]
    [Priority(1)]
    public async Task LinkfilterAsync(bool enable)
    {
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.LinkfilterEnabled = enable);
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("linkfilter")]
    [Priority(0)]
    public Task LinkfilterAsync()
    {
        LinkfilterSettings settings = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).LinkfilterSettings;
        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_lf);
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_lf_invite, settings.BlockDiscordInvites ? TranslationKey.str_on : TranslationKey.str_off, true);
            emb.AddLocalizedField(TranslationKey.str_lf_ddos, settings.BlockBooterWebsites ? TranslationKey.str_on : TranslationKey.str_off, true);
            emb.AddLocalizedField(TranslationKey.str_lf_gore, settings.BlockDisturbingWebsites ? TranslationKey.str_on : TranslationKey.str_off, true);
            emb.AddLocalizedField(TranslationKey.str_lf_ip, settings.BlockIpLoggingWebsites ? TranslationKey.str_on : TranslationKey.str_off, true);
            emb.AddLocalizedField(TranslationKey.str_lf_urlshort, settings.BlockUrlShorteners ? TranslationKey.str_on : TranslationKey.str_off, true);
        });
    }

    [Command("linkfilter invites")]
    [Alias("lf invites")]
    public Task LinkfilterInvitesAsync(bool enable)
        => this.ModifyLinkfilterAsync(cfg => cfg.LinkfilterDiscordInvitesEnabled = enable);

    [Command("linkfilter booters")]
    [Alias("lf booters", "lf ddos")]
    public Task LinkfilterBootersAsync(bool enable)
        => this.ModifyLinkfilterAsync(cfg => cfg.LinkfilterBootersEnabled = enable);

    [Command("linkfilter shocksites")]
    [Alias("lf shocksites", "lf disturbing", "lf gore")]
    public Task LinkfilterDisturbingAsync(bool enable)
        => this.ModifyLinkfilterAsync(cfg => cfg.LinkfilterDisturbingWebsitesEnabled = enable);

    [Command("linkfilter iploggers")]
    [Alias("lf iploggers", "lf ip")]
    public Task LinkfilterIpLoggersAsync(bool enable)
        => this.ModifyLinkfilterAsync(cfg => cfg.LinkfilterIpLoggersEnabled = enable);

    [Command("linkfilter shorteners")]
    [Alias("lf shorteners", "lf urlshort")]
    public Task LinkfilterShortenersAsync(bool enable)
        => this.ModifyLinkfilterAsync(cfg => cfg.LinkfilterUrlShortenersEnabled = enable);

    [Command("antiflood")]
    [Alias("antiraid", "ar", "af")]
    [Priority(1)]
    public async Task AntifloodAsync(bool enable, short sensitivity = 5, Punishment.Action action = Punishment.Action.Kick, short cooldownSeconds = 10)
    {
        ValidateRange(sensitivity, AntifloodSettings.MinSensitivity, AntifloodSettings.MaxSensitivity, TranslationKey.cmd_err_range_sens);
        ValidateRange(cooldownSeconds, AntifloodSettings.MinCooldown, AntifloodSettings.MaxCooldown, TranslationKey.cmd_err_range_cd);
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.AntifloodSettings = new AntifloodSettings {
            Enabled = enable,
            Sensitivity = sensitivity,
            Action = action,
            Cooldown = cooldownSeconds
        });
        await this.InfoAsync(this.ModuleColor, enable ? TranslationKey.evt_af_enable : TranslationKey.evt_af_disable);
    }

    [Command("antiflood")]
    [Priority(0)]
    public Task AntifloodAsync()
        => this.ShowConfigSettingAsync(TranslationKey.str_antiflood, this.guildConfig.GetCachedConfig(this.Context.Guild.Id).AntifloodSettings.ToEmbedFieldString());

    [Command("antiflood action")]
    public async Task AntifloodActionAsync(Punishment.Action action)
    {
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.AntifloodAction = action);
        await this.InfoAsync(this.ModuleColor, TranslationKey.evt_af_action(action.Humanize()));
    }

    [Command("antiflood sensitivity")]
    public async Task AntifloodSensitivityAsync(short sensitivity)
    {
        ValidateRange(sensitivity, AntifloodSettings.MinSensitivity, AntifloodSettings.MaxSensitivity, TranslationKey.cmd_err_range_sens);
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.AntifloodSensitivity = sensitivity);
        await this.InfoAsync(this.ModuleColor, TranslationKey.evt_af_sens(sensitivity));
    }

    [Command("antiflood cooldown")]
    public async Task AntifloodCooldownAsync(short cooldownSeconds)
    {
        ValidateRange(cooldownSeconds, AntifloodSettings.MinCooldown, AntifloodSettings.MaxCooldown, TranslationKey.cmd_err_range_cd);
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.AntifloodCooldown = cooldownSeconds);
        await this.InfoAsync(this.ModuleColor, TranslationKey.evt_af_cd(cooldownSeconds));
    }

    [Command("instantleave")]
    [Alias("il")]
    [Priority(1)]
    public async Task InstantLeaveAsync(bool enable, short cooldownSeconds = 5)
    {
        ValidateRange(cooldownSeconds, AntiInstantLeaveSettings.MinCooldown, AntiInstantLeaveSettings.MaxCooldown, TranslationKey.cmd_err_range_cd);
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.AntiInstantLeaveSettings = new AntiInstantLeaveSettings {
            Enabled = enable,
            Cooldown = cooldownSeconds
        });
        await this.InfoAsync(this.ModuleColor, enable ? TranslationKey.evt_il_enable : TranslationKey.evt_il_disable);
    }

    [Command("instantleave")]
    [Priority(0)]
    public Task InstantLeaveAsync()
        => this.ShowConfigSettingAsync(TranslationKey.str_instantleave, this.guildConfig.GetCachedConfig(this.Context.Guild.Id).AntiInstantLeaveSettings.ToEmbedFieldString());

    [Command("instantleave cooldown")]
    public async Task InstantLeaveCooldownAsync(short cooldownSeconds)
    {
        ValidateRange(cooldownSeconds, AntiInstantLeaveSettings.MinCooldown, AntiInstantLeaveSettings.MaxCooldown, TranslationKey.cmd_err_range_cd);
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.AntiInstantLeaveCooldown = cooldownSeconds);
        await this.InfoAsync(this.ModuleColor, TranslationKey.evt_il_cd(cooldownSeconds));
    }

    [Command("antispam")]
    [Alias("as")]
    [Priority(1)]
    public Task AntispamAsync(bool enable, short sensitivity = 5, Punishment.Action action = Punishment.Action.TemporaryMute)
        => this.ConfigureExemptableAsync<AntispamSettings>(enable, sensitivity, action, AntispamSettings.MinSensitivity, AntispamSettings.MaxSensitivity, cfg => cfg.AntispamSettings, (cfg, settings) => cfg.AntispamSettings = settings, TranslationKey.evt_as_enable, TranslationKey.evt_as_disable);

    [Command("antispam")]
    [Priority(0)]
    public Task AntispamAsync()
        => this.ShowExemptableAsync<AntispamService, ExemptedSpamEntity>(TranslationKey.str_antispam, this.guildConfig.GetCachedConfig(this.Context.Guild.Id).AntispamSettings.ToEmbedFieldString());

    [Command("antispam action")]
    public Task AntispamActionAsync(Punishment.Action action)
        => this.SetActionAsync(cfg => cfg.AntispamAction = action, TranslationKey.evt_as_action(action.Humanize()));

    [Command("antispam sensitivity")]
    public Task AntispamSensitivityAsync(short sensitivity)
        => this.SetSensitivityAsync(sensitivity, AntispamSettings.MinSensitivity, AntispamSettings.MaxSensitivity, cfg => cfg.AntispamSensitivity = sensitivity, TranslationKey.evt_as_sens(sensitivity));

    [Command("antimention")]
    [Alias("am")]
    [Priority(1)]
    public Task AntiMentionAsync(bool enable, short sensitivity = 5, Punishment.Action action = Punishment.Action.TemporaryMute)
        => this.ConfigureExemptableAsync<AntiMentionSettings>(enable, sensitivity, action, AntiMentionSettings.MinSensitivity, AntiMentionSettings.MaxSensitivity, cfg => cfg.AntiMentionSettings, (cfg, settings) => cfg.AntiMentionSettings = settings, TranslationKey.evt_am_enable, TranslationKey.evt_am_disable);

    [Command("antimention")]
    [Priority(0)]
    public Task AntiMentionAsync()
        => this.ShowExemptableAsync<AntiMentionService, ExemptedMentionEntity>(TranslationKey.str_antimention, this.guildConfig.GetCachedConfig(this.Context.Guild.Id).AntiMentionSettings.ToEmbedFieldString());

    [Command("antimention action")]
    public Task AntiMentionActionAsync(Punishment.Action action)
        => this.SetActionAsync(cfg => cfg.AntiMentionAction = action, TranslationKey.evt_am_action(action.Humanize()));

    [Command("antimention sensitivity")]
    public Task AntiMentionSensitivityAsync(short sensitivity)
        => this.SetSensitivityAsync(sensitivity, AntiMentionSettings.MinSensitivity, AntiMentionSettings.MaxSensitivity, cfg => cfg.AntiMentionSensitivity = sensitivity, TranslationKey.evt_am_sens(sensitivity));

    [Command("ratelimit")]
    [Alias("rl")]
    [Priority(1)]
    public Task RatelimitAsync(bool enable, short sensitivity = 5, Punishment.Action action = Punishment.Action.TemporaryMute)
        => this.ConfigureExemptableAsync<RatelimitSettings>(enable, sensitivity, action, RatelimitSettings.MinSensitivity, RatelimitSettings.MaxSensitivity, cfg => cfg.RatelimitSettings, (cfg, settings) => cfg.RatelimitSettings = settings, TranslationKey.evt_rl_enable, TranslationKey.evt_rl_disable);

    [Command("ratelimit")]
    [Priority(0)]
    public Task RatelimitAsync()
        => this.ShowExemptableAsync<RatelimitService, ExemptedRatelimitEntity>(TranslationKey.str_ratelimit, this.guildConfig.GetCachedConfig(this.Context.Guild.Id).RatelimitSettings.ToEmbedFieldString());

    [Command("ratelimit action")]
    public Task RatelimitActionAsync(Punishment.Action action)
        => this.SetActionAsync(cfg => cfg.RatelimitAction = action, TranslationKey.evt_rl_action(action.Humanize()));

    [Command("ratelimit sensitivity")]
    public Task RatelimitSensitivityAsync(short sensitivity)
        => this.SetSensitivityAsync(sensitivity, RatelimitSettings.MinSensitivity, RatelimitSettings.MaxSensitivity, cfg => cfg.RatelimitSensitivity = sensitivity, TranslationKey.evt_rl_sens(sensitivity));

    [Command("antispam exempt channel")]
    public Task AntispamExemptChannelsAsync(params ITextChannel[] channels)
        => this.ExemptAsync<AntispamService, ExemptedSpamEntity>(channels.Select(c => c.Id), ExemptedEntityType.Channel);

    [Command("antispam exempt member")]
    public Task AntispamExemptMembersAsync(params IGuildUser[] members)
        => this.ExemptAsync<AntispamService, ExemptedSpamEntity>(members.Select(m => m.Id), ExemptedEntityType.Member);

    [Command("antispam exempt role")]
    public Task AntispamExemptRolesAsync(params IRole[] roles)
        => this.ExemptAsync<AntispamService, ExemptedSpamEntity>(roles.Select(r => r.Id), ExemptedEntityType.Role);

    [Command("antispam unexempt channel")]
    public Task AntispamUnexemptChannelsAsync(params ITextChannel[] channels)
        => this.UnexemptAsync<AntispamService, ExemptedSpamEntity>(channels.Select(c => c.Id), ExemptedEntityType.Channel);

    [Command("antispam unexempt member")]
    public Task AntispamUnexemptMembersAsync(params IGuildUser[] members)
        => this.UnexemptAsync<AntispamService, ExemptedSpamEntity>(members.Select(m => m.Id), ExemptedEntityType.Member);

    [Command("antispam unexempt role")]
    public Task AntispamUnexemptRolesAsync(params IRole[] roles)
        => this.UnexemptAsync<AntispamService, ExemptedSpamEntity>(roles.Select(r => r.Id), ExemptedEntityType.Role);

    [Command("antimention exempt channel")]
    public Task AntiMentionExemptChannelsAsync(params ITextChannel[] channels)
        => this.ExemptAsync<AntiMentionService, ExemptedMentionEntity>(channels.Select(c => c.Id), ExemptedEntityType.Channel);

    [Command("antimention exempt member")]
    public Task AntiMentionExemptMembersAsync(params IGuildUser[] members)
        => this.ExemptAsync<AntiMentionService, ExemptedMentionEntity>(members.Select(m => m.Id), ExemptedEntityType.Member);

    [Command("antimention exempt role")]
    public Task AntiMentionExemptRolesAsync(params IRole[] roles)
        => this.ExemptAsync<AntiMentionService, ExemptedMentionEntity>(roles.Select(r => r.Id), ExemptedEntityType.Role);

    [Command("antimention unexempt channel")]
    public Task AntiMentionUnexemptChannelsAsync(params ITextChannel[] channels)
        => this.UnexemptAsync<AntiMentionService, ExemptedMentionEntity>(channels.Select(c => c.Id), ExemptedEntityType.Channel);

    [Command("antimention unexempt member")]
    public Task AntiMentionUnexemptMembersAsync(params IGuildUser[] members)
        => this.UnexemptAsync<AntiMentionService, ExemptedMentionEntity>(members.Select(m => m.Id), ExemptedEntityType.Member);

    [Command("antimention unexempt role")]
    public Task AntiMentionUnexemptRolesAsync(params IRole[] roles)
        => this.UnexemptAsync<AntiMentionService, ExemptedMentionEntity>(roles.Select(r => r.Id), ExemptedEntityType.Role);

    [Command("ratelimit exempt channel")]
    public Task RatelimitExemptChannelsAsync(params ITextChannel[] channels)
        => this.ExemptAsync<RatelimitService, ExemptedRatelimitEntity>(channels.Select(c => c.Id), ExemptedEntityType.Channel);

    [Command("ratelimit exempt member")]
    public Task RatelimitExemptMembersAsync(params IGuildUser[] members)
        => this.ExemptAsync<RatelimitService, ExemptedRatelimitEntity>(members.Select(m => m.Id), ExemptedEntityType.Member);

    [Command("ratelimit exempt role")]
    public Task RatelimitExemptRolesAsync(params IRole[] roles)
        => this.ExemptAsync<RatelimitService, ExemptedRatelimitEntity>(roles.Select(r => r.Id), ExemptedEntityType.Role);

    [Command("ratelimit unexempt channel")]
    public Task RatelimitUnexemptChannelsAsync(params ITextChannel[] channels)
        => this.UnexemptAsync<RatelimitService, ExemptedRatelimitEntity>(channels.Select(c => c.Id), ExemptedEntityType.Channel);

    [Command("ratelimit unexempt member")]
    public Task RatelimitUnexemptMembersAsync(params IGuildUser[] members)
        => this.UnexemptAsync<RatelimitService, ExemptedRatelimitEntity>(members.Select(m => m.Id), ExemptedEntityType.Member);

    [Command("ratelimit unexempt role")]
    public Task RatelimitUnexemptRolesAsync(params IRole[] roles)
        => this.UnexemptAsync<RatelimitService, ExemptedRatelimitEntity>(roles.Select(r => r.Id), ExemptedEntityType.Role);

    private Task ModifyLinkfilterAsync(Action<GuildConfig> modify)
    {
        return this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, modify)
            .ContinueWith(_ => this.InfoAsync(this.ModuleColor)).Unwrap();
    }

    private async Task ConfigureExemptableAsync<TSettings>(
        bool enable,
        short sensitivity,
        Punishment.Action action,
        int minSensitivity,
        int maxSensitivity,
        Func<GuildConfig, TSettings> _,
        Action<GuildConfig, TSettings> set,
        TranslationKey enabledKey,
        TranslationKey disabledKey)
        where TSettings : ISettings, new()
    {
        ValidateRange(sensitivity, minSensitivity, maxSensitivity, TranslationKey.cmd_err_range_sens);
        var settings = new TSettings();
        switch (settings) {
            case AntispamSettings antispam:
                antispam.Enabled = enable;
                antispam.Sensitivity = sensitivity;
                antispam.Action = action;
                break;
            case AntiMentionSettings antimention:
                antimention.Enabled = enable;
                antimention.Sensitivity = sensitivity;
                antimention.Action = action;
                break;
            case RatelimitSettings ratelimit:
                ratelimit.Enabled = enable;
                ratelimit.Sensitivity = sensitivity;
                ratelimit.Action = action;
                break;
        }

        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => set(cfg, settings));
        await this.InfoAsync(this.ModuleColor, enable ? enabledKey : disabledKey);
    }

    private async Task SetActionAsync(Action<GuildConfig> setAction, TranslationKey key)
    {
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, setAction);
        await this.InfoAsync(this.ModuleColor, key);
    }

    private async Task SetSensitivityAsync(short sensitivity, int min, int max, Action<GuildConfig> setSensitivity, TranslationKey key)
    {
        ValidateRange(sensitivity, min, max, TranslationKey.cmd_err_range_sens);
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, setSensitivity);
        await this.InfoAsync(this.ModuleColor, key);
    }

    private async Task ExemptAsync<TService, TExempt>(IEnumerable<ulong> ids, ExemptedEntityType type)
        where TService : ExemptableProtectionServiceBase<TExempt>
        where TExempt : ExemptedEntity, new()
    {
        ulong[] idArray = ids.ToArray();
        if (!idArray.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_exempt);

        await this.Context.Services.GetRequiredService<TService>().ExemptAsync(this.Context.Guild.Id, type, idArray);
        await this.InfoAsync(this.ModuleColor);
    }

    private async Task UnexemptAsync<TService, TExempt>(IEnumerable<ulong> ids, ExemptedEntityType type)
        where TService : ExemptableProtectionServiceBase<TExempt>
        where TExempt : ExemptedEntity, new()
    {
        ulong[] idArray = ids.ToArray();
        if (!idArray.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_exempt);

        await this.Context.Services.GetRequiredService<TService>().UnexemptAsync(this.Context.Guild.Id, type, idArray);
        await this.InfoAsync(this.ModuleColor);
    }

    private async Task ShowExemptableAsync<TService, TExempt>(TranslationKey title, TranslationKey description)
        where TService : ExemptableProtectionServiceBase<TExempt>
        where TExempt : ExemptedEntity, new()
    {
        IReadOnlyList<TExempt> exempts = await this.Context.Services.GetRequiredService<TService>().GetExemptsAsync(this.Context.Guild.Id);
        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(title);
            emb.WithLocalizedDescription(description);
            emb.WithColor(this.ModuleColor);
            if (exempts.Any())
                emb.AddLocalizedField(TranslationKey.str_exempts, FormatExempts(exempts));
        });
    }

    private Task ShowConfigSettingAsync(TranslationKey title, TranslationKey description)
        => this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(title);
            emb.WithLocalizedDescription(description);
            emb.WithColor(this.ModuleColor);
        });

    private static string FormatExempts(IEnumerable<ExemptedEntity> exempts)
        => exempts.Select(ex => ex.Type switch {
            ExemptedEntityType.Channel => $"{InlineCode("c")} {MentionUtils.MentionChannel(ex.Id)}",
            ExemptedEntityType.Member => $"{InlineCode("m")} {MentionUtils.MentionUser(ex.Id)}",
            ExemptedEntityType.Role => $"{InlineCode("r")} {MentionUtils.MentionRole(ex.Id)}",
            _ => ex.Id.ToString()
        }).JoinWith();

    private static void ValidateRange(short value, int min, int max, Func<object?, object?, TranslationKey> keyFactory)
    {
        if (value < min || value > max)
            throw new LocalizedCommandException(keyFactory(min, max));
    }

    private static void ValidateMemberUpdateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message) || message.Length is < 3 or > GuildConfig.MemberUpdateMessageLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_memupd_msg(GuildConfig.MemberUpdateMessageLimit));
    }
}
