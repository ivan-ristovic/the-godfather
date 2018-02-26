#region USING_DIRECTIVES
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace TheGodfather.Services
{
    public static class UrbanDictService
    {
        public static async Task<UrbanDictData> GetDefinitionForTermAsync(string query)
        {
            try {
                using (var http = new HttpClient()) {
                    var result = await http.GetStringAsync($"http://api.urbandictionary.com/v0/define?term={ WebUtility.UrlEncode(query) }")
                        .ConfigureAwait(false);
                    var data = JsonConvert.DeserializeObject<UrbanDictData>(result);
                    if (data.ResultType != "no_results")
                        return data;
                }
            } catch {

            }

            return null;
        }

        
        public class UrbanDictData
        {
            [JsonProperty("tags")]
            public string[] Tags { get; set; }

            [JsonProperty("result_type")]
            public string ResultType { get; set; }

            [JsonProperty("list")]
            public UrbanDictList[] List { get; set; }

            [JsonProperty("sounds")]
            public string[] Sounds { get; set; }
        }

        public class UrbanDictList
        {
            [JsonProperty("definition")]
            public string Definition { get; set; }

            [JsonProperty("permalink")]
            public string Permalink { get; set; }

            [JsonProperty("thumbs_up")]
            public int ThumbsUp { get; set; }

            [JsonProperty("author")]
            public string Author { get; set; }

            [JsonProperty("word")]
            public string Word { get; set; }

            [JsonProperty("defid")]
            public int Defid { get; set; }

            [JsonProperty("current_vote")]
            public string CurrentVote { get; set; }

            [JsonProperty("example")]
            public string Example { get; set; }

            [JsonProperty("thumbs_down")]
            public int ThumbsDown { get; set; }
        }
    }

}
