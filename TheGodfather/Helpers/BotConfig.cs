#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
#endregion

namespace TheGodfather.Helpers
{
    public class BotConfig
    {
        [JsonProperty("token")]
        public string Token { get; private set; }
        
        [JsonProperty("prefix")]
        public string DefaultPrefix { get; private set; }

        [JsonProperty("key-giphy")]
        public string GiphyKey { get; private set; }

        [JsonProperty("key-steam")]
        public string SteamKey { get; private set; }

        [JsonProperty("key-imgur")]
        public string ImgurKey { get; private set; }

        [JsonProperty("key-youtube")]
        public string YoutubeKey { get; private set; }


        public static BotConfig Load()
        {
            BotConfig cfg = null;
            if (File.Exists("Resources/config.json")) {
                try {
                    cfg = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("Resources/config.json"));
                } catch (Exception e) {
                    Console.WriteLine("EXCEPTION OCCURED WHILE LOADING CONFIG FILE: " + Environment.NewLine + e.ToString());
                    return null;
                }
            } else {
                return null;
            }

            return cfg;
        }
    }
}
