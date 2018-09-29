using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("statuses")]
    public partial class DatabaseStatuses
    {
        public string Status { get; set; }
        public short Type { get; set; }
        public int Id { get; set; }
    }
}
