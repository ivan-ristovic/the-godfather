#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
#endregion


namespace TheGodfatherBot.Modules.Search
{
    [Group("youtube", CanInvokeWithoutSubcommand = true)]
    [Description("Youtube search commands.")]
    [Aliases("y", "yt")]
    public class CommandsYoutube
    {

        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Search query.")] string query = null,
                                           [Description("Number of results.")] int ammount = 1)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Search query missing.");

            var results = await GetYoutubeResults(query, ammount);
            
            await ctx.RespondAsync($"Search results for ***{query}***", embed: EmbedYouTubeResults(results));
        }


        #region HELPER_FUNCTIONS
        private async Task<List<SearchResult>> GetYoutubeResults(string query, int ammount)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer() {
                ApiKey = TheGodfather.GetToken("Resources/youtube.txt"),
                ApplicationName = "TheGodfather"
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = query;
            searchListRequest.MaxResults = ammount;

            var searchListResponse = await searchListRequest.ExecuteAsync();

            List<SearchResult> videos = new List<SearchResult>();
            videos.AddRange(searchListResponse.Items);

            return videos;
        }
        
        private DiscordEmbed EmbedYouTubeResults(List<SearchResult> results)
        {
            var em = new DiscordEmbedBuilder() {
                Color = DiscordColor.Red
            };
            foreach (var r in results) {
                switch (r.Id.Kind) {
                    case "youtube#video":
                        em.AddField(r.Snippet.Title, "https://www.youtube.com/watch?v=" + r.Id.VideoId);
                        break;

                    case "youtube#channel":
                        em.AddField(r.Snippet.Title, "https://www.youtube.com/channel/" + r.Id.ChannelId);
                        break;

                    case "youtube#playlist":
                        em.AddField(r.Snippet.Title, "https://www.youtube.com/playlist?list=" + r.Id.PlaylistId);
                        break;
                }
            }

            return em;
        }
        #endregion
    }
}
