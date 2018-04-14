#region USING_DIRECTIVES
using System;

using TheGodfather.Common;
#endregion

namespace TheGodfather.Modules.Gambling.Common
{
    public class Card : IComparable
    {
        public CardSuit Suit { get; }
        public int Value { get; }


        public Card(CardSuit suit, int value)
        {
            Suit = suit;
            Value = value;
        }


        public int CompareTo(object obj)
        {
            Card c = obj as Card;
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
