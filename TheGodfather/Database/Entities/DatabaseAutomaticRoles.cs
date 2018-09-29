using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("automatic_roles")]
    public partial class DatabaseAutomaticRoles
    {
        public long Gid { get; set; }
        public long Rid { get; set; }

        public virtual DatabaseGuildConfig G { get; set; }
    }
}
