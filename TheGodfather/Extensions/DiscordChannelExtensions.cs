#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
#endregion

namespace TheGodfather.Extensions
{
    internal static class DiscordChannelExtensions
    {
        public static Task<DiscordMessage> EmbedAsync(this DiscordChannel channel, string message, DiscordEmoji icon = null, DiscordColor? color = null)
        {
            return channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                Description = $"{icon ?? ""} {message}",
                Color = color ?? DiscordColor.Green
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
                Description = $"{StaticDiscordEmoji.Question} {question} (y/n)",
                Color = DiscordColor.Yellow
            });

            if (await ctx.Client.GetInteractivity().WaitForBoolReplyAsync(channel.Id, ctx.User.Id))
                return true;

            if (reply)
                await channel.InformFailureAsync("Alright, aborting...");

            return false;
        }
    }
}
