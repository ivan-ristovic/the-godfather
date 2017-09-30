#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfatherBot.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfatherBot.Modules.Search
{
    public class CommandsUrbanDict
    {
        [Command("urban")]
        [Description("Search Urban Dictionary for a query.")]
        [Aliases("ud")]
        public async Task UrbanDictAsync(CommandContext ctx,
                                        [RemainingText, Description("Query.")] string q = null)
        {
            if (string.IsNullOrWhiteSpace(q))
                throw new InvalidCommandUsageException("Query missing.");

            var data = await UrbanDict.GetDataAsync(q);
            if (data.Key) {
                var interactivity = ctx.Client.GetInteractivityModule();
                foreach (var v in data.Value.List) {
                    await ctx.RespondAsync("", embed: new DiscordEmbedBuilder() {
                        Description = v.Definition,
                        Color = DiscordColor.CornflowerBlue
                    });

                    var t = interactivity.WaitForMessageAsync(
                        m => m.Channel.Id == ctx.Channel.Id && m.Content.ToLower() == "next",
                        TimeSpan.FromSeconds(10)
                    );
                    t.Wait();
                    if (t.Result == null)
                        break;
                }
            } else {
                await ctx.RespondAsync("No results found!");
            }
        }
    }

    #region HELPER_CLASSES
    public class List
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

    public class Data
    {
        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("result_type")]
        public string ResultType { get; set; }

        [JsonProperty("list")]
        public List[] List { get; set; }

        [JsonProperty("sounds")]
        public string[] Sounds { get; set; }
    }

    public class UrbanDict
    {
        public async static Task<KeyValuePair<bool, Data>> GetDataAsync(string query)
        {
            using (var http = new HttpClient()) {
                var result = await http.GetStringAsync($"http://api.urbandictionary.com/v0/define?term={ WebUtility.UrlEncode(query) }");
                var data = JsonConvert.DeserializeObject<Data>(result);

                return new KeyValuePair<bool, Data>(data.ResultType == "no_results" ? false : true, data);
            }
        }
    }
    #endregion
}