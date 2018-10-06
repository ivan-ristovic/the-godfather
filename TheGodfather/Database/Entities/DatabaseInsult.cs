using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("insults")]
    public class DatabaseInsult
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("content"), Required]
        public string Content { get; set; }
    }
}
