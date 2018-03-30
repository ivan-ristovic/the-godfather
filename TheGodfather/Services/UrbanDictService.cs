#region USING_DIRECTIVES
using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Common;
using TheGodfather.Services.Common;

using DSharpPlus;
#endregion

namespace TheGodfather.Services
{
    public class UrbanDictService : HttpService
    {
        public static async Task<UrbanDictData> GetDefinitionForTermAsync(string query)
        {
            try {
                var result = await _http.GetStringAsync($"http://api.urbandictionary.com/v0/define?term={ WebUtility.UrlEncode(query) }")
                    .ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<UrbanDictData>(result);
                if (data.ResultType != "no_results")
                    return data;
            } catch (Exception e) {
                Logger.LogException(LogLevel.Debug, e);
            }

            return null;
        }
    }
}
