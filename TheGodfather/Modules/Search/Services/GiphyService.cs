using GiphyDotNet.Manager;
using GiphyDotNet.Model.Parameters;
using ImageData = GiphyDotNet.Model.GiphyImage.Data;
using RandomImageData = GiphyDotNet.Model.GiphyRandomImage.Data;
using RandomImageResult = GiphyDotNet.Model.Results.GiphyRandomResult;
using SearchResult = GiphyDotNet.Model.Results.GiphySearchResult;

namespace TheGodfather.Modules.Search.Services;

public sealed class GiphyService : ITheGodfatherService
{
    public bool IsDisabled => this.giphy is null;

    private readonly Giphy? giphy;


    public GiphyService(BotConfigService cfg)
    {
        if (!string.IsNullOrWhiteSpace(cfg.CurrentConfiguration.GiphyKey))
            this.giphy = new Giphy(cfg.CurrentConfiguration.GiphyKey);
    }


    public async Task<ImageData[]?> SearchGifAsync(string query, int amount = 1)
    {
        if (this.IsDisabled)
            return null;

        if (amount is < 1 or > 20)
            amount = 1;

        SearchResult res = await this.giphy!.GifSearch(new SearchParameter {
            Query = query,
            Limit = amount
        }).ConfigureAwait(false);

        return res.Data;
    }

    public async Task<RandomImageData?> GetRandomGifAsync()
    {
        if (this.IsDisabled)
            return null;

        RandomImageResult res = await this.giphy!.RandomGif(new RandomParameter()).ConfigureAwait(false);
        return res?.Data;
    }

    public async Task<ImageData[]?> GetTrendingGifsAsync(int amount = 1)
    {
        if (this.IsDisabled)
            return null;

        if (amount is < 1 or > 20)
            amount = 1;

        SearchResult res = await this.giphy!.TrendingGifs(new TrendingParameter {
            Limit = amount
        }).ConfigureAwait(false);

        return res.Data;
    }

    public async Task<ImageData[]?> SearchStickerAsync(string query, int amount = 1)
    {
        if (this.IsDisabled)
            return null;

        if (amount is < 1 or > 20)
            amount = 1;

        SearchResult res = await this.giphy!.StickerSearch(new SearchParameter {
            Query = query,
            Limit = amount
        }).ConfigureAwait(false);

        return res.Data;
    }

    public async Task<RandomImageData?> GetRandomStickerAsync()
    {
        if (this.IsDisabled)
            return null;

        RandomImageResult res = await this.giphy!.RandomSticker(new RandomParameter()).ConfigureAwait(false);
        return res?.Data;
    }

    public async Task<ImageData[]?> GetTrendingStickerssAsync(int amount = 1)
    {
        if (this.IsDisabled)
            return null;

        if (amount is < 1 or > 20)
            amount = 1;

        SearchResult res = await this.giphy!.TrendingStickers(new TrendingParameter {
            Limit = amount
        }).ConfigureAwait(false);

        return res.Data;
    }
}