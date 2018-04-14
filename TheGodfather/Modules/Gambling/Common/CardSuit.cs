#region USING_DIRECTIVES
using TheGodfather.Common;

using DSharpPlus.Entities;
#endregion;

namespace TheGodfather.Modules.Gambling.Common
{
    public enum CardSuit
    {
        Spades = 1,
        Hearts = 2,
        Diamonds = 3,
        Clubs = 4
    }

    public static class CardSuitExtensions
    {
        public static DiscordEmoji ToDiscordEmoji(this CardSuit suit)
        {
            switch (suit) {
                case CardSuit.Spades:
                    return StaticDiscordEmoji.CardSuits[0];
                case CardSuit.Clubs:
                    return StaticDiscordEmoji.CardSuits[1];
                case CardSuit.Hearts:
                    return StaticDiscordEmoji.CardSuits[2];
                case CardSuit.Diamonds:
                    return StaticDiscordEmoji.CardSuits[3];
            }
            return StaticDiscordEmoji.Question;
        }
    }
}
