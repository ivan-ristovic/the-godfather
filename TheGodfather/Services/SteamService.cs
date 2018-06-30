#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using DSharpPlus;
using DSharpPlus.Entities;

using SteamWebAPI2.Utilities;
using SteamWebAPI2.Interfaces;
using Steam.Models.SteamCommunity;
#endregion

namespace TheGodfather.Services
{
    public class SteamService : IGodfatherService
    {
        private SteamUser _steam { get; set; }


        public SteamService(string key)
        {
            _steam = new SteamUser(key);
        }


        public static string ProfileUrlForId(ulong id)
            => $"http://steamcommunity.com/id/{ id }/";


        public async Task<DiscordEmbed> GetEmbeddedResultAsync(ulong id)
        {
            SteamCommunityProfileModel profile = null;
            ISteamWebResponse<PlayerSummaryModel> summary = null;
            try {
                profile = await _steam.GetCommunityProfileAsync(id)
                    .ConfigureAwait(false);
                summary = await _steam.GetPlayerSummaryAsync(id)
                    .ConfigureAwait(false);
            } catch (Exception e) {
                TheGodfather.LogProvider.LogException(LogLevel.Debug, e);
            }

            if (profile == null || summary == null || summary.Data == null)
                return null;

            return EmbedSteamResult(profile, summary.Data);
        }

        public DiscordEmbed EmbedSteamResult(SteamCommunityProfileModel model, PlayerSummaryModel summary)
        {
            var em = new DiscordEmbedBuilder() {
                Title = summary.Nickname,
                Description = Regex.Replace(model.Summary, "<[^>]*>", ""),
                ThumbnailUrl = model.AvatarFull.ToString(),
                Color = DiscordColor.Black,
                Url = ProfileUrlForId(model.SteamID)
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

            //em.AddField("Game activity", $"{model.HoursPlayedLastTwoWeeks} hours past 2 weeks.", inline: true);

            if (model.IsVacBanned) {
                var bans = _steam.GetPlayerBansAsync(model.SteamID).Result.Data;

                uint bancount = 0;
                foreach (var b in bans)
                    bancount += b.NumberOfVACBans;

                em.AddField("VAC Status:", $"{Formatter.Bold(bancount.ToString())} ban(s) on record.", inline: true);
            } else {
                em.AddField("VAC Status:", "No bans registered");
            }

            return em.Build();
        }
    }
}
