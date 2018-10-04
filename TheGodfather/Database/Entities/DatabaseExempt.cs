using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    public partial class DatabaseExempt
    {
        public long Id { get; set; }
        public char Type { get; set; }
        public long Gid { get; set; }

        public virtual DatabaseGuildConfig G { get; set; }
    }

    [Table("log_exempt")]
    public partial class DatabaseLogExempt : DatabaseExempt
    {

    }

    [Table("antispam_exempt")]
    public partial class DatabaseAntispamExempt : DatabaseExempt
    {

    }

    [Table("ratelimit_exempt")]
    public partial class DatabaseRatelimitExempt : DatabaseExempt
    {

    }
}
