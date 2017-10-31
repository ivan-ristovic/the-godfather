#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Commands.Search
{
    public class CommandsUrbanDict
    {
        [Command("urban")]
        [Description("Search Urban Dictionary for a query.")]
        [Aliases("ud")]
        [Cooldown(2, 5, CooldownBucketType.User), Cooldown(4, 5, CooldownBucketType.Channel)]
        [CheckIgnore]
        public async Task UrbanDictAsync(CommandContext ctx,
                                        [RemainingText, Description("Query.")] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                throw new InvalidCommandUsageException("Query missing.");

            var data = await UrbanDict.GetDataAsync(q)
                .ConfigureAwait(false);
            if (data.Key) {
                var interactivity = ctx.Client.GetInteractivityModule();
                foreach (var v in data.Value.List) {
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                        Description = v.Definition.Length < 1000 ? v.Definition : v.Definition.Take(1000) + "...",
                        Color = DiscordColor.CornflowerBlue
                    }.Build()).ConfigureAwait(false);

                    var msg = await interactivity.WaitForMessageAsync(
                        m => m.Channel.Id == ctx.Channel.Id && m.Content.ToLower() == "next"
                        , TimeSpan.FromSeconds(5)
                    ).ConfigureAwait(false);
                    if (msg == null)
                        break;
                }
            } else {
                await ctx.RespondAsync("No results found!")
                    .ConfigureAwait(false);
            }
        }
    }

    #region HELPER_CLASSES
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

    public class UrbanDict
    {
        public async static Task<KeyValuePair<bool, UrbanDictData>> GetDataAsync(string query)
        {
            using (var http = new HttpClient()) {
                var result = await http.GetStringAsync($"http://api.urbandictionary.com/v0/define?term={ WebUtility.UrlEncode(query) }");
                var data = JsonConvert.DeserializeObject<UrbanDictData>(result);

                return new KeyValuePair<bool, UrbanDictData>(data.ResultType == "no_results" ? false : true, data);
            }
        }
    }
    #endregion
}