#region USING_DIRECTIVES
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TheGodfather.Services.Common;
#endregion

namespace TheGodfather.Services
{
    public class UrbanDictService : TheGodfatherHttpService
    {
        private static readonly string _url = "http://api.urbandictionary.com/v0";


        public static async Task<UrbanDictData> GetDefinitionForTermAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query missing", "query");

            string result = await _http.GetStringAsync($"{_url}/define?term={WebUtility.UrlEncode(query)}").ConfigureAwait(false);
            var data = JsonConvert.DeserializeObject<UrbanDictData>(result);
            if (data.ResultType == "no_results" || !data.List.Any())
                return null;

            return data;
        }
    }
}
