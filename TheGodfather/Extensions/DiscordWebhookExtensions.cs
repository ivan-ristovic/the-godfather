using DSharpPlus.Entities;

namespace TheGodfather.Extensions
{
    internal static class DiscordWebhookExtensions
    {
        public static string BuildUrlString(this DiscordWebhook wh)
            => $"https://discordapp.com/api/webhooks/{ wh.ChannelId }/{ wh.Token }"; 
    }
}
