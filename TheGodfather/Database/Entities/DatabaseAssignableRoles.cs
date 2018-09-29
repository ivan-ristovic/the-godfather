using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("assignable_roles")]
    public partial class AssignableRoles
    {
        public long Gid { get; set; }
        public long Rid { get; set; }

        public virtual DatabaseGuildConfig G { get; set; }
    }
}
