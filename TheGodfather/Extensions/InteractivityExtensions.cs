#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

using TheGodfather.Common.Converters;
using TheGodfather.Exceptions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Extensions
{
    internal static class InteractivityExtensions
    {
        public static Task<bool> WaitForBoolReplyAsync(this InteractivityExtension interactivity, CommandContext ctx, ulong uid = 0)
            => interactivity.WaitForBoolReplyAsync(ctx.Channel.Id, uid != 0 ? uid : ctx.User.Id, ctx.Services.GetService<InteractivityService>());

        public static async Task<bool> WaitForBoolReplyAsync(this InteractivityExtension interactivity,
                                                             ulong cid,
                                                             ulong uid,
                                                             InteractivityService interactivityService = null)
        {
            if (!(interactivityService is null))
                interactivityService.AddPendingResponse(cid, uid);

            bool response = false;
            InteractivityResult<DiscordMessage> mctx = await interactivity.WaitForMessageAsync(
                m => {
                    if (m.ChannelId != cid || m.Author.Id != uid)
                        return false;
                    bool? b = CustomBoolConverter.TryConvert(m.Content);
                    response = b ?? false;
                    return b.HasValue;
                }
            );

            if (!(interactivityService is null) && !interactivityService.RemovePendingResponse(cid, uid))
                throw new ConcurrentOperationException("Failed to remove user from waiting list. This is bad!");

            return response;
        }

        public static async Task<InteractivityResult<DiscordMessage>> WaitForDmReplyAsync(this InteractivityExtension interactivity,
                                                                                          DiscordDmChannel dm,
                                                                                          ulong cid,
                                                                                          ulong uid,
                                                                                          InteractivityService interactivityService = null)
        {
            if (!(interactivityService is null))
                interactivityService.AddPendingResponse(cid, uid);
            
            InteractivityResult<DiscordMessage> mctx = await interactivity.WaitForMessageAsync(xm => xm.Channel == dm && xm.Author.Id == uid, TimeSpan.FromMinutes(1));

            if (!(interactivityService is null) && !interactivityService.RemovePendingResponse(cid, uid))
                throw new ConcurrentOperationException("Failed to remove user from waiting list. This is bad!");

            return mctx;
        }
    }
}
