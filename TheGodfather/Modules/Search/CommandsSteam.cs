#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfatherBot.Exceptions;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using SteamSharp;
#endregion


namespace TheGodfatherBot.Modules.Search
{
    [Group("steam", CanInvokeWithoutSubcommand = false)]
    [Description("Youtube search commands.")]
    [Aliases("s", "st")]
    public class CommandsSteam
    {
        #region PRIVATE_FIELDS
        private SteamClient _steam = new SteamClient() {
            Authenticator = SteamSharp.Authenticators.APIKeyAuthenticator.ForProtectedResource(TheGodfather.GetToken("Resources/steam.txt"))
        };
        #endregion


        #region COMMAND_STEAM_PROFILE
        [Command("profile")]
        [Description("Get Steam user information from ID.")]
        [Aliases("id")]
        public async Task SteamProfile(CommandContext ctx,
                                      [Description("ID.")] string id = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new InvalidCommandUsageException("ID missing.");

            SteamUser user = null;
            try {
                user = await SteamCommunity.GetUserAsync(_steam, new SteamID(id));
            } catch (FormatException e) {
                throw new CommandFailedException("Invalid ID format.", e);
            }

            if (user == null) {
                await ctx.RespondAsync("No users found.");
                return;
            }

            await ctx.RespondAsync(user.PlayerInfo.ProfileURL, embed: EmbedSteamResult(user.PlayerInfo));
        }
        #endregion


        #region HELPER_FUNCTIONS
        private DiscordEmbed EmbedSteamResult(SteamCommunity.PlayerInfo info)
        {
            var em = new DiscordEmbedBuilder() {
                Title = info.PersonaName,
                //Description = Regex.Replace(info., "<[^>]*>", ""),
                ThumbnailUrl = info.AvatarMediumURL,
                Color = DiscordColor.Black
            };

            if (info.CommunityVisibilityState != CommunityVisibilityState.Public) {
                em.Description = "This profile is private.";
                return em;
            }

            if (info.PersonaState != PersonaState.Offline)
                em.AddField("Status:", info.PersonaState.ToString(), inline: true);
            else
                em.AddField("Last seen:", info.LastLogOff.ToUniversalTime().ToString(), inline: true);

            if (!string.IsNullOrWhiteSpace(info.GameID))
                em.AddField("Playing: ", $"{info.GameExtraInfo}");

            //em.AddField("Game activity", $"{model.HoursPlayedLastTwoWeeks} hours past 2 weeks.", inline: true);
            em.AddField("Member since", info.DateTimeCreated.ToUniversalTime().ToString(), inline: true);
            /*
            if (model.IsVacBanned) {
                var bans = _steam.GetPlayerBansAsync(model.SteamID).Result.Data;

                uint bancount = 0;
                foreach (var b in bans)
                    bancount += b.NumberOfVACBans;

                em.AddField("VAC Status:", $"**{bancount}** ban(s) on record.", inline: true);
            }*/

            return em;
        }
        #endregion
    }
}