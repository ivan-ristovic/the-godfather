#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Entities;

using GiphyDotNet;
using GiphyDotNet.Manager;
using GiphyDotNet.Model.Parameters;
#endregion

namespace TheGodfather.Services
{
    public class GiphyService
    {
        private Giphy _giphy { get; set; }


        public GiphyService(string key)
        {
            _giphy = new Giphy(key);
        }

        
        public async Task<GiphyDotNet.Model.GiphyImage.Data[]> SearchAsync(string query, int limit = 1)
        {
            var res = await _giphy.GifSearch(new SearchParameter() {
                Query = query,
                Limit = limit
            }).ConfigureAwait(false);

            return res.Data;
        }

        public async Task<GiphyDotNet.Model.GiphyRandomImage.Data> GetRandomGifAsync()
        {
            var res = await _giphy.RandomGif(new RandomParameter())
                .ConfigureAwait(false);

            return res.Data;
        }

        public async Task<GiphyDotNet.Model.GiphyImage.Data[]> GetTrendingGifsAsync(int limit)
        {
            var res = await _giphy.TrendingGifs(new TrendingParameter() {
                Limit = limit
            }).ConfigureAwait(false);

            return res.Data;
        }
    }
}
