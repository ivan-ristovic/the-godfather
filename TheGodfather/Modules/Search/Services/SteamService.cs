using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Steam.Models;
using Steam.Models.SteamCommunity;
using Steam.Models.SteamStore;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using TheGodfather.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Search.Services
{
    public sealed class SteamService : ITheGodfatherService, IDisposable
    {
        private const string SteamCommunityURL = "http://steamcommunity.com";
        private const string SteamStoreURL = "https://store.steampowered.com/";
        private static readonly Regex _tagRegex = new Regex(@"<[^>]*>", RegexOptions.Compiled);

        private static void ResetCache(object? sender)
        {
            if (sender is SteamService service) {
                lock (service._lock)
                    service.appCache = null;
            }
        }

        public bool IsDisabled => this.users is null || this.apps is null || this.store is null;

        private readonly SteamUser? users;
        private readonly SteamApps? apps;
        private readonly SteamStore? store;
        private readonly Timer cacheResetTimer;
        private readonly object _lock = new object();
        private ImmutableDictionary<uint, string>? appCache;


        public SteamService(BotConfigService cfg)
        {
            this.cacheResetTimer = new Timer(ResetCache, this, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(15));
            if (!string.IsNullOrWhiteSpace(cfg.CurrentConfiguration.SteamKey)) {
                var webInterfaceFactory = new SteamWebInterfaceFactory(cfg.CurrentConfiguration.SteamKey);
                this.users = webInterfaceFactory.CreateSteamWebInterface<SteamUser>();
                this.apps = webInterfaceFactory.CreateSteamWebInterface<SteamApps>();
                this.store = webInterfaceFactory.CreateSteamStoreInterface();
            }
        }

        public void Dispose()
        {
            this.cacheResetTimer.Dispose();
        }


        public string GetCommunityProfileUrl(ulong id)
            => $"{SteamCommunityURL}/id/{id}";

        public string GetCommunityProfileUrl(string username)
            => $"{SteamCommunityURL}/id/{WebUtility.UrlEncode(username)}";

        public string GetGameStoreUrl(uint id)
            => $"{SteamStoreURL}/app/{id}";

        public async Task<(SteamCommunityProfileModel, PlayerSummaryModel)?> GetInfoAsync(string vanityUrl)
        {
            if (this.IsDisabled)
                return null;

            try {
                ISteamWebResponse<ulong> res = await this.users!.ResolveVanityUrlAsync(vanityUrl);
                return await this.GetInfoAsync(res.Data);
            } catch {
                return null;
            }
        }

        public async Task<(SteamCommunityProfileModel, PlayerSummaryModel)?> GetInfoAsync(ulong id)
        {
            if (this.IsDisabled)
                return null;

            SteamCommunityProfileModel? profile;
            ISteamWebResponse<PlayerSummaryModel>? summary;
            try {
                profile = await this.users!.GetCommunityProfileAsync(id);
                summary = await this.users!.GetPlayerSummaryAsync(id);
            } catch {
                return null;
            }

            if (profile is null || summary is null || summary.Data is null)
                return null;

            profile.Summary = _tagRegex.Replace(profile.Summary, string.Empty);
            return (profile, summary.Data);
        }

        public async Task<int?> GetVacBanCountAsync(ulong id)
        {
            if (this.IsDisabled)
                return null;

            try {
                ISteamWebResponse<IReadOnlyCollection<PlayerBansModel>> bans = await this.users!.GetPlayerBansAsync(id);
                return bans.Data.Sum(b => (int)b.NumberOfVACBans);
            } catch {
                return null;
            }
        }

        public async Task<uint?> GetAppIdAsync(string name)
        {
            if (this.IsDisabled)
                return null;
            
            name = name.ToLowerInvariant();
            if (this.appCache is null) {
                try {
                    ISteamWebResponse<IReadOnlyCollection<SteamAppModel>>? apps = await this.apps!.GetAppListAsync();
                    lock (this._lock)
                        this.appCache = apps.Data.ToDictionary(a => a.AppId, a => a.Name.ToLowerInvariant()).ToImmutableDictionary();
                } catch {
                    return null;
                }
            }

            KeyValuePair<uint, string> bestMatch;
            lock (this._lock)
                bestMatch = this.appCache.MinBy(kvp => name.LevenshteinDistanceTo(kvp.Value));
            return name.LevenshteinDistanceTo(bestMatch.Value) <= 5 ? bestMatch.Key : (uint?)null;
        }

        public async Task<StoreAppDetailsDataModel?> GetStoreInfoAsync(uint id)
        {
            if (this.IsDisabled)
                return null;

            try {
                return await this.store!.GetStoreAppDetailsAsync(id);
            } catch {
                return null;
            }
        }
    }
}
