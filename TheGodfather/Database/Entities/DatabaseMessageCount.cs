using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("msgcount")]
    public partial class DatabaseMessageCount
    {
        public long Uid { get; set; }
        public long Count { get; set; }
    }
}
