using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("birthdays")]
    public class DatabaseBirthday
    {
        [ForeignKey("DbGuildConfig")]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId => (ulong)this.GuildIdDb;

        [Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId => (ulong)this.UserIdDb;

        [Column("cid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ChannelIdDb { get; set; }
        [NotMapped]
        public ulong ChannelId => (ulong)this.ChannelIdDb;

        [Column("date", TypeName = "date")]
        public DateTime Date { get; set; }

        [Column("last_update_year")]
        public int LastUpdateYear { get; set; }


        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }
    }
}
