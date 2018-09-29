using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("swat_servers")]
    public partial class DatabaseSwatServers
    {
        public string Ip { get; set; }
        public int Joinport { get; set; }
        public int Queryport { get; set; }
        public string Name { get; set; }
    }
}
