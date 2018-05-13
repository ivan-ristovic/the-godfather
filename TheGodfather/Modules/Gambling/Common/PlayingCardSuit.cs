#region USING_DIRECTIVES
using TheGodfather.Common;

using DSharpPlus.Entities;
#endregion;

namespace TheGodfather.Modules.Gambling.Common
{
    public enum PlayingCardSuit
    {
        Spades = 1,
        Hearts = 2,
        Diamonds = 3,
        Clubs = 4
    }

    public static class PlayingCardSuitExtensions
    {
        public static DiscordEmoji ToDiscordEmoji(this PlayingCardSuit suit)
        {
            switch (suit) {
                case PlayingCardSuit.Spades:
                    return StaticDiscordEmoji.CardSuits[0];
                case PlayingCardSuit.Clubs:
                    return StaticDiscordEmoji.CardSuits[1];
                case PlayingCardSuit.Hearts:
                    return StaticDiscordEmoji.CardSuits[2];
                case PlayingCardSuit.Diamonds:
                    return StaticDiscordEmoji.CardSuits[3];
            }
            return StaticDiscordEmoji.Question;
        }
    }
}
