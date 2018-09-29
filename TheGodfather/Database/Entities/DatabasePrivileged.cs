using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("privileged")]
    public partial class DatabasePrivileged
    {
        public long Uid { get; set; }
    }
}
