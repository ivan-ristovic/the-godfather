using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common.Converters;

namespace TheGodfather.Extensions;

internal static class InteractivityExtensions
{
    public static async Task<int?> WaitForOptionReplyAsync(this InteractivityExtension interactivity, CommandContext ctx, int max, int min = 0)
    {
        InteractivityService ins = ctx.Services.GetRequiredService<InteractivityService>();

        ins.AddPendingResponse(ctx.Channel.Id, ctx.User.Id);

        int? response = await WaitForOptionReplyAsync(interactivity, ctx.Channel, ctx.User, max, min);

        if (!ins.RemovePendingResponse(ctx.Channel.Id, ctx.User.Id))
            throw new ConcurrentOperationException("Failed to remove user from pending list");

        return response;
    }

    public static async Task<int?> WaitForOptionReplyAsync(this InteractivityExtension interactivity, DiscordChannel channel, DiscordUser user, int max, int min = 0)
    {
        int index = 0;
        InteractivityResult<DiscordMessage> mctx = await interactivity.WaitForMessageAsync(
            m => m.Channel == channel && m.Author == user && int.TryParse(m.Content, out index) && index < max && index >= min
        );
        return mctx.TimedOut ? null : index;
    }

    public static async Task<bool> WaitForBoolReplyAsync(this InteractivityExtension interactivity, CommandContext ctx)
    {
        InteractivityService ins = ctx.Services.GetRequiredService<InteractivityService>();

        ins.AddPendingResponse(ctx.Channel.Id, ctx.User.Id);

        bool response = await WaitForBoolReplyAsync(interactivity, ctx.Channel, ctx.User);

        if (!ins.RemovePendingResponse(ctx.Channel.Id, ctx.User.Id))
            throw new ConcurrentOperationException("Failed to remove user from pending list");

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

    public static async Task<Punishment.Action?> WaitForPunishmentActionAsync(this InteractivityExtension interactivity, DiscordChannel channel, DiscordUser user)
    {
        var converter = new PunishmentActionConverter();
        InteractivityResult<DiscordMessage> mctx = await interactivity.WaitForMessageAsync(
            m => m.Channel == channel && m.Author == user && converter.TryConvert(m.Content, out _)
        );

        if (!mctx.TimedOut) {
            new PunishmentActionConverter().TryConvert(mctx.Result.Content, out Punishment.Action action);
            return action;
        }

        return null;
    }

    public static Task<InteractivityResult<DiscordMessage>> GetNextMessageAsync(this DiscordChannel channel, DiscordUser user,
        Func<DiscordMessage, bool> predicate)
        => channel.GetNextMessageAsync(m => m.Author == user && predicate(m));
}