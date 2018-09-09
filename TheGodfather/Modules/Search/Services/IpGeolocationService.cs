#region USING_DIRECTIVES
using Newtonsoft.Json;

using System;
using System.Net;
using System.Threading.Tasks;

using TheGodfather.Modules.Search.Common;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Search.Services
{
    public class IpGeolocationService : TheGodfatherHttpService
    {
        private static readonly string _url = "http://ip-api.com/json";


        public override bool IsDisabled() 
            => false;


        public static Task<IpInfo> GetInfoForIpAsync(string ipstr)
        {
            if (string.IsNullOrWhiteSpace(ipstr))
                throw new ArgumentException("IP missing!", nameof(ipstr));

            if (!IPAddress.TryParse(ipstr, out IPAddress ip))
                throw new ArgumentException("Given string does not map to a IPv4 address.");

            return GetInfoForIpAsync(ip);
        }

        public static async Task<IpInfo> GetInfoForIpAsync(IPAddress ip)
        {
            if (ip == null)
                throw new ArgumentNullException(nameof(ip));

            string response = await _http.GetStringAsync($"{_url}/{ip.ToString()}").ConfigureAwait(false);
            var data = JsonConvert.DeserializeObject<IpInfo>(response);
            return data;
        }
    }
}
