using System;

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
            switch (Value) {
                case 1:  return ":regional_indicator_a:" + suit;
                case 2:  return ":two:" + suit;
                case 3:  return ":three:" + suit;
                case 4:  return ":four:" + suit;
                case 5:  return ":five:" + suit;
                case 6:  return ":six:" + suit;
                case 7:  return ":seven:" + suit;
                case 8:  return ":eight:" + suit;
                case 9:  return ":nine:" + suit;
                case 10: return ":regional_indicator_t:" + suit;
                case 11: return ":regional_indicator_j:" + suit;
                case 12: return ":regional_indicator_q:" + suit;
                case 13: return ":regional_indicator_k:" + suit;
            }

            return ":question:" + suit;
        }
    }
}
