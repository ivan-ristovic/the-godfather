#region USING_DIRECTIVES
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace TheGodfather.Database.Entities
{
    [Table("swat_servers")]
    public class DatabaseSwatServer
    {
        public static DatabaseSwatServer FromIP(string ip, int queryport = 10481, string name = null)
        {
            int joinport = 10480;

            string[] split = ip.Split(':');
            ip = split[0];
            if (split.Length > 1) {
                if (!int.TryParse(split[1], out joinport))
                    joinport = 10480;
            }
            if (queryport == 10481)
                queryport = joinport + 1;


            return new DatabaseSwatServer {
                IP = ip,
                JoinPort = joinport,
                Name = name,
                QueryPort = queryport
            };
        }

        
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


    public sealed class DatabaseSwatServerComparer : IEqualityComparer<DatabaseSwatServer>
    {
        public bool Equals(DatabaseSwatServer x, DatabaseSwatServer y)
            => x.IP == y.IP && x.JoinPort == y.JoinPort && x.QueryPort == y.QueryPort;

        public int GetHashCode(DatabaseSwatServer obj)
            => $"{obj.IP}:{obj.JoinPort}:{obj.QueryPort}".GetHashCode();
    }
}
