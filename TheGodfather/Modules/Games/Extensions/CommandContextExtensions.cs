﻿using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common;
using TheGodfather.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Games.Extensions
{
    public static class CommandContextExtensions
    {
        public static async Task<DiscordUser?> WaitForGameOpponentAsync(this CommandContext ctx)
        {
            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();

            await ctx.ImpInfoAsync(DiscordColor.Orange, Emojis.Question, "q-game", ctx.User.Mention);
            string acceptStr = lcs.GetString(ctx.Guild?.Id, "str-challenge");

            InteractivityResult<DiscordMessage> mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(
                xm => {
                    if (xm.Author.IsBot || xm.Author == ctx.User || xm.Channel != ctx.Channel)
                        return false;
                    return xm.Content.Equals(acceptStr, StringComparison.InvariantCultureIgnoreCase);
                }
            );

            return mctx.TimedOut ? null : mctx.Result.Author;
        }
    }
}
