using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("bank_accounts")]
    public class DatabaseBankAccount
    {

        public DatabaseBankAccount()
        {

        }

        public DatabaseBankAccount(ulong gid, ulong uid)
        {
            this.GuildIdDb = (long)gid;
            this.UserIdDb = (long)uid;
        }


        [Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId => (ulong)this.UserIdDb;
        
        [ForeignKey("DbGuildConfig")]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId => (ulong)this.GuildIdDb;

        [Column("balance")]
        public long Balance { get; set; } = 10000;


        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }
    }
}
