#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;

using SteamWebAPI2;
using SteamWebAPI2.Models;
using SteamWebAPI2.Utilities;
using SteamWebAPI2.Exceptions;
using SteamWebAPI2.Interfaces;
using Steam.Models.SteamCommunity;
using Steam.Models.SteamStore;
using Steam.Models.SteamPlayer;
#endregion

namespace TheGodfather.Services
{
    public class SteamService
    {
        private SteamUser _steam { get; set; }


        public SteamService(string key)
        {
            _steam = new SteamUser(key);
        }


        public async Task<DiscordEmbed> GetEmbeddedResultAsync(ulong id)
        {
            var model = await _steam.GetCommunityProfileAsync(id)
                .ConfigureAwait(false);
            if (model == null)
                return null;

            var summary = await _steam.GetPlayerSummaryAsync(id)
                .ConfigureAwait(false);

            return EmbedSteamResult(model, summary.Data);
        }

        public DiscordEmbed EmbedSteamResult(SteamCommunityProfileModel model, PlayerSummaryModel summary)
        {
            var em = new DiscordEmbedBuilder() {
                Title = summary.Nickname,
                Description = Regex.Replace(model.Summary, "<[^>]*>", ""),
                ThumbnailUrl = model.AvatarFull.ToString(),
                Color = DiscordColor.Black,
                Url = $"http://steamcommunity.com/id/{model.SteamID}/"
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

            if (summary.PlayingGameName != null)
                em.AddField("Playing: ", $"{summary.PlayingGameName}");

            //em.AddField("Game activity", $"{model.HoursPlayedLastTwoWeeks.ToString()} hours past 2 weeks.", inline: true);

            if (model.IsVacBanned) {
                var bans = _steam.GetPlayerBansAsync(model.SteamID).Result.Data;

                uint bancount = 0;
                foreach (var b in bans)
                    bancount += b.NumberOfVACBans;

                em.AddField("VAC Status:", $"**{bancount}** ban(s) on record.", inline: true);
            }

            return em.Build();
        }
    }
}
