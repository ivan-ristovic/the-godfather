#region USING_DIRECTIVES
using GiphyDotNet.Manager;
using GiphyDotNet.Model.Parameters;
using System;
using System.Threading.Tasks;
using ImageData = GiphyDotNet.Model.GiphyImage.Data;
using RandomImageData = GiphyDotNet.Model.GiphyRandomImage.Data;
#endregion

namespace TheGodfather.Services
{
    public class GiphyService : ITheGodfatherService
    {
        private readonly Giphy giphy;


        public GiphyService(string key)
        {
            this.giphy = new Giphy(key);
        }

        
        public async Task<ImageData[]> SearchAsync(string query, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or whitespace", "query");

            if (amount < 1 || amount > 20)
                throw new ArgumentException("Result amount out of range", "amount");

            var res = await this.giphy.GifSearch(new SearchParameter() {
                Query = query,
                Limit = amount
            }).ConfigureAwait(false);

            return res.Data;
        }

        public async Task<RandomImageData> GetRandomGifAsync()
            => (await this.giphy.RandomGif(new RandomParameter()).ConfigureAwait(false))?.Data;

        public async Task<ImageData[]> GetTrendingGifsAsync(int amount = 1)
        {
            if (amount < 1 || amount > 20)
                throw new ArgumentException("Result amount out of range", "amount");

            var res = await this.giphy.TrendingGifs(new TrendingParameter() {
                Limit = amount
            }).ConfigureAwait(false);

            return res.Data;
        }
    }
}
