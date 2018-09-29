using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("log_exempt")]
    public partial class DatabaseLogExempt
    {
        public long Id { get; set; }
        public char Type { get; set; }
        public long Gid { get; set; }

        public virtual DatabaseGuildConfig G { get; set; }
    }
}
