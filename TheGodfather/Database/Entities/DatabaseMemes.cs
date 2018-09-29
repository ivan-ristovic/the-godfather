using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("memes")]
    public partial class DatabaseMemes
    {
        public long Gid { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public virtual DatabaseGuildConfig G { get; set; }
    }
}
