#region USING_DIRECTIVES
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace TheGodfather.Database.Entities
{
    [Table("swat_players")]
    public class DatabaseSwatPlayer
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name"), Required]
        public string[] Names { get; set; }

        [Column("ip"), Required]
        public string[] IPs { get; set; }

        [Column("additional_info")]
        public string Info { get; set; }

        [Column("is_blacklisted")]
        public bool IsBlacklisted { get; set; }
    }
}
