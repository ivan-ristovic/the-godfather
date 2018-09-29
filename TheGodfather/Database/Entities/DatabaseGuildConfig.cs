#region USING_DIRECTIVES
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace TheGodfather.Database.Entities
{
    [Table("guild_cfg")]
    public sealed class DatabaseGuildConfig
    {
        [Column("gid"), Key]
        public long GuildId { get; set; }

        [Column("welcome_cid")]
        public long WelcomeChannelId { get; set; }

        [Column("leave_cid")]
        public long LeaveChannelId { get; set; }

        [Column("welcome_message")]
        public string WelcomeMessage { get; set; }

        [Column("leave_message")]
        public string LeaveMessage { get; set; }

        [Column("prefix")]
        public string Prefix { get; set; }

        [Column("currency")]
        public string Currency { get; set; }

        [Column("mute_rid")]
        public long MuteRoleId { get; set; }

        [Column("suggestions_enabled")]
        public bool SuggestionsEnabled { get; set; }

        [Column("log_cid")]
        public long LogChannelId { get; set; }

        [Column("linkfilter_enabled")]
        public bool LinkfilterEnabled { get; set; }

        [Column("linkfilter_invites")]
        public bool LinkfilterDiscordInvites { get; set; }

        [Column("linkfilter_booters")]
        public bool LinkfilterBooters { get; set; }

        [Column("linkfilter_disturbing")]
        public bool LinkfilterDisturbing { get; set; }

        [Column("linkfilter_iploggers")]
        public bool LinkfilterIpLoggers { get; set; }

        [Column("linkfilter_shorteners")]
        public bool LinkfilterUrlShorteners { get; set; }

        [Column("silent_respond")]
        public bool SilentRespond { get; set; }

        [Column("ratelimit_enabled")]
        public bool RatelimitEnabled { get; set; }

        [Column("ratelimit_action")]
        public short RatelimitAction { get; set; }

        [Column("ratelimit_sens")]
        public short RatelimitSensitivity { get; set; }

        [Column("antiflood_enabled")]
        public bool AntifloodEnabled { get; set; }

        [Column("antiflood_action")]
        public short AntifloodAction { get; set; }

        [Column("antiflood_sens")]
        public short AntifloodSensitivity { get; set; }

        [Column("antiflood_cooldown")]
        public short AntifloodCooldown { get; set; }

        [Column("antispam_enabled")]
        public bool AntispamEnabled { get; set; }

        [Column("antispam_action")]
        public short AntispamAction { get; set; }

        [Column("antispam_sens")]
        public short AntispamSensitivity { get; set; }

        [Column("antijoinleave_enabled")]
        public bool AntiInstantLeaveEnabled { get; set; }

        [Column("antijoinleave_cooldown")]
        public short AntiInstantLeaveCooldown { get; set; }
    }
}
