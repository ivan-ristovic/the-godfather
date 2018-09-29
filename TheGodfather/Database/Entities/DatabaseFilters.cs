using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("filters")]
    public partial class DatabaseFilters
    {
        public long Gid { get; set; }
        public string Filter { get; set; }
        public int Id { get; set; }

        public virtual DatabaseGuildConfig G { get; set; }
    }
}
