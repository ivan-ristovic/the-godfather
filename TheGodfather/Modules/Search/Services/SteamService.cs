using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Steam.Models.SteamCommunity;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using TheGodfather.Services;

namespace TheGodfather.Modules.Search.Services
{
    public sealed class SteamService : ITheGodfatherService
    {
        private const string SteamCommunityURL = "http://steamcommunity.com";
        private static readonly Regex _tagRegex = new Regex(@"<[^>]*>", RegexOptions.Compiled);

        public bool IsDisabled => this.user is null;

        private readonly SteamUser? user;


        public SteamService(BotConfigService cfg)
        {
            if (!string.IsNullOrWhiteSpace(cfg.CurrentConfiguration.SteamKey)) {
                var webInterfaceFactory = new SteamWebInterfaceFactory(cfg.CurrentConfiguration.SteamKey);
                this.user = webInterfaceFactory.CreateSteamWebInterface<SteamUser>();
            }
        }


        public string GetCommunityProfileUrl(ulong id)
            => $"{SteamCommunityURL}/id/{id}";

        public string GetCommunityProfileUrl(string username)
            => $"{SteamCommunityURL}/id/{WebUtility.UrlEncode(username)}";

        public async Task<(SteamCommunityProfileModel, PlayerSummaryModel)?> GetInfoAsync(string vanityUrl)
        {
            if (this.IsDisabled)
                return null;

            try {
                ISteamWebResponse<ulong> res = await this.user!.ResolveVanityUrlAsync(vanityUrl);
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
                profile = await this.user!.GetCommunityProfileAsync(id);
                summary = await this.user!.GetPlayerSummaryAsync(id);
            } catch {
                return null;
            }

            if (profile is null || summary is null || summary.Data is null)
                return null;

            profile.Summary = _tagRegex.Replace(profile.Summary, string.Empty);
            return (profile, summary.Data);
        }
    }
}
