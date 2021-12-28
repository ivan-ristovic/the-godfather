using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Services.Common;

namespace TheGodfather.Database.Models;

[Table("guild_cfg")]
public class GuildConfig
{
    public const int LocaleLimit = 8;
    public const int PrefixLimit = 8;
    public const int TimezoneIdLimit = 32;
    public const int CurrencyLimit = 32;
    public const int MemberUpdateMessageLimit = 128;

    public const int MinTempMuteCooldown = 60;
    public const int MaxTempMuteCooldown = 604800;
    public const int MinTempBanCooldown = 60;
    public const int MaxTempBanCooldown = 2592000;

    public virtual ICollection<ActionHistoryEntry> ActionHistory { get; set; }
    public virtual ICollection<AutoRole> AutoRoles { get; set; }
    public virtual ICollection<BankAccount> Accounts { get; set; }
    public virtual ICollection<Birthday> Birthdays { get; set; }
    public virtual ICollection<Chicken> Chickens { get; set; }
    public virtual ICollection<ChickenBoughtUpgrade> ChickenBoughtUpgrades { get; set; }
    public virtual ICollection<CommandRule> CommandRules { get; set; }
    public virtual ICollection<EmojiReaction> EmojiReactions { get; set; }
    public virtual ICollection<ExemptedSpamEntity> ExemptsSpam { get; set; }
    public virtual ICollection<ExemptedMentionEntity> ExemptsMention { get; set; }
    public virtual ICollection<ExemptedLoggingEntity> ExemptsLogging { get; set; }
    public virtual ICollection<ExemptedRatelimitEntity> ExemptsRatelimit { get; set; }
    public virtual ICollection<Filter> Filters { get; set; }
    public virtual ICollection<ForbiddenName> ForbiddenNames { get; set; }
    public virtual ICollection<GuildTask> GuildTasks { get; set; }
    public virtual ICollection<Meme> Memes { get; set; }
    public virtual ICollection<PurchasableItem> PurchasableItems { get; set; }
    public virtual ICollection<XpRank> Ranks { get; set; }
    public virtual ICollection<SelfRole> SelfRoles { get; set; }
    public virtual ICollection<RssSubscription> Subscriptions { get; set; }
    public virtual ICollection<TextReaction> TextReactions { get; set; }


    public GuildConfig()
    {
        this.Accounts = new HashSet<BankAccount>();
        this.ActionHistory = new HashSet<ActionHistoryEntry>();
        this.AutoRoles = new HashSet<AutoRole>();
        this.Birthdays = new HashSet<Birthday>();
        this.Chickens = new HashSet<Chicken>();
        this.ChickenBoughtUpgrades = new HashSet<ChickenBoughtUpgrade>();
        this.CommandRules = new HashSet<CommandRule>();
        this.EmojiReactions = new HashSet<EmojiReaction>();
        this.ExemptsSpam = new HashSet<ExemptedSpamEntity>();
        this.ExemptsMention = new HashSet<ExemptedMentionEntity>();
        this.ExemptsLogging = new HashSet<ExemptedLoggingEntity>();
        this.ExemptsRatelimit = new HashSet<ExemptedRatelimitEntity>();
        this.Filters = new HashSet<Filter>();
        this.GuildTasks = new HashSet<GuildTask>();
        this.ForbiddenNames = new HashSet<ForbiddenName>();
        this.Memes = new HashSet<Meme>();
        this.PurchasableItems = new HashSet<PurchasableItem>();
        this.Ranks = new HashSet<XpRank>();
        this.SelfRoles = new HashSet<SelfRole>();
        this.Subscriptions = new HashSet<RssSubscription>();
        this.TextReactions = new HashSet<TextReaction>();
    }


