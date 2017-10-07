#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfatherBot.Exceptions;
using TheGodfatherBot.Helpers;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using SteamWebAPI2;
using SteamWebAPI2.Models;
using SteamWebAPI2.Utilities;
using SteamWebAPI2.Exceptions;
using SteamWebAPI2.Interfaces;
using Steam.Models.SteamCommunity;
using Steam.Models.SteamStore;
using Steam.Models.SteamPlayer;
#endregion


namespace TheGodfatherBot.Commands.Search
{
    [Group("steam", CanInvokeWithoutSubcommand = false)]
    [Description("Youtube search commands.")]
    [Aliases("s", "st")]
    [Cooldown(2, 5, CooldownBucketType.User), Cooldown(4, 5, CooldownBucketType.Channel)]
    public class CommandsSteam
    {
        #region PRIVATE_FIELDS
        private SteamUser _steam = new SteamUser(TheGodfather.Config.SteamKey);
        #endregion


        #region COMMAND_STEAM_PROFILE
        [Command("profile")]
        [Description("Get Steam user information from ID.")]
        [Aliases("id")]
        public async Task SteamProfile(CommandContext ctx,
                                      [Description("ID.")] ulong id = 0)
        {
            var model = await _steam.GetCommunityProfileAsync(id);
            if (model == null)
                throw new CommandFailedException("No users found!");

            var summary = await _steam.GetPlayerSummaryAsync(id);

            await ctx.RespondAsync($"http://steamcommunity.com/id/{model.SteamID}/", embed: EmbedSteamResult(model, summary.Data));
        }
        #endregion


        #region HELPER_FUNCTIONS
        private DiscordEmbed EmbedSteamResult(SteamCommunityProfileModel model, PlayerSummaryModel summary)
        {
            var em = new DiscordEmbedBuilder() {
                Title = summary.Nickname,
                Description = Regex.Replace(model.Summary, "<[^>]*>", ""),
                ThumbnailUrl = model.AvatarFull.ToString(),
                Color = DiscordColor.Black
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

            return em;
        }
        #endregion
    }
}