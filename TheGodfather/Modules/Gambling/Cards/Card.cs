using System;

namespace TheGodfather.Modules.Gambling.Cards
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

        private char GetSuitSign(CardSuit suit)
        {
            switch (suit) {
                case CardSuit.Clubs:
                    return '♣';
                case CardSuit.Diamonds:
                    return '♦';
                case CardSuit.Hearts:
                    return '♥';
                case CardSuit.Spades:
                    return '♠';
            }
            return '?';
        }

        public override string ToString()
        {
            char suit = GetSuitSign(Suit);
            if (Value > 1 && Value < 10)
                return Value.ToString() + suit;
            else if (Value == 10)
                return "T" + suit;
            else if (Value == 11)
                return "J" + suit;
            else if (Value == 12)
                return "Q" + suit;
            else if (Value == 13)
                return "K" + suit;
            else if (Value == 1)
                return "A" + suit;
            else
                return "?" + suit;
        }
    }
}
