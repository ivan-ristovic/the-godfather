using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common.Converters;
using TheGodfather.Exceptions;
using TheGodfather.Services;

namespace TheGodfather.Extensions
{
    internal static class InteractivityExtensions
    {
        public static async Task<bool> WaitForBoolReplyAsync(this InteractivityExtension interactivity, CommandContext ctx)
        {
            InteractivityService ins = ctx.Services.GetRequiredService<InteractivityService>();

            ins.AddPendingResponse(ctx.Channel.Id, ctx.User.Id);

            bool response = await WaitForBoolReplyAsync(interactivity, ctx.Channel, ctx.User);

            if (!ins.RemovePendingResponse(ctx.Channel.Id, ctx.User.Id))
                throw new ConcurrentOperationException(ctx, "err-concurrent-usr-rem");

            return response;
        }

        public static async Task<bool> WaitForBoolReplyAsync(this InteractivityExtension interactivity, DiscordChannel channel, DiscordUser user)
        {
            var conv = new BoolConverter();

            bool response = false;
            InteractivityResult<DiscordMessage> mctx = await interactivity.WaitForMessageAsync(
                m => m.Channel == channel && m.Author == user && conv.TryConvert(m.Content, out response)
            );
            return !mctx.TimedOut && response;
        }
    }
}
