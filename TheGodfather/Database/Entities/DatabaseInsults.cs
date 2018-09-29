using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("insults")]
    public partial class DatabaseInsults
    {
        public int Id { get; set; }
        public string Insult { get; set; }
    }
}
