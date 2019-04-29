#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.EventListeners
{
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

    internal static partial class Listeners
    {
        private static readonly string _unknown = Formatter.Italic("Unknown");


        public static DiscordEmbedBuilder FormEmbedBuilder(EventOrigin origin, string title, string desc = null)
        {
            var emb = new DiscordEmbedBuilder {
                Title = title
            };

            if (!(desc is null))
                emb.WithDescription(desc);

            switch (origin) {
                case EventOrigin.Channel:
                    emb.WithColor(DiscordColor.Turquoise);
                    break;
                case EventOrigin.Emoji:
                    emb.WithColor(DiscordColor.Orange);
                    break;
                case EventOrigin.Guild:
                    emb.WithColor(DiscordColor.DarkGreen);
                    break;
                case EventOrigin.KickOrBan:
                    emb.WithColor(DiscordColor.Red);
                    break;
                case EventOrigin.Linkfilter:
                    emb.WithColor(DiscordColor.DarkRed);
                    break;
                case EventOrigin.Member:
                    emb.WithColor(DiscordColor.Sienna);
                    break;
                case EventOrigin.Message:
                    emb.WithColor(DiscordColor.Azure);
                    break;
                case EventOrigin.Role:
                    emb.WithColor(DiscordColor.Lilac);
                    break;
                default:
                    break;
            }

            return emb;
        }
    }
}
