#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Common.Converters;
#endregion

namespace TheGodfather.Extensions
{
    public static class DiscordChannelExtensions
    {
        public static Task<DiscordDmChannel> CreateDmChannelAsync(this DiscordClient client, ulong uid)
        {
            DiscordMember member = client.Guilds.Values
                .SelectMany(e => e.Members)
                .FirstOrDefault(e => e.Id == uid);

            return member?.CreateDmChannelAsync();
        }

        public static Task<DiscordMessage> InformSuccessAsync(this DiscordChannel channel, string message, DiscordEmoji icon = null)
        {
            return channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                Description = $"{icon ?? ""} {message}",
                Color = DiscordColor.Green
            });
        }

        public static Task<DiscordMessage> InformFailureAsync(this DiscordChannel channel, string message)
        {
            return channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                Description = $"{StaticDiscordEmoji.BoardPieceX} {message}",
                Color = DiscordColor.IndianRed
            });
        }

        public static async Task<bool> WaitForBoolResponseAsync(this DiscordChannel channel, CommandContext ctx, string question, bool reply = true)
        {
            await channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                Description = $"{StaticDiscordEmoji.Question} {question}",
                Color = DiscordColor.Yellow
            });

            if (await ctx.Client.GetInteractivity().WaitForBoolReplyAsync(ctx))
                return true;

            if (reply)
                await channel.InformFailureAsync("Alright, aborting...");

            return false;
        }
    }
}
