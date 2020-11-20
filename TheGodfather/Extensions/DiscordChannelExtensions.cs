using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using TheGodfather.Common;
using TheGodfather.Services;

namespace TheGodfather.Extensions
{
    internal static class DiscordChannelExtensions
    {
        public static Task<DiscordMessage> EmbedAsync(this DiscordChannel channel, string message, DiscordEmoji? icon = null, DiscordColor? color = null)
        {
            return channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                Description = $"{icon ?? ""} {message}",
                Color = color ?? DiscordColor.Green
            });
        }

        public static Task<DiscordMessage> LocalizedEmbedAsync(this DiscordChannel channel, LocalizationService lcs, string key, params object?[]? args)
            => LocalizedEmbedAsync(channel, lcs, key, null, null, args);

        public static Task<DiscordMessage> LocalizedEmbedAsync(this DiscordChannel channel, LocalizationService lcs, string key, 
                                                               DiscordEmoji? icon, DiscordColor? color,
                                                               object?[]? args)
        {
            return channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                Description = $"{icon ?? ""} {lcs.GetString(channel.Guild.Id, key, args)}",
                Color = color ?? DiscordColor.Green
            });
        }

        public static Task<DiscordMessage> InformFailureAsync(this DiscordChannel channel, string message)
        {
            return channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                Description = $"{Emojis.X} {message}",
                Color = DiscordColor.IndianRed
            });
        }

        public static async Task<IReadOnlyList<DiscordMessage>> GetMessagesFromAsync(this DiscordChannel channel, DiscordMember member, int limit = 1)
        {
            var messages = new List<DiscordMessage>();

            // TODO fix
            //for (int step = 50; messages.Count < limit && step < 400; step *= 2) {
            //    ulong? lastId = messages.FirstOrDefault()?.Id;
            //    IReadOnlyList<DiscordMessage> requested = lastId is null
            //        ? await channel.GetMessagesAsync(step)
            //        : await channel.GetMessagesBeforeAsync(messages.FirstOrDefault().Id, step - messages.Count);
            //    messages.AddRange(requested.Where(m => m.Author.Id == member.Id).Take(limit));
            //}

            return messages.AsReadOnly();
        }

        public static async Task<DiscordOverwrite?> FindOverwriteForRoleAsync(this DiscordChannel channel, DiscordRole role)
        {
            IEnumerable<DiscordOverwrite> roleOverwrites = channel.PermissionOverwrites.Where(o => o.Type == OverwriteType.Role);
            foreach (DiscordOverwrite overwrite in roleOverwrites) {
                DiscordRole? r = await overwrite.GetRoleAsync();
                if (r is { } && r == role)
                    return overwrite;
            }
            return null;
        }
    }
}
