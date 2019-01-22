#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

using TheGodfather.Common.Converters;
using TheGodfather.Exceptions;
#endregion

namespace TheGodfather.Extensions
{
    internal static class InteractivityExtensions
    {
        public static Task<bool> WaitForBoolReplyAsync(this InteractivityExtension interactivity, CommandContext ctx, ulong uid = 0)
            => interactivity.WaitForBoolReplyAsync(ctx.Channel.Id, uid != 0 ? uid : ctx.User.Id, ctx.Services.GetService<SharedData>());

        public static async Task<bool> WaitForBoolReplyAsync(this InteractivityExtension interactivity, ulong cid, ulong uid, SharedData shared = null)
        {
            if (!(shared is null))
                shared.AddPendingResponse(cid, uid);

            bool response = false;
            MessageContext mctx = await interactivity.WaitForMessageAsync(
                m => {
                    if (m.ChannelId != cid || m.Author.Id != uid)
                        return false;
                    bool? b = CustomBoolConverter.TryConvert(m.Content);
                    response = b ?? false;
                    return b.HasValue;
                }
            );

            if (!(shared is null) && !shared.TryRemovePendingResponse(cid, uid))
                throw new ConcurrentOperationException("Failed to remove user from waiting list. This is bad!");

            return response;
        }

        public static async Task<MessageContext> WaitForDmReplyAsync(this InteractivityExtension interactivity, DiscordDmChannel dm, ulong cid, ulong uid, SharedData shared = null)
        {
            if (!(shared is null))
                shared.AddPendingResponse(cid, uid);
            
            MessageContext mctx = await interactivity.WaitForMessageAsync(xm => xm.Channel == dm && xm.Author.Id == uid, TimeSpan.FromMinutes(1));

            if (!(shared is null) && !shared.TryRemovePendingResponse(cid, uid))
                throw new ConcurrentOperationException("Failed to remove user from waiting list. This is bad!");

            return mctx;
        }
    }
}
