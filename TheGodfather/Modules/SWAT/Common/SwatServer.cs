using System;

namespace TheGodfather.Modules.SWAT.Common
{
    public sealed class SwatServer
    {
        public string Name { get; set; }
        public string Ip { get; set; }
        public int JoinPort { get; set; }
        public int QueryPort { get; set; }


        public SwatServer(string name, string ip, int joinport, int queryport)
        {
            Name = name;
            Ip = ip;
            JoinPort = joinport;
            QueryPort = queryport;
        }


        public static SwatServer FromIP(string ip, int queryport = 10481, string name = null)
        {
            int joinport = 10480;

            var split = ip.Split(':');
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
}
