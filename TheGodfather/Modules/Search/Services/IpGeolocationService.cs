using System.Net;
using Newtonsoft.Json;
using TheGodfather.Modules.Search.Common;

namespace TheGodfather.Modules.Search.Services;

public sealed class IpGeolocationService : TheGodfatherHttpService
{
    private const string Endpoint = "http://ip-api.com/json";

    public override bool IsDisabled => false;


    public static Task<IpInfo?> GetInfoForIpAsync(string ipstr)
    {
        if (string.IsNullOrWhiteSpace(ipstr) || !IPAddress.TryParse(ipstr, out IPAddress? ip))
            return Task.FromResult<IpInfo?>(null);

        return GetInfoForIpAsync(ip);
    }

    public static async Task<IpInfo?> GetInfoForIpAsync(IPAddress? ip)
    {
        if (ip is null)
            return null;

        try {
            string response = await _http.GetStringAsync($"{Endpoint}/{ip}").ConfigureAwait(false);
            IpInfo data = JsonConvert.DeserializeObject<IpInfo>(response) ?? throw new JsonSerializationException();
            return data;
        } catch {
            return null;
        }
    }
}