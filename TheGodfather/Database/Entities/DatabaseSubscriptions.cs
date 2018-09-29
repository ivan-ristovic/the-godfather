using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("subscriptions")]
    public partial class DatabaseSubscriptions
    {
        public int Id { get; set; }
        public long Cid { get; set; }
        public string Qname { get; set; }

        public virtual DatabaseFeeds IdNavigation { get; set; }
    }
}
