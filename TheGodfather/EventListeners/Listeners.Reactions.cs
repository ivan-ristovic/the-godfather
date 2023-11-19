﻿using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.EventListeners.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Misc.Services;
using TheGodfather.Modules.Owner.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.EventListeners;

internal static partial class Listeners
{
    [AsyncEventListener(DiscordEventType.MessageReactionsCleared)]
    public static Task MessageReactionsClearedEventHandlerAsync(TheGodfatherBot bot, MessageReactionsClearEventArgs e)
    {
        if (e.Guild is null || e.Channel is null || e.Message is null || e.Message.Author == bot.Client.CurrentUser)
            return Task.CompletedTask;

        if (bot.Services.GetRequiredService<BlockingService>().IsChannelBlocked(e.Channel.Id))
            return Task.CompletedTask;

        if (bot.Services.GetRequiredService<ChannelEventService>().IsEventRunningInChannel(e.Channel.Id))
            return Task.CompletedTask;

        if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
            return Task.CompletedTask;

        if (LoggingService.IsChannelExempted(bot, e.Guild, e.Channel, out _))
            return Task.CompletedTask;

        LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();
            
        string jumplink = Formatter.MaskedUrl(ls.GetString(e.Guild.Id, TranslationKey.str_jumplink), e.Message.JumpLink);
        emb.WithLocalizedTitle(DiscordEventType.MessageReactionsCleared, TranslationKey.evt_msg_reactions_clear, jumplink);
        emb.AddLocalizedField(TranslationKey.str_location, e.Channel.Mention, true);
        emb.AddLocalizedField(TranslationKey.str_author, e.Message.Author?.Mention, true);
        return logService.LogAsync(e.Channel.Guild, emb);
    }

    [AsyncEventListener(DiscordEventType.MessageReactionAdded)]
    public static async Task MessageReactionAddedEventHandlerAsync(TheGodfatherBot bot, MessageReactionAddEventArgs e)
    {
        if (e.Guild is null || e.Channel is null || e.Message is null || e.User.IsBot)
            return;

        StarboardService ss = bot.Services.GetRequiredService<StarboardService>();
        if (ss.IsStarboardEnabled(e.Guild.Id, out ulong cid, out string star) && cid != e.Channel.Id && e.Emoji.GetDiscordName() == star) {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Reacted with star emoji: Message {MessageId}, {Guild}", e.Message.Id, e.Guild);
            ss.RegisterModifiedMessage(e.Guild.Id, e.Channel.Id, e.Message.Id);
        }

        ReactionRoleService rrs = bot.Services.GetRequiredService<ReactionRoleService>();
        ReactionRole? rr = await rrs.GetAsync(e.Guild.Id, e.Emoji.GetDiscordName(), e.Channel.Id, e.Message.Id);
        if (rr is not null) {
            DiscordRole? role = e.Guild.GetRole(rr.RoleId);
            if (role is not null)
                try {
                    DiscordMember member = await e.Guild.GetMemberAsync(e.User.Id);
                    await member.GrantRoleAsync(role, "_gf: Reaction role");
                    LogExt.Debug(bot.GetId(e.Guild.Id), "Granted reaction {Role} to {Member} of {Guild}", role, member, e.Guild);
                } catch (Exception ex) when (ex is UnauthorizedException | ex is NotFoundException) {
                    LogExt.Debug(bot.GetId(e.Guild.Id), "Failed to grant reaction role {Role} to {Member} of {Guild}", role, e.User, e.Guild);
                }
            else
                LogExt.Debug(bot.GetId(e.Guild.Id), "Failed to find reaction role {Role} in {Guild}", rr.RoleId, e.Guild);
        }
    }

    [AsyncEventListener(DiscordEventType.MessageReactionRemoved)]
    public static async Task MessageReactionRemovedEventHandlerAsync(TheGodfatherBot bot, MessageReactionRemoveEventArgs e)
    {
        if (e.Guild is null || e.Channel is null || e.Message is null)
            return;

        StarboardService ss = bot.Services.GetRequiredService<StarboardService>();
        if (ss.IsStarboardEnabled(e.Guild.Id, out ulong cid, out string star) && cid != e.Channel.Id && e.Emoji.GetDiscordName() == star) {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Removed star emoji reaction: Message {MessageId}, {Guild}", e.Message.Id, e.Guild);
            ss.RegisterModifiedMessage(e.Guild.Id, e.Channel.Id, e.Message.Id);
        }

        ReactionRoleService rrs = bot.Services.GetRequiredService<ReactionRoleService>();
        ReactionRole? rr = await rrs.GetAsync(e.Guild.Id, e.Emoji.GetDiscordName(), e.Channel.Id, e.Message.Id);
        if (rr is not null) {
            DiscordRole? role = e.Guild.GetRole(rr.RoleId);
            if (role is not null)
                try {
                    DiscordMember member = await e.Guild.GetMemberAsync(e.User.Id);
                    await member.RevokeRoleAsync(role, "_gf: Reaction role");
                    LogExt.Debug(bot.GetId(e.Guild.Id), "Revoked reaction {Role} to {Member} of {Guild}", role, member, e.Guild);
                } catch (Exception ex) when (ex is UnauthorizedException | ex is NotFoundException) {
                    LogExt.Debug(bot.GetId(e.Guild.Id), "Failed to revoke reaction role {Role} to {Member} of {Guild}", role, e.User, e.Guild);
                }
            else
                LogExt.Debug(bot.GetId(e.Guild.Id), "Failed to find reaction role {Role} in {Guild}", rr.RoleId, e.Guild);
        }
    }

    [AsyncEventListener(DiscordEventType.MessageReactionRemovedEmoji)]
    public static Task MessageReactionRemovedEmojiEventHandlerAsync(TheGodfatherBot bot, MessageReactionRemoveEmojiEventArgs e)
    {
        if (e.Guild is null || e.Channel is null || e.Message is null)
            return Task.CompletedTask;

        StarboardService ss = bot.Services.GetRequiredService<StarboardService>();
        if (ss.IsStarboardEnabled(e.Guild.Id, out ulong cid, out string star) && cid != e.Channel.Id && e.Emoji.GetDiscordName() == star) {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Cleared star emoji reactions: Message {MessageId}, {Guild}", e.Message.Id, e.Guild);
            ss.RegisterModifiedMessage(e.Guild.Id, e.Channel.Id, e.Message.Id);
        }

        return Task.CompletedTask;
    }
}