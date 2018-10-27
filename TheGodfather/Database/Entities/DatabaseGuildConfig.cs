#region USING_DIRECTIVES
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TheGodfather.Common;
using TheGodfather.Modules.Administration.Common;
#endregion

namespace TheGodfather.Database.Entities
{
    [Table("guild_cfg")]
    public class DatabaseGuildConfig
    {

        public DatabaseGuildConfig()
        {
            this.Accounts = new HashSet<DatabaseBankAccount>();
            this.AntispamExempts = new HashSet<DatabaseExemptAntispam>();
            this.AutoRoles = new HashSet<DatabaseAutoRole>();
            this.Birthdays = new HashSet<DatabaseBirthday>();
            this.Chickens = new HashSet<DatabaseChicken>();
            this.ChickensBoughtUpgrades = new HashSet<DatabaseChickenBoughtUpgrade>();
            this.EmojiReactions = new HashSet<DatabaseEmojiReaction>();
            this.Filters = new HashSet<DatabaseFilter>();
            this.LoggingExempts = new HashSet<DatabaseExemptLogging>();
            this.Memes = new HashSet<DatabaseMeme>();
            this.PurchasableItems = new HashSet<DatabasePurchasableItem>();
            this.Ranks = new HashSet<DatabaseGuildRank>();
            this.RatelimitExempts = new HashSet<DatabaseExemptRatelimit>();
            this.SavedTasks = new HashSet<DatabaseSavedTask>();
            this.SelfRoles = new HashSet<DatabaseSelfRole>();
            this.Subscriptions = new HashSet<DatabaseRssSubscription>();
            this.TextReactions = new HashSet<DatabaseTextReaction>();
        }


        [Key]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("prefix"), MaxLength(16)]
        public string Prefix { get; set; }

        [Column("currency"), MaxLength(32)]
        public string Currency { get; set; }

        [Column("suggestions_enabled")]
        public bool SuggestionsEnabled { get; set; }

        [Column("log_cid")]
        public long? LogChannelIdDb { get; set; } 
        [NotMapped]
        public ulong LogChannelId { get => (ulong)this.LogChannelIdDb.GetValueOrDefault(); set => this.LogChannelIdDb = (long)value; }
        [NotMapped]
        public bool LoggingEnabled => this.LogChannelId != default;

        [Column("mute_rid")]
        public long? MuteRoleIdDb { get; set; }
        [NotMapped]
        public ulong MuteRoleId { get => (ulong) this.MuteRoleIdDb.GetValueOrDefault(); set => this.MuteRoleIdDb = (long)value; }

        [Column("silent_response_enabled")]
        public bool ReactionResponse { get; set; }

        #region MEMBER_UPDATES
        [Column("welcome_cid")]
        public long? WelcomeChannelIdDb { get; set; }
        [NotMapped]
        public ulong WelcomeChannelId => (ulong)this.WelcomeChannelIdDb.GetValueOrDefault();

        [Column("leave_cid")]
        public long? LeaveChannelIdDb { get; set; }
        [NotMapped]
        public ulong LeaveChannelId => (ulong)this.LeaveChannelIdDb.GetValueOrDefault();

        [Column("welcome_msg"), MaxLength(128)]
        public string WelcomeMessage { get; set; }

        [Column("leave_msg"), MaxLength(128)]
        public string LeaveMessage { get; set; }
        #endregion

        #region LINKFILTER
        [Column("linkfilter_enabled")]
        public bool LinkfilterEnabled { get; set; }

        [Column("linkfilter_booters")]
        public bool LinkfilterBootersEnabled { get; set; } = true;

        [Column("linkfilter_disturbing")]
        public bool LinkfilterDisturbingWebsitesEnabled { get; set; } = true;

        [Column("linkfilter_invites")]
        public bool LinkfilterDiscordInvitesEnabled { get; set; }

        [Column("linkfilter_loggers")]
        public bool LinkfilterIpLoggersEnabled { get; set; } = true;

        [Column("linkfilter_shorteners")]
        public bool LinkfilterUrlShortenersEnabled { get; set; } = true;

        [NotMapped]
        public LinkfilterSettings LinkfilterSettings {
            get => new LinkfilterSettings() {
                BlockBooterWebsites = this.LinkfilterBootersEnabled,
                BlockDiscordInvites = this.LinkfilterDiscordInvitesEnabled,
                BlockDisturbingWebsites = this.LinkfilterDisturbingWebsitesEnabled,
                BlockIpLoggingWebsites = this.LinkfilterIpLoggersEnabled,
                BlockUrlShorteners = this.LinkfilterUrlShortenersEnabled,
                Enabled = this.LinkfilterEnabled
            };
            set {
                this.LinkfilterEnabled = value.Enabled;
                this.LinkfilterBootersEnabled = value.BlockBooterWebsites;
                this.LinkfilterDiscordInvitesEnabled = value.BlockDiscordInvites;
                this.LinkfilterDisturbingWebsitesEnabled = value.BlockDisturbingWebsites;
                this.LinkfilterIpLoggersEnabled = value.BlockIpLoggingWebsites;
                this.LinkfilterUrlShortenersEnabled = value.BlockUrlShorteners;
            }
        }
        #endregion

        #region ANTIFLOOD
        [Column("antiflood_enabled")]
        public bool AntifloodEnabled { get; set; }

        [Column("antiflood_action")]
        public PunishmentActionType AntifloodAction { get; set; } = PunishmentActionType.PermanentBan;

