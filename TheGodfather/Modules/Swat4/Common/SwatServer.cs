using System.Collections.Generic;

namespace TheGodfather.Modules.Swat.Common
{
    public sealed class SwatServer
    {
        public string Name { get; set; }
        public string Ip { get; set; }
        public int JoinPort { get; set; }
        public int QueryPort { get; set; }


        public SwatServer(string name, string ip, int joinport, int queryport)
        {
            this.Name = name;
            this.Ip = ip;
            this.JoinPort = joinport;
            this.QueryPort = queryport;
        }


        public static SwatServer FromIP(string ip, int queryport = 10481, string name = null)
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

            return new SwatServer(name, ip, joinport, queryport);
        }

    }

    public sealed class SwatServerComparer : IEqualityComparer<SwatServer>
    {
        public bool Equals(SwatServer x, SwatServer y)
            => x.Ip == y.Ip && x.JoinPort == y.JoinPort && x.QueryPort == y.QueryPort;

        public int GetHashCode(SwatServer obj)
            => $"{obj.Ip}:{obj.JoinPort}:{obj.QueryPort}".GetHashCode();
    }
}
