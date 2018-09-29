using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("accounts")]
    public partial class DatabaseAccounts
    {
        public long Uid { get; set; }
        public long Balance { get; set; }
        public long Gid { get; set; }

        public virtual DatabaseGuildConfig G { get; set; }
    }
}
