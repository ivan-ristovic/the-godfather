using System.IO;
using System.Net;
using System.Threading;
using System.Xml.Serialization;
using TheGodfather.Modules.Search.Common;

namespace TheGodfather.Modules.Search.Services;

public sealed class GoodreadsService : TheGodfatherHttpService
{
    private const string Endpoint = "https://www.goodreads.com/search/index.xml";

    private static readonly XmlSerializer _serializer = new(typeof(GoodreadsResponse));
    private static readonly SemaphoreSlim _requestSemaphore = new(1, 1);


    public override bool IsDisabled => string.IsNullOrWhiteSpace(this.key);

    private readonly string? key;


    public GoodreadsService(BotConfigService cfg)
    {
        this.key = cfg.CurrentConfiguration.GoodreadsKey;
    }


    public async Task<GoodreadsSearchInfo?> SearchBooksAsync(string query)
    {
        if (this.IsDisabled)
            return null;

        await _requestSemaphore.WaitAsync();
        try {
            await using Stream stream = await _http.GetStreamAsync($"{Endpoint}?key={this.key}&q={WebUtility.UrlEncode(query)}").ConfigureAwait(false);
            var response = _serializer.Deserialize(stream) as GoodreadsResponse;
            return response?.SearchInfo;
        } catch {
            return null;
        } finally {
            await Task.Delay(TimeSpan.FromSeconds(1));
            _requestSemaphore.Release();
        }
    }
}