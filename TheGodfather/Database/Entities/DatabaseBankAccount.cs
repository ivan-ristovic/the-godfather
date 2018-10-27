#region USING_DIRECTIVES
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace TheGodfather.Database.Entities
{
    [Table("bank_accounts")]
    public class DatabaseBankAccount
    {
        [NotMapped]
        public static readonly int StartingBalance = 10000;


        [Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId { get => (ulong)this.UserIdDb; set => this.UserIdDb = (long)value; }
        
        [ForeignKey("DbGuildConfig")]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("balance")]
        public long Balance { get; set; } = StartingBalance;


        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }
    }
}
