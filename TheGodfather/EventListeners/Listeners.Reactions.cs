using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.EventListeners.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Misc.Common;
using TheGodfather.Modules.Misc.Extensions;
using TheGodfather.Modules.Misc.Services;
using TheGodfather.Modules.Owner.Services;
using TheGodfather.Services;

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.MessageReactionsCleared)]
        public static Task MessageReactionsClearedEventHandlerAsync(TheGodfatherBot bot, MessageReactionsClearEventArgs e)
        {
            if (e.Guild is null || e.Channel is null || e.Message is null)
                return Task.CompletedTask;

            if (bot.Services.GetRequiredService<BlockingService>().IsChannelBlocked(e.Channel.Id))
                return Task.CompletedTask;

            if (e.Message.Author == bot.Client.CurrentUser && bot.Services.GetRequiredService<ChannelEventService>().IsEventRunningInChannel(e.Channel.Id))
                return Task.CompletedTask;

            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return Task.CompletedTask;

            if (LoggingService.IsChannelExempted(bot, e.Guild, e.Channel, out _))
                return Task.CompletedTask;

            LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();

            string jumplink = Formatter.MaskedUrl(ls.GetString(e.Guild.Id, "str-jumplink"), e.Message.JumpLink);
            emb.WithLocalizedTitle(DiscordEventType.MessageReactionsCleared, "evt-msg-reactions-clear", desc: jumplink);
            emb.AddLocalizedTitleField("str-location", e.Channel.Mention, inline: true);
            emb.AddLocalizedTitleField("str-author", e.Message.Author?.Mention, inline: true);
            return logService.LogAsync(e.Channel.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.MessageReactionAdded)]
        public static async Task MessageReactionAddedEventHandlerAsync(TheGodfatherBot bot, MessageReactionAddEventArgs e)
        {
            if (e.Guild is null || e.Channel is null || e.Message is null)
                return;

            StarboardService ss = bot.Services.GetRequiredService<StarboardService>();
            if (ss.IsStarboardEnabled(e.Guild.Id, out string? star) && e.Emoji.GetDiscordName() == star) {
                StarboardModificationResult res = await ss.UpdateStarCountAsync(e.Guild.Id, e.Channel.Id, e.Message.Id, 1);
                await UpdateStarboardAsync(bot, e.Emoji, res, e.Guild, e.Message);
            }
        }

        [AsyncEventListener(DiscordEventType.MessageReactionRemoved)]
        public static async Task MessageReactionRemovedEventHandlerAsync(TheGodfatherBot bot, MessageReactionRemoveEventArgs e)
        {
            if (e.Guild is null || e.Channel is null || e.Message is null)
                return;

            StarboardService ss = bot.Services.GetRequiredService<StarboardService>();
            if (ss.IsStarboardEnabled(e.Guild.Id, out string? star) && e.Emoji.GetDiscordName() == star) {
                StarboardModificationResult res = await ss.UpdateStarCountAsync(e.Guild.Id, e.Channel.Id, e.Message.Id, -1);
                await UpdateStarboardAsync(bot, e.Emoji, res, e.Guild, e.Message);
            }
        }

        [AsyncEventListener(DiscordEventType.MessageReactionRemovedEmoji)]
        public static async Task MessageReactionRemovedEmojiEventHandlerAsync(TheGodfatherBot bot, MessageReactionRemoveEmojiEventArgs e)
        {
            if (e.Guild is null || e.Channel is null || e.Message is null)
                return;

            StarboardService ss = bot.Services.GetRequiredService<StarboardService>();
            if (ss.IsStarboardEnabled(e.Guild.Id, out string? star) && e.Emoji.GetDiscordName() == star) {
                StarboardModificationResult res = await ss.UpdateStarCountAsync(e.Guild.Id, e.Channel.Id, e.Message.Id, 0);
                await UpdateStarboardAsync(bot, e.Emoji, res, e.Guild, e.Message);
            }
        }


        private static async Task UpdateStarboardAsync(TheGodfatherBot bot, DiscordEmoji star, StarboardModificationResult res,
                                                       DiscordGuild guild, DiscordMessage msg)
        {
            if (res.ActionType == StarboardActionType.None)
                return;

            msg = await msg.Channel.GetMessageAsync(msg.Id);

            StarboardService ss = bot.Services.GetRequiredService<StarboardService>();
            ulong starChannelId = await ss.GetStarboardChannelAsync(guild.Id);
            if (starChannelId == msg.Channel.Id)
                return;

            DiscordChannel? starChannel = null;
            DiscordMessage? starMessage = null;
            try {
                starChannel = await bot.Client.GetShard(guild.Id).GetChannelAsync(starChannelId);
                if (res.ActionType != StarboardActionType.Add)
                    starMessage = await starChannel.GetMessageAsync(res.Entry.StarMessageId);
            } catch (NotFoundException) {
                LogExt.Debug(bot.GetId(guild.Id),
                    "Failed to fetch star message {MessageId} in channel {ChannelId}, guild {GuildId}",
                    res.Entry.StarMessageId, starChannelId, guild.Id
                );
            }
            if (starChannel is { }) {
                LocalizationService lcs = bot.Services.GetRequiredService<LocalizationService>();
                try {
                    switch (res.ActionType) {
                        case StarboardActionType.Add:
                            DiscordMessage sm = await starChannel.SendMessageAsync(embed: msg.ToStarboardEmbed(lcs, star, res.Entry.Stars));
                            await ss.AddStarboardLinkAsync(guild.Id, msg.Channel.Id, msg.Id, sm.Id);
                            break;
                        case StarboardActionType.Remove:
                            if (starMessage is { })
                                await starMessage.DeleteAsync("_gf: Starboard - delete");
                            break;
                        case StarboardActionType.Update:
                            if (starMessage is null)
                                await starChannel.SendMessageAsync(embed: msg.ToStarboardEmbed(lcs, star, res.Entry.Stars));
                            else
                                await starMessage.ModifyAsync(embed: msg.ToStarboardEmbed(lcs, star, res.Entry.Stars));
                            break;
                    }
                } catch {
                    // TODO
                }
            } else {
                // TODO disable starboard and clear all starboard messages from db
            }
        }
    }
}
