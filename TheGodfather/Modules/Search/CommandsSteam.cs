#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using SteamWebAPI2.Interfaces;
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

            var result = await _steam.GetPlayerSummaryAsync(id);

            await ctx.RespondAsync(result.Data.ProfileUrl, embed: new DiscordEmbedBuilder() {
                Title = result.Data.Nickname,
                ImageUrl = result.Data.AvatarMediumUrl,
                Color = DiscordColor.Black
            });
        }
        #endregion
    }
}
