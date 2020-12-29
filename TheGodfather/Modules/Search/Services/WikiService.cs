using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Search.Services
{
    public sealed class WikiService : TheGodfatherHttpService
    {
        public const string WikipediaIconUrl = "https://en.wikipedia.org/static/images/project-logos/enwiki.png";

        private const string ApiUrl = "https://en.wikipedia.org/w/api.php?action=opensearch";
        
        private static readonly SemaphoreSlim _requestSemaphore = new SemaphoreSlim(1, 1);

        public override bool IsDisabled => false;


        public static async Task<WikiSearchResponse?> SearchAsync(string query, int amount = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
                return null;

            if (amount < 1 || amount > 20)
                amount = 10;

            string url = $"{ApiUrl}&limit={amount}&namespace=0&format=json&search={WebUtility.UrlEncode(query)}";
            string result = await _http.GetStringAsync(url).ConfigureAwait(false);

            await _requestSemaphore.WaitAsync();
            try {
                var jarr = JArray.Parse(result);
                JToken? tquery = jarr.First;
                JToken? thits = tquery?.Next;
                JToken? tsnippets = thits?.Next;
                JToken? turls = tsnippets?.Next;
                return tquery is null || thits is null || tsnippets is null || turls is null
                    ? null
                    : new WikiSearchResponse(thits.ToObject<string[]>(), tsnippets.ToObject<string[]>(), turls.ToObject<string[]>());
            } catch {
                return null;
            } finally {
                _requestSemaphore.Release();
            }
        }
    }
}
