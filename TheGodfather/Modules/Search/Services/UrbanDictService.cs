#region USING_DIRECTIVES
using Newtonsoft.Json;

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using TheGodfather.Modules.Search.Common;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Search.Services
{
    public class UrbanDictService : TheGodfatherHttpService
    {
        private static readonly string _url = "http://api.urbandictionary.com/v0";


        public override bool IsDisabled() 
            => false;


        public static async Task<UrbanDictData> GetDefinitionForTermAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query missing", nameof(query));

            string result = await _http.GetStringAsync($"{_url}/define?term={WebUtility.UrlEncode(query)}").ConfigureAwait(false);
            var data = JsonConvert.DeserializeObject<UrbanDictData>(result);
            if (data.ResultType == "no_results" || !data.List.Any())
                return null;

            foreach (UrbanDictList res in data.List) {
                res.Definition = new string(res.Definition.ToCharArray().Where(c => c != ']' && c != '[').ToArray());
                if (!string.IsNullOrWhiteSpace(res.Example))
                    res.Example = new string(res.Example.ToCharArray().Where(c => c != ']' && c != '[').ToArray());
            }

            return data;
        }
    }
}
