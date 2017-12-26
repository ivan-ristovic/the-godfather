using System;

namespace TheGodfather.Helpers.Swat
{
    public class SwatServer
    {
        public string Name { get; set; }
        public string IP { get; set; }
        public int JoinPort { get; set; }
        public int QueryPort { get; set; }


        public SwatServer(string name, string ip, int joinport, int queryport)
        {
            Name = name;
            IP = ip;
            JoinPort = joinport;
            QueryPort = queryport;
        }


        public static SwatServer CreateFromIP(string ip, int queryport = 10481, string name = null)
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
