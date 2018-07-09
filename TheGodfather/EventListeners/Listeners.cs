#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using System;
#endregion

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        private static readonly string _unknown = Formatter.Italic("Unknown");

        private static string ToUTC(DateTime datetime)
            => $"At {datetime.ToUniversalTime().ToString()} UTC";

        private static string FormatEventTime(DateTimeOffset datetime)
            => $"At {datetime.ToUniversalTime().ToString()} UTC";

        private static DiscordEmbedBuilder FormEmbedBuilder(EventOrigin origin, string title, string desc = null)
        {
            var emb = new DiscordEmbedBuilder() {
                Title = title
            };

            if (desc != null)
                emb.WithDescription(desc);

            switch (origin) {
                case EventOrigin.Channel:
                    emb.WithColor(DiscordColor.Aquamarine);
                    break;
                case EventOrigin.Emoji:
                    emb.WithColor(DiscordColor.Gold);
                    break;
                case EventOrigin.Guild:
                    emb.WithColor(DiscordColor.DarkGreen);
                    break;
                case EventOrigin.KickOrBan:
                    emb.WithColor(DiscordColor.DarkRed);
                    break;
                case EventOrigin.Linkfilter:
                    emb.WithColor(DiscordColor.Red);
                    break;
                case EventOrigin.Member:
                    emb.WithColor(DiscordColor.White);
                    break;
                case EventOrigin.Message:
                    emb.WithColor(DiscordColor.SpringGreen);
                    break;
                case EventOrigin.Role:
                    emb.WithColor(DiscordColor.Magenta);
                    break;
                default:
                    emb.WithColor(DiscordColor.Black);
                    break;
            }

            return emb;
        }
    }


    internal enum EventOrigin
    {
        Channel,
        Client,
        Command,
        Emoji,
        Guild,
        KickOrBan,
        Linkfilter,
        Member,
        Message,
        Role
    }
}
