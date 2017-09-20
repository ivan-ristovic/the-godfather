#region USING_DIRECTIVES
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using SteamWebAPI2.Interfaces;
using Steam.Models.SteamCommunity;
using SteamWebAPI2.Utilities;
#endregion


namespace TheGodfatherBot.Modules.Search
{
    [Group("steam", CanInvokeWithoutSubcommand = false)]
    [Description("Youtube search commands.")]
    [Aliases("s", "st")]
    public class CommandsSteam
    {
        #region PRIVATE_FIELDS
        private SteamUser _steam = new SteamUser(TheGodfather.GetToken("Resources/steam.txt"));
        #endregion


        #region COMMAND_STEAM_PROFILE
        [Command("profile")]
        [Description("Get Steam user information from ID.")]
        [Aliases("id")]
        public async Task SteamProfile(CommandContext ctx,
                                      [Description("ID.")] ulong id = 0)
        {
            if (id == 0)
                throw new ArgumentException("ID missing.");

            var result = await _steam.GetCommunityProfileAsync(id);
            if (result == null) {
                await ctx.RespondAsync("No users found.");
                return;
            }
            var summary = await _steam.GetPlayerSummaryAsync(id);

            await ctx.RespondAsync(summary.Data.ProfileUrl, embed: EmbedSteamResult(result, summary.Data));
        }
        #endregion


        #region HELPER_FUNCTIONS
        private DiscordEmbed EmbedSteamResult(SteamCommunityProfileModel model, PlayerSummaryModel summary)
        {
            var em = new DiscordEmbedBuilder() {
                Title = summary.Nickname,
                Description = Regex.Replace(model.Summary, "<[^>]*>", ""),
                ImageUrl = summary.AvatarMediumUrl,
                Color = DiscordColor.Black
            };

            if (summary.ProfileVisibility == ProfileVisibility.Private)
                em.Description = "This profile is private.";

            if (!string.IsNullOrWhiteSpace(summary.PlayingGameId))
                em.AddField("Playing: ", $"{summary.PlayingGameName} ({summary.PlayingGameId})", inline: true);

            em.AddField("Last seen:" , summary.LastLoggedOffDate.ToUniversalTime().ToString(), inline: true);
            em.AddField("Game activity", $"{model.HoursPlayedLastTwoWeeks} hours past 2 weeks.");

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
