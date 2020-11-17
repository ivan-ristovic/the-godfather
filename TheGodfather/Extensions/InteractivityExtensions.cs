using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common.Converters;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Common;
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

        public static async Task<DiscordChannel?> WaitForChannelMentionAsync(this InteractivityExtension interactivity, DiscordChannel channel, DiscordUser user)
        {
            InteractivityResult<DiscordMessage> mctx = await interactivity.WaitForMessageAsync(
                m => m.Channel == channel && m.Author == user && m.MentionedChannels.Count == 1
            );
            return mctx.TimedOut ? null : mctx.Result.MentionedChannels.FirstOrDefault() ?? null;
        }

        public static async Task<PunishmentAction?> WaitForPunishmentActionAsync(this InteractivityExtension interactivity, DiscordChannel channel, DiscordUser user)
        {
            var converter = new PunishmentActionConverter();
            InteractivityResult<DiscordMessage> mctx = await interactivity.WaitForMessageAsync(
                m => m.Channel == channel && m.Author == user && converter.TryConvert(m.Content, out _)
            );
            
            if (!mctx.TimedOut) {
                new PunishmentActionConverter().TryConvert(mctx.Result.Content, out PunishmentAction action);
                return action;
            }

            return null;
        }

        public static Task<InteractivityResult<DiscordMessage>> GetNextMessageAsync(this DiscordChannel channel, DiscordUser user,
                                                                                    Func<DiscordMessage, bool> predicate) 
            => channel.GetNextMessageAsync(m => m.Author == user && predicate(m));
    }
}
