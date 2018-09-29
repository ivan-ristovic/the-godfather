using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("guild_cfg")]
    public partial class DatabaseGuildConfig
    {
        public DatabaseGuildConfig()
        {
            this.Accounts = new HashSet<DatabaseAccounts>();
            this.AssignableRoles = new HashSet<AssignableRoles>();
            this.AutomaticRoles = new HashSet<DatabaseAutomaticRoles>();
            this.Chickens = new HashSet<DatabaseChickens>();
            this.EmojiReactions = new HashSet<DatabaseEmojiReactions>();
            this.Filters = new HashSet<DatabaseFilters>();
            this.Items = new HashSet<DatabaseItems>();
            this.LogExempt = new HashSet<DatabaseLogExempt>();
            this.Memes = new HashSet<DatabaseMemes>();
            this.Ranks = new HashSet<DatabaseRanks>();
            this.SavedTasks = new HashSet<DatabaseSavedTasks>();
            this.TextReactions = new HashSet<DatabaseTextReactions>();
        }

        public long Gid { get; set; }
        public long WelcomeCid { get; set; }
        public long LeaveCid { get; set; }
        public string WelcomeMsg { get; set; }
        public string LeaveMsg { get; set; }
        public string Prefix { get; set; }
        public bool SuggestionsEnabled { get; set; }
        public long LogCid { get; set; }
        public bool LinkfilterEnabled { get; set; }
        public bool LinkfilterInvites { get; set; }
        public bool LinkfilterBooters { get; set; }
        public bool LinkfilterDisturbing { get; set; }
        public bool LinkfilterIploggers { get; set; }
        public bool LinkfilterShorteners { get; set; }
        public bool SilentRespond { get; set; }
        public string Currency { get; set; }
        public bool RatelimitEnabled { get; set; }
        public short RatelimitAction { get; set; }
        public short RatelimitSens { get; set; }
        public bool AntifloodEnabled { get; set; }
        public short AntifloodSens { get; set; }
        public short AntifloodCooldown { get; set; }
        public short AntifloodAction { get; set; }
        public long MuteRid { get; set; }
        public bool AntijoinleaveEnabled { get; set; }
        public short AntijoinleaveCooldown { get; set; }
        public bool AntispamEnabled { get; set; }
        public short AntispamAction { get; set; }
        public short AntispamSens { get; set; }

        public virtual ICollection<DatabaseAccounts> Accounts { get; set; }
        public virtual ICollection<AssignableRoles> AssignableRoles { get; set; }
        public virtual ICollection<DatabaseAutomaticRoles> AutomaticRoles { get; set; }
        public virtual ICollection<DatabaseChickens> Chickens { get; set; }
        public virtual ICollection<DatabaseEmojiReactions> EmojiReactions { get; set; }
        public virtual ICollection<DatabaseFilters> Filters { get; set; }
        public virtual ICollection<DatabaseItems> Items { get; set; }
        public virtual ICollection<DatabaseLogExempt> LogExempt { get; set; }
        public virtual ICollection<DatabaseMemes> Memes { get; set; }
        public virtual ICollection<DatabaseRanks> Ranks { get; set; }
        public virtual ICollection<DatabaseSavedTasks> SavedTasks { get; set; }
        public virtual ICollection<DatabaseTextReactions> TextReactions { get; set; }
    }
}