    [Key]
    [Column("gid")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long GuildIdDb { get; set; }
    [NotMapped]
    public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

    [Column("prefix")][MaxLength(PrefixLimit)]
    public string? Prefix { get; set; }

    [Column("locale")][MaxLength(LocaleLimit)]
    public string? Locale { get; set; }

    [Column("backup")]
    public bool BackupEnabled { get; set; }

    [Column("timezone_id")][MaxLength(TimezoneIdLimit)]
    public string? TimezoneId { get; set; }

    [Column("currency")][MaxLength(CurrencyLimit)]
    public string? Currency { get; set; }

    [Column("suggestions_enabled")]
    public bool SuggestionsEnabled { get; set; }

    [Column("log_cid")]
    public long LogChannelIdDb { get; set; }
    [NotMapped]
    public ulong LogChannelId { get => (ulong)this.LogChannelIdDb; set => this.LogChannelIdDb = (long)value; }
    [NotMapped]
    public bool LoggingEnabled => this.LogChannelId != default;

    [Column("mute_rid")]
    public long MuteRoleIdDb { get; set; }
    [NotMapped]
    public ulong MuteRoleId { get => (ulong)this.MuteRoleIdDb; set => this.MuteRoleIdDb = (long)value; }

    [Column("temp_mute_cooldown", TypeName = "interval")]
    public TimeSpan? TempMuteCooldownDb { get; set; }
    [NotMapped]
    public TimeSpan TempMuteCooldown => this.TempMuteCooldownDb ?? TimeSpan.FromHours(8);

    [Column("temp_ban_cooldown", TypeName = "interval")]
    public TimeSpan? TempBanCooldownDb { get; set; }
    [NotMapped]
    public TimeSpan TempBanCooldown => this.TempBanCooldownDb ?? TimeSpan.FromDays(1);

    [Column("silent_response_enabled")]
    public bool ReactionResponse { get; set; }

    [Column("silent_levelup_enabled")]
    public bool SilentLevelUpEnabled { get; set; }

    [Column("action_history_enabled")]
    public bool ActionHistoryEnabled { get; set; }


    #region Starboard
    [Column("starboard_cid")]
    public long StarboardChannelIdDb { get; set; }
    [NotMapped]
    public ulong StarboardChannelId { get => (ulong)this.StarboardChannelIdDb; set => this.StarboardChannelIdDb = (long)value; }
        
    [Column("starboard_emoji")][MaxLength(DiscordLimits.EmojiNameLimit)]
    public string? StarboardEmoji { get; set; }
    [NotMapped]
    public bool StarboardEnabled => this.StarboardChannelId != default;

    [Column("starboard_sens")]
    public int StarboardSensitivity { get; set; } = 5;
    #endregion

    #region Welcome/Leave Settings
    [Column("welcome_cid")]
    public long WelcomeChannelIdDb { get; set; }
    [NotMapped]
    public ulong WelcomeChannelId { get => (ulong)this.WelcomeChannelIdDb; set => this.WelcomeChannelIdDb = (long)value; }

    [Column("leave_cid")]
    public long LeaveChannelIdDb { get; set; }
    [NotMapped]
    public ulong LeaveChannelId { get => (ulong)this.LeaveChannelIdDb; set => this.LeaveChannelIdDb = (long)value; }

    [Column("welcome_msg")][MaxLength(MemberUpdateMessageLimit)]
    public string? WelcomeMessage { get; set; }

    [Column("leave_msg")][MaxLength(MemberUpdateMessageLimit)]
    public string? LeaveMessage { get; set; }
    #endregion

    #region Linkfilter Settings
    [NotMapped]
    public LinkfilterSettings LinkfilterSettings { get; set; } = new();

    [Column("linkfilter_enabled")]
    public bool LinkfilterEnabled {
        get => this.LinkfilterSettings.Enabled;
        set => this.LinkfilterSettings.Enabled = value;
    }

    [Column("linkfilter_booters")]
    public bool LinkfilterBootersEnabled {
        get => this.LinkfilterSettings.BlockBooterWebsites;
        set => this.LinkfilterSettings.BlockBooterWebsites = value;
    }

    [Column("linkfilter_disturbing")]
    public bool LinkfilterDisturbingWebsitesEnabled {
        get => this.LinkfilterSettings.BlockDisturbingWebsites;
        set => this.LinkfilterSettings.BlockDisturbingWebsites = value;
    }

    [Column("linkfilter_invites")]
    public bool LinkfilterDiscordInvitesEnabled {
        get => this.LinkfilterSettings.BlockDiscordInvites;
        set => this.LinkfilterSettings.BlockDiscordInvites = value;
    }

    [Column("linkfilter_loggers")]
    public bool LinkfilterIpLoggersEnabled {
        get => this.LinkfilterSettings.BlockIpLoggingWebsites;
        set => this.LinkfilterSettings.BlockIpLoggingWebsites = value;
    }

    [Column("linkfilter_shorteners")]
    public bool LinkfilterUrlShortenersEnabled {
        get => this.LinkfilterSettings.BlockUrlShorteners;
        set => this.LinkfilterSettings.BlockUrlShorteners = value;
    }
    #endregion

    #region Antiflood Settings
    [NotMapped]
    public AntifloodSettings AntifloodSettings { get; set; } = new();

    [Column("antiflood_enabled")]
    public bool AntifloodEnabled {
        get => this.AntifloodSettings.Enabled;
        set => this.AntifloodSettings.Enabled = value;
    }

    [Column("antiflood_action")]
    public Punishment.Action AntifloodAction {
        get => this.AntifloodSettings.Action;
        set => this.AntifloodSettings.Action = value;
    }

    [Column("antiflood_sensitivity")]
    public short AntifloodSensitivity {
        get => this.AntifloodSettings.Sensitivity;
        set => this.AntifloodSettings.Sensitivity = value;
    }

    [Column("antiflood_cooldown")]
    public short AntifloodCooldown {
        get => this.AntifloodSettings.Cooldown;
        set => this.AntifloodSettings.Cooldown = value;
    }
    #endregion

    #region AntiInstantLeave Settings
    [NotMapped]
    public AntiInstantLeaveSettings AntiInstantLeaveSettings { get; set; } = new();

    [Column("antiinstantleave_enabled")]
    public bool AntiInstantLeaveEnabled {
        get => this.AntiInstantLeaveSettings.Enabled;
        set => this.AntiInstantLeaveSettings.Enabled = value;
    }

    [Column("antiinstantleave_cooldown")]
    public short AntiInstantLeaveCooldown {
        get => this.AntiInstantLeaveSettings.Cooldown;
        set => this.AntiInstantLeaveSettings.Cooldown = value;
    }
    #endregion

    #region Antispam Settings
    [NotMapped]
    public AntispamSettings AntispamSettings { get; set; } = new();

    [Column("antispam_enabled")]
    public bool AntispamEnabled {
        get => this.AntispamSettings.Enabled;
        set => this.AntispamSettings.Enabled = value;
    }

    [Column("antispam_action")]
    public Punishment.Action AntispamAction {
        get => this.AntispamSettings.Action;
        set => this.AntispamSettings.Action = value;
    }

    [Column("antispam_sensitivity")]
    public short AntispamSensitivity {
        get => this.AntispamSettings.Sensitivity;
        set => this.AntispamSettings.Sensitivity = value;
    }
    #endregion

    #region AntiMention Settings
    [NotMapped]
    public AntiMentionSettings AntiMentionSettings { get; set; } = new();

    [Column("antimention_enabled")]
    public bool AntiMentionEnabled {
        get => this.AntiMentionSettings.Enabled;
        set => this.AntiMentionSettings.Enabled = value;
    }

    [Column("antimention_action")]
    public Punishment.Action AntiMentionAction {
        get => this.AntiMentionSettings.Action;
        set => this.AntiMentionSettings.Action = value;
    }

    [Column("antimention_sensitivity")]
    public short AntiMentionSensitivity {
        get => this.AntiMentionSettings.Sensitivity;
        set => this.AntiMentionSettings.Sensitivity = value;
    }
    #endregion

    #region Ratelimit Settings
    [NotMapped]
    public RatelimitSettings RatelimitSettings { get; set; } = new();

    [Column("ratelimit_enabled")]
    public bool RatelimitEnabled {
        get => this.RatelimitSettings.Enabled;
        set => this.RatelimitSettings.Enabled = value;
    }

    [Column("ratelimit_action")]
    public Punishment.Action RatelimitAction {
        get => this.RatelimitSettings.Action;
        set => this.RatelimitSettings.Action = value;
    }

    [Column("ratelimit_sensitivity")]
    public short RatelimitSensitivity {
        get => this.RatelimitSettings.Sensitivity;
        set => this.RatelimitSettings.Sensitivity = value;
    }
    #endregion


    [NotMapped]
    public CachedGuildConfig CachedConfig {
        get => new() {
            AntispamSettings = this.AntispamSettings,
            AntiMentionSettings = this.AntiMentionSettings,
            Currency = this.Currency ?? CachedGuildConfig.DefaultCurrency,
            LinkfilterSettings = this.LinkfilterSettings,
            Locale = this.Locale ?? BotConfig.DefaultLocale,
            LogChannelId = this.LogChannelId,
            Prefix = this.Prefix ?? BotConfig.DefaultPrefix,
            RatelimitSettings = this.RatelimitSettings,
            ReactionResponse = this.ReactionResponse,
            StarboardEmoji = this.StarboardEmoji,
            StarboardSensitivity = this.StarboardSensitivity,
            StarboardChannelId = this.StarboardChannelId,
            SuggestionsEnabled = this.SuggestionsEnabled,
            TimezoneId = this.TimezoneId ?? CachedGuildConfig.DefaultTimezoneId
        };
        set {
            this.AntispamSettings = value.AntispamSettings;
            this.AntiMentionSettings = value.AntiMentionSettings;
            this.Currency = value.Currency;
            this.LinkfilterSettings = value.LinkfilterSettings;
            this.Locale = value.Locale;
            this.LogChannelId = value.LogChannelId;
            this.Prefix = value.Prefix;
            this.RatelimitSettings = value.RatelimitSettings;
            this.ReactionResponse = value.ReactionResponse;
            this.StarboardEmoji = value.StarboardEmoji;
            this.StarboardSensitivity = value.StarboardSensitivity;
            this.StarboardChannelId = value.StarboardChannelId;
            this.SuggestionsEnabled = value.SuggestionsEnabled;
            this.TimezoneId = value.TimezoneId;
        }
    }
}