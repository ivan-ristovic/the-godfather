using System.Linq;
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
        public static Task MessageReactionAddedEventHandlerAsync(TheGodfatherBot bot, MessageReactionAddEventArgs e)
        {
            if (e.Guild is null || e.Channel is null || e.Message is null)
                return Task.CompletedTask;

            StarboardService ss = bot.Services.GetRequiredService<StarboardService>();
            if (ss.IsStarboardEnabled(e.Guild.Id, out ulong cid, out string star) && cid != e.Channel.Id && e.Emoji.GetDiscordName() == star)
                ss.RegisterModifiedMessage(e.Guild.Id, e.Channel.Id, e.Message.Id);

            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.MessageReactionRemoved)]
        public static Task MessageReactionRemovedEventHandlerAsync(TheGodfatherBot bot, MessageReactionRemoveEventArgs e)
        {
            if (e.Guild is null || e.Channel is null || e.Message is null)
                return Task.CompletedTask;

            StarboardService ss = bot.Services.GetRequiredService<StarboardService>();
            if (ss.IsStarboardEnabled(e.Guild.Id, out ulong cid, out string star) && cid != e.Channel.Id && e.Emoji.GetDiscordName() == star)
                ss.RegisterModifiedMessage(e.Guild.Id, e.Channel.Id, e.Message.Id);

            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.MessageReactionRemovedEmoji)]
        public static Task MessageReactionRemovedEmojiEventHandlerAsync(TheGodfatherBot bot, MessageReactionRemoveEmojiEventArgs e)
        {
            if (e.Guild is null || e.Channel is null || e.Message is null)
                return Task.CompletedTask;

            StarboardService ss = bot.Services.GetRequiredService<StarboardService>();
            if (ss.IsStarboardEnabled(e.Guild.Id, out ulong cid, out string star) && cid != e.Channel.Id && e.Emoji.GetDiscordName() == star)
                ss.RegisterModifiedMessage(e.Guild.Id, e.Channel.Id, e.Message.Id);

            return Task.CompletedTask;
        }
    }
}
