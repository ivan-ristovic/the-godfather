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
            LocalizationService ls = ctx.Services.GetRequiredService<LocalizationService>();

            ins.AddPendingResponse(ctx.Channel.Id, ctx.User.Id);

            bool response = await WaitForBoolReplyAsync(interactivity, ctx.Channel, ctx.User);

            if (!ins.RemovePendingResponse(ctx.Channel.Id, ctx.User.Id))
                throw new ConcurrentOperationException(ctx, "err-concurrent-usr-rem");

            return response;
        }

        public static async Task<bool> WaitForBoolReplyAsync(this InteractivityExtension interactivity, DiscordChannel channel, DiscordUser user)
        {
            bool response = false;
            InteractivityResult<DiscordMessage> mctx = await interactivity.WaitForMessageAsync(
                m => m.Channel == channel && m.Author == user && new BoolConverter().TryConvert(m.Content, out _)
            );
            return response;
        }
    }
}
