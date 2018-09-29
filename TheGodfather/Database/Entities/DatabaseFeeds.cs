using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("feeds")]
    public partial class DatabaseFeeds
    {
        public DatabaseFeeds()
        {
            this.Subscriptions = new HashSet<DatabaseSubscriptions>();
        }

        public int Id { get; set; }
        public string Url { get; set; }
        public string Savedurl { get; set; }

        public virtual ICollection<DatabaseSubscriptions> Subscriptions { get; set; }
    }
}
