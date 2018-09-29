using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("purchases")]
    public partial class DatabasePurchases
    {
        public int Id { get; set; }
        public long Uid { get; set; }

        public virtual DatabaseItems IdNavigation { get; set; }
    }
}
