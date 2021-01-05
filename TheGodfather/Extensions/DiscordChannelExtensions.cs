using System;
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
            => LocalizedEmbedAsync(channel, lcs, null, null, key, args);

        public static Task<DiscordMessage> LocalizedEmbedAsync(this DiscordChannel channel, LocalizationService lcs, DiscordEmoji? icon, DiscordColor? color, 
                                                               string key, params object?[]? args)
        {
            return channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                Description = $"{icon ?? ""} {lcs.GetString(channel.GuildId, key, args)}",
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

        public static async Task<DiscordMessage?> GetLastMessageAsync(this DiscordChannel channel)
        {
            IReadOnlyList<DiscordMessage> m = await channel.GetMessagesBeforeAsync(channel.LastMessageId, 1);
            return m.FirstOrDefault();
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

        public static bool IsNsfwOrNsfwName(this DiscordChannel channel) 
            => channel.IsNSFW || channel.Name.StartsWith("nsfw", StringComparison.InvariantCultureIgnoreCase);
    }
}
