using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Exceptions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Polls.Extensions
{
    internal static class CommandContextExtensions
    {
        public static async Task<List<string>?> WaitAndParsePollOptionsAsync(this CommandContext ctx, string separator = ";")
        {
            InteractivityService interactivity = ctx.Services.GetRequiredService<InteractivityService>();
            interactivity.AddPendingResponse(ctx.Channel.Id, ctx.User.Id);

            InteractivityResult<DiscordMessage> mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(
                xm => xm.Author == ctx.User && xm.Channel == ctx.Channel
            );

            if (!interactivity.RemovePendingResponse(ctx.Channel.Id, ctx.User.Id))
                throw new ConcurrentOperationException("Failed to remove user from pending list");

            if (mctx.TimedOut)
                return null;

            return mctx.Result.Content.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Distinct()
                .ToList();
        }
    }
}
