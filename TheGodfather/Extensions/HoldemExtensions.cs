#region USING_DIRECTIVES
using System.Collections.Generic;
using TheGodfather.Common;

using TexasHoldem.Logic.Cards;
#endregion

namespace TheGodfather.Extensions
{
    public static class HoldemExtensions
    {
        public static string ToUserFriendlyString(this Card c)
        {
            if (c.Type >= CardType.Two && c.Type <= CardType.Ace)
                return StaticDiscordEmoji.CardValues[(int)(c.Type - 1)] + c.Suit.ToFriendlyString();
            else
                return StaticDiscordEmoji.Question + c.Suit.ToFriendlyString();
        }

        public static IReadOnlyList<Card> DrawCards(this Deck d, int amount)
        {
            List<Card> _cards = new List<Card>();
            try {
                for (int i = 0; i < amount; i++)
                    _cards.Add(d.GetNextCard());
            } catch {
                _cards.Clear();
            }
            return _cards.AsReadOnly();
        }
    }
}
