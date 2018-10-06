using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("swat_servers")]
    public class DatabaseSwatServer
    {
        [Key]
        [Column("ip"), Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string IP { get; set; }

        [Column("join_port")]
        public int JoinPort { get; set; } = 10480;

        [Column("query_port")]
        public int QueryPort { get; set; }
        
        [Column("name"), Required]
        public string Name { get; set; }
    }
}
