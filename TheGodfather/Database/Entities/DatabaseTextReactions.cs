using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("text_reactions")]
    public partial class DatabaseTextReactions
    {
        public long Gid { get; set; }
        public string Trigger { get; set; }
        public string Response { get; set; }
        public int Id { get; set; }

        public virtual DatabaseGuildConfig G { get; set; }
    }
}