        [Column("antiflood_sensitivity")]
        public short AntifloodSensitivity { get; set; } = 5;

        [Column("antiflood_cooldown")]
        public short AntifloodCooldown { get; set; } = 10;

        [NotMapped]
        public AntifloodSettings AntifloodSettings {
            get => new AntifloodSettings() {
                Action = this.AntifloodAction,
                Cooldown = this.AntifloodCooldown,
                Enabled = this.AntifloodEnabled,
                Sensitivity = this.AntifloodSensitivity
            };
            set {
                this.AntifloodAction = value.Action;
                this.AntifloodCooldown = value.Cooldown;
                this.AntifloodEnabled = value.Enabled;
                this.AntifloodSensitivity = value.Sensitivity;
            }
        }
        #endregion

        #region ANTIINSTANTLEAVE
        [Column("antilnstantleave_enabled")]
        public bool AntiInstantLeaveEnabled { get; set; }

        [Column("antiinstantleave_cooldown")]
        public short AntiInstantLeaveCooldown { get; set; } = 3;

        [NotMapped]
        public AntiInstantLeaveSettings AntiInstantLeaveSettings {
            get => new AntiInstantLeaveSettings() {
                Cooldown = this.AntiInstantLeaveCooldown,
                Enabled = this.AntiInstantLeaveEnabled
            };
            set {
                this.AntiInstantLeaveCooldown = value.Cooldown;
                this.AntiInstantLeaveEnabled = value.Enabled;
            }
        }
        #endregion

        #region ANTISPAM
        [Column("antispam_enabled")]
        public bool AntispamEnabled { get; set; }

        [Column("antispam_action")]
        public PunishmentActionType AntispamAction { get; set; } = PunishmentActionType.TemporaryMute;

        [Column("antispam_sensitivity")]
        public short AntispamSensitivity { get; set; } = 5;

        [NotMapped]
        public AntispamSettings AntispamSettings {
            get => new AntispamSettings() {
                Action = this.AntispamAction,
                Enabled = this.AntispamEnabled,
                Sensitivity = this.AntispamSensitivity
            };
            set {
                this.AntispamAction = value.Action;
                this.AntispamEnabled = value.Enabled;
                this.AntispamSensitivity = value.Sensitivity;
            }
        }
        #endregion

        #region RATELIMIT
        [Column("ratelimit_enabled")]
        public bool RatelimitEnabled { get; set; }

        [Column("ratelimit_action")]
        public PunishmentActionType RatelimitAction { get; set; } = PunishmentActionType.TemporaryMute;

        [Column("ratelimit_sensitivity")]
        public short RatelimitSensitivity { get; set; } = 5;

        [NotMapped]
        public RatelimitSettings RatelimitSettings {
            get => new RatelimitSettings() {
                Action = this.RatelimitAction,
                Enabled = this.RatelimitEnabled,
                Sensitivity = this.RatelimitSensitivity
            };
            set {
                this.RatelimitAction = value.Action;
                this.RatelimitEnabled = value.Enabled;
                this.RatelimitSensitivity = value.Sensitivity;
            }
        }
        #endregion

        [NotMapped]
        public CachedGuildConfig CachedConfig {
            get => new CachedGuildConfig() {
                AntispamSettings = this.AntispamSettings,
                Currency = this.Currency,
                LinkfilterSettings = this.LinkfilterSettings,
                LogChannelId = this.LogChannelId,
                Prefix = this.Prefix,
                RatelimitSettings = this.RatelimitSettings,
                ReactionResponse = this.ReactionResponse,
                SuggestionsEnabled = this.SuggestionsEnabled
            };
            set {
                this.AntispamSettings = value.AntispamSettings;
                this.Currency = value.Currency;
                this.LinkfilterSettings = value.LinkfilterSettings;
                this.LogChannelId = value.LogChannelId;
                this.Prefix = value.Prefix;
                this.RatelimitSettings = value.RatelimitSettings;
                this.ReactionResponse = value.ReactionResponse;
                this.SuggestionsEnabled = value.SuggestionsEnabled;
            }
        }


        public virtual ICollection<DatabaseBankAccount> Accounts { get; set; }
        public virtual ICollection<DatabaseExemptAntispam> AntispamExempts { get; set; }
        public virtual ICollection<DatabaseAutoRole> AutoRoles { get; set; }
        public virtual ICollection<DatabaseBirthday> Birthdays { get; set; }
        public virtual ICollection<DatabaseChicken> Chickens { get; set; }
        public virtual ICollection<DatabaseChickenBoughtUpgrade> ChickensBoughtUpgrades { get; set; }
        public virtual ICollection<DatabaseEmojiReaction> EmojiReactions { get; set; }
        public virtual ICollection<DatabaseFilter> Filters { get; set; }
        public virtual ICollection<DatabaseExemptLogging> LoggingExempts { get; set; }
        public virtual ICollection<DatabaseMeme> Memes { get; set; }
        public virtual ICollection<DatabasePurchasableItem> PurchasableItems { get; set; }
        public virtual ICollection<DatabaseGuildRank> Ranks { get; set; }
        public virtual ICollection<DatabaseExemptRatelimit> RatelimitExempts { get; set; }
        public virtual ICollection<DatabaseSavedTask> SavedTasks { get; set; }
        public virtual ICollection<DatabaseSelfRole> SelfRoles { get; set; }
        public virtual ICollection<DatabaseRssSubscription> Subscriptions { get; set; }
        public virtual ICollection<DatabaseTextReaction> TextReactions { get; set; }
    }
}
