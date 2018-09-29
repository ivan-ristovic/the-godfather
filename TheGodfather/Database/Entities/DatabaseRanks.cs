using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("ranks")]
    public partial class DatabaseRanks
    {
        public long Gid { get; set; }
        public short Rank { get; set; }
        public string Name { get; set; }

        public virtual DatabaseGuildConfig G { get; set; }
    }
}
