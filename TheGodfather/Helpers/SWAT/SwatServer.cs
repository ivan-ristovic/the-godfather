using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TheGodfather.Helpers.Swat
{
    public class SwatServer
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("IP")]
        public string IP { get; set; }

        [JsonProperty("JoinPort")]
        public int JoinPort { get; set; }

        [JsonProperty("QueryPort")]
        public int QueryPort { get; set; }


        public SwatServer(string name, string ip, int joinport, int queryport)
        {
            Name = name;
            IP = ip;
            JoinPort = joinport;
            QueryPort = queryport;
        }
    }
}
