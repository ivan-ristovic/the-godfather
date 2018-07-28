#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TheGodfather.Services.Common;
#endregion

namespace TheGodfather.Services
{
    public class IpGeolocationService : TheGodfatherHttpService
    {
        private static readonly string _url = "http://ip-api.com/json";
        

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
