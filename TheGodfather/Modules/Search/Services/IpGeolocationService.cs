#region USING_DIRECTIVES
using Newtonsoft.Json;

using System;
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


        public static async Task<IpInfo> GetInfoForIpAsync(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new ArgumentException("IP missing!", nameof(ip));

            string response = await _http.GetStringAsync($"{_url}/{ip}").ConfigureAwait(false);
            var data = JsonConvert.DeserializeObject<IpInfo>(response);
            return data;
        }
    }
}
