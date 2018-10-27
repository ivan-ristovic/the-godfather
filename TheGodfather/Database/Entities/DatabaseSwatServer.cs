#region USING_DIRECTIVES
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace TheGodfather.Database.Entities
{
    [Table("swat_servers")]
    public class DatabaseSwatServer
    {
        [Key]
        [Column("ip"), Required, MaxLength(16)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string IP { get; set; }

        [Column("join_port")]
        public int JoinPort { get; set; } = 10480;

        [Column("query_port")]
        public int QueryPort { get; set; }
        
        [Column("name"), Required, MaxLength(32)]
        public string Name { get; set; }
    }
}
