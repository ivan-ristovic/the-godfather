using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("items")]
    public partial class DatabaseItems
    {
        public DatabaseItems()
        {
            this.Purchases = new HashSet<DatabasePurchases>();
        }

        public int Id { get; set; }
        public long Gid { get; set; }
        public string Name { get; set; }
        public long Price { get; set; }

        public virtual DatabaseGuildConfig G { get; set; }
        public virtual ICollection<DatabasePurchases> Purchases { get; set; }
    }
}
