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
        [NotMapped]
        public LinkfilterSettings LinkfilterSettings { get; set; } = new LinkfilterSettings();

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

        #region ANTIFLOOD
        [NotMapped]
        public AntifloodSettings AntifloodSettings { get; set; } = new AntifloodSettings();

        [Column("antiflood_enabled")]
        public bool AntifloodEnabled {
            get => this.AntifloodSettings.Enabled;
            set => this.AntifloodSettings.Enabled = value;
        }

        [Column("antiflood_action")]
        public PunishmentActionType AntifloodAction {
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

        #region ANTIINSTANTLEAVE
        [NotMapped]
        public AntiInstantLeaveSettings AntiInstantLeaveSettings { get; set; } = new AntiInstantLeaveSettings();

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

        #region ANTISPAM
        [NotMapped]
        public AntispamSettings AntispamSettings { get; set; } = new AntispamSettings();

        [Column("antispam_enabled")]
        public bool AntispamEnabled {
            get => this.AntispamSettings.Enabled;
            set => this.AntispamSettings.Enabled = value;
        }

        [Column("antispam_action")]
        public PunishmentActionType AntispamAction {
            get => this.AntispamSettings.Action;
            set => this.AntispamSettings.Action = value;
        }

        [Column("antispam_sensitivity")]
        public short AntispamSensitivity {
            get => this.AntispamSettings.Sensitivity;
            set => this.AntispamSettings.Sensitivity = value;
        }
        #endregion

        #region RATELIMIT
        [NotMapped]
        public RatelimitSettings RatelimitSettings { get; set; } = new RatelimitSettings();

        [Column("ratelimit_enabled")]
        public bool RatelimitEnabled {
            get => this.RatelimitSettings.Enabled;
            set => this.RatelimitSettings.Enabled = value;
        }

        [Column("ratelimit_action")]
        public PunishmentActionType RatelimitAction {
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
            get => new CachedGuildConfig {
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
