#region USING_DIRECTIVES
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Extensions
{
    public static class DiscordWebhookExtensions
    {
        public static string BuildUrlString(this DiscordWebhook wh)
            => $"https://discordapp.com/api/webhooks/{ wh.ChannelId }/{ wh.Token }";
    }
}
