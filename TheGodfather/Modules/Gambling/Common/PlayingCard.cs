#region USING_DIRECTIVES
using System;

using TheGodfather.Common;
#endregion

namespace TheGodfather.Modules.Gambling.Common
{

    public class PlayingCard : IComparable
    {
        public PlayingCardSuit Suit { get; }
        public int Value { get; }


        public PlayingCard(PlayingCardSuit suit, int value)
        {
            Suit = suit;
            Value = value;
        }


        public int CompareTo(object obj)
        {
            PlayingCard c = obj as PlayingCard;
            return c != null ? Value - c.Value : 0;
        }

        public override string ToString()
        {
            if (Value > 0 && Value < 14)
                return StaticDiscordEmoji.CardValues[Value - 1] + Suit.ToDiscordEmoji();
            else
                return StaticDiscordEmoji.Question + Suit.ToDiscordEmoji();
        }
    }
}
