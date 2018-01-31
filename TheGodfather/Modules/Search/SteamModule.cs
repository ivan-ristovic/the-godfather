#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("steam")]
    [Description("Youtube search commands.")]
    [Aliases("s", "st")]
    [Cooldown(2, 5, CooldownBucketType.User), Cooldown(4, 5, CooldownBucketType.Channel)]
    [ListeningCheckAttribute]
    public class SteamModule : BaseCommandModule
    {
        #region COMMAND_STEAM_PROFILE
        [Command("profile")]
        [Description("Get Steam user information from ID.")]
        [Aliases("id")]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description("ID.")] ulong id)
        {
            DiscordEmbed em = null;
            try {
                em = await ctx.Services.GetService<SteamService>().GetEmbeddedResultAsync(id)
                    .ConfigureAwait(false);
            } catch (Exception e) {
                throw new CommandFailedException("Error getting Steam information.", e);
            }

            if (em == null)
                throw new CommandFailedException("User not found!");

            await ctx.RespondAsync(embed: em)
                .ConfigureAwait(false);
        }
        #endregion
    }
}