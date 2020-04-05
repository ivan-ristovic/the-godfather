#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;

using Steam.Models.SteamCommunity;

using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Search.Services
{
    public class SteamService : ITheGodfatherService
    {
        public bool IsDisabled => this.user is null;

        private readonly SteamUser user;


        public static string GetProfileUrlForId(ulong id)
            => $"http://steamcommunity.com/id/{ id }/";


        public SteamService(BotConfigService cfg)
        {
            // FIXME
            //if (!string.IsNullOrWhiteSpace(cfg.CurrentConfiguration.SteamKey))
            //    this.user = new SteamUser(cfg.CurrentConfiguration.SteamKey);
        }


        public DiscordEmbed EmbedSteamResult(SteamCommunityProfileModel model, PlayerSummaryModel summary)
        {
            if (this.IsDisabled)
                return null;

            var em = new DiscordEmbedBuilder {
                Title = summary.Nickname,
                Description = Regex.Replace(model.Summary, "<[^>]*>", string.Empty),
                ThumbnailUrl = model.AvatarFull.ToString(),
                Color = DiscordColor.Black,
                Url = GetProfileUrlForId(model.SteamID)
            };

            if (summary.ProfileVisibility != ProfileVisibility.Public) {
                em.Description = "This profile is private.";
                return em;
            }

            em.AddField("Member since", summary.AccountCreatedDate.ToUniversalTime().ToString(), inline: true);

            if (summary.UserStatus != Steam.Models.SteamCommunity.UserStatus.Offline)
                em.AddField("Status:", summary.UserStatus.ToString(), inline: true);
            else
                em.AddField("Last seen:", summary.LastLoggedOffDate.ToUniversalTime().ToString(), inline: true);

            if (!string.IsNullOrWhiteSpace(summary.PlayingGameName))
                em.AddField("Playing: ", summary.PlayingGameName);

            if (!string.IsNullOrWhiteSpace(model.Location))
                em.AddField("Location: ", model.Location);

            // em.AddField("Game activity", $"{model.HoursPlayedLastTwoWeeks} hours past 2 weeks.", inline: true);

            if (model.IsVacBanned) {
                System.Collections.Generic.IReadOnlyCollection<PlayerBansModel> bans = this.user.GetPlayerBansAsync(model.SteamID).Result.Data;

                uint bancount = 0;
                foreach (PlayerBansModel b in bans)
                    bancount += b.NumberOfVACBans;

                em.AddField("VAC Status:", $"{Formatter.Bold(bancount.ToString())} ban(s) on record.", inline: true);
            } else {
                em.AddField("VAC Status:", "No bans registered");
            }

            return em.Build();
        }

        public async Task<DiscordEmbed> GetEmbeddedInfoAsync(ulong id)
        {
            if (this.IsDisabled)
                return null;

            SteamCommunityProfileModel profile = null;
            ISteamWebResponse<PlayerSummaryModel> summary = null;

            try {
                profile = await this.user.GetCommunityProfileAsync(id);
                summary = await this.user.GetPlayerSummaryAsync(id);
            } catch {

            }

            if (profile is null || summary is null || summary.Data is null)
                return null;

            return this.EmbedSteamResult(profile, summary.Data);
        }
    }
}
