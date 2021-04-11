using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Serilog;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Exceptions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Search.Services
{
    public sealed class CryptoCurrencyService : TheGodfatherHttpService
    {
        private const string Endpoint = "https://pro-api.coinmarketcap.com/v1/cryptocurrency/listings/latest";
        private const string SlugUrl = "https://coinmarketcap.com/currencies/{0}";
        private const string CoinUrl = "https://s3.coinmarketcap.com/static/img/coins/128x128/{0}.png";
        private const string GraphUrl = "https://s3.coinmarketcap.com/generated/sparklines/web/7d/usd/{0}.png";
        private const int CurrencyPoolSize = 500;
        private const int CacheExpirationTimeSeconds = 3600;

        public override bool IsDisabled => string.IsNullOrWhiteSpace(this.key);

        private readonly string? key;
        private readonly IMemoryCache cache;
        private readonly SemaphoreSlim sem;
        private readonly ConcurrentDictionary<string, string> ids;
        private DateTimeOffset? lastUpdateTime;


        public CryptoCurrencyService(BotConfigService cfg)
        {
            this.key = cfg.CurrentConfiguration.CryptoKey;
            this.cache = new MemoryCache(new MemoryCacheOptions {
                ExpirationScanFrequency = TimeSpan.FromSeconds(CacheExpirationTimeSeconds),
                SizeLimit = CurrencyPoolSize,
            });
            this.sem = new SemaphoreSlim(1, 1);
            this.ids = new ConcurrentDictionary<string, string>();
        }


        public string GetSlugUrl(CryptoResponseData data)
            => string.Format(SlugUrl, WebUtility.UrlEncode(data.Slug));

        public string GetCoinUrl(CryptoResponseData data)
            => string.Format(CoinUrl, WebUtility.UrlEncode(data.Id));

        public string GetWeekGraphUrl(CryptoResponseData data)
            => string.Format(GraphUrl, WebUtility.UrlEncode(data.Id));

        public async Task<CryptoResponseData?> SearchAsync(string currency)
        {
            if (this.IsDisabled || string.IsNullOrWhiteSpace(currency))
                return null;

            currency = currency.ToLowerInvariant();

            await this.sem.WaitAsync();
            try {
                CryptoResponseData? cachedData = this.InternalConsultCache(currency);
                if (cachedData is { })
                    return cachedData;

                if (lastUpdateTime is { } && (DateTimeOffset.Now - lastUpdateTime.Value).TotalSeconds < CacheExpirationTimeSeconds)
                    return null;

                await this.InternalTryUpdateCacheAsync();
                return this.InternalConsultCache(currency);
            } catch (HttpRequestException e) {
                Log.Error(e, "Failed to fetch crypto API JSON.");
                throw new SearchServiceException<CryptoResponseStatus>(e.Message, new CryptoResponseStatus {
                    ErrorCode = e.StatusCode is { } ? (int)e.StatusCode : 400,
                    ErrorMessage = e.Message,
                });
            } catch (JsonSerializationException e) {
                Log.Error(e, "Failed to deserialize crypto API JSON.");
            } catch (SearchServiceException<CryptoResponseStatus> e) {
                Log.Error(e, "Failed to retrieve crypto API data.");
                throw;
            } finally {
                this.sem.Release();
            }

            return null;
        }


        private async Task InternalTryUpdateCacheAsync()
        {
            string url = $"{Endpoint}?CMC_PRO_API_KEY={key}&start=1&limit={CurrencyPoolSize}&convert=USD";

            string? json = await _http.GetStringAsync(url);
            CryptoResponse? response = JsonConvert.DeserializeObject<CryptoResponse>(json);
            if (response is null)
                throw new JsonSerializationException();

            if (!response.IsSuccess)
                throw new SearchServiceException<CryptoResponseStatus>(response.Status.ErrorMessage, response.Status);

            foreach (CryptoResponseData currency in response.Data) {
                this.ids.AddOrUpdate(currency.Name, currency.Id, (_, _) => currency.Id);
                var cacheEntryOptions = new MemoryCacheEntryOptions {
                    Size = 1,
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheExpirationTimeSeconds)
                };
                cache.Set(currency.Id, currency, cacheEntryOptions);
            }

            this.lastUpdateTime = DateTimeOffset.Now;
        }

        private CryptoResponseData? InternalConsultCache(string currency)
        {
            if (!this.ids.Any())
                return null;

            string key = this.ids.Keys.MinBy(k => k.LevenshteinDistanceTo(currency));
            return key.LevenshteinDistanceTo(currency) < 3
                ? cache.Get<CryptoResponseData>(this.ids[key])
                : null;
        }
    }
}
