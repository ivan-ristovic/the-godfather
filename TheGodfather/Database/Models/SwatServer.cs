using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    [Table("swat_servers")]
    public class SwatServer
    {
        public static SwatServer FromIP(string ip, int queryport = 10481, string? name = null)
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

            return new SwatServer {
                IP = ip,
                JoinPort = joinport,
                Name = name ?? "<unknown>",
                QueryPort = queryport
            };
        }


        [Column("ip"), Required, MaxLength(16)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string IP { get; set; } = null!;

        [Column("join_port")]
        public int JoinPort { get; set; } = 10480;

        [Column("query_port")]
        public int QueryPort { get; set; }

        [Column("name"), Required, MaxLength(32)]
        public string Name { get; set; } = null!;
    }


    public sealed class SwatServerComparer : IEqualityComparer<SwatServer>
    {
        public bool Equals(SwatServer? x, SwatServer? y)
            => Equals(x?.IP, y?.IP) && Equals(x?.JoinPort, y?.JoinPort) && Equals(x?.QueryPort, y?.QueryPort);

        public int GetHashCode(SwatServer obj)
            => $"{obj.IP}:{obj.JoinPort}:{obj.QueryPort}".GetHashCode();
    }
}
