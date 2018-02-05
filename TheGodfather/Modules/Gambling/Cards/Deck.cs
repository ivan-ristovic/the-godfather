#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;

using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Gambling.Cards
{
    public class Deck
    {
        private List<Card> _cards;
        public IReadOnlyList<Card> Cards => _cards.AsReadOnly();
        public int CardCount => _cards.Count;


        public Deck()
        {
            OpenNew();
        }


        public void OpenNew()
        {
            _cards = new List<Card>(52);
            for (var i = 1; i < 5; i++)
                for (var j = 1; j < 14; j++)
                    _cards.Add(new Card((CardSuit)i, j));
        }

        public void Shuffle()
        {
            if (CardCount <= 1)
                return;
            var shuffled = _cards.Shuffle();
            _cards = shuffled as List<Card> ?? _cards;
        }

        public Card Draw()
        {
            var card = _cards[0];
            _cards.RemoveAt(0);
            return card;
        }

        public IEnumerable<Card> Draw(int amount)
        {
            if (amount <= 0)
                return Enumerable.Empty<Card>();
            var drawn = _cards.Take(amount);
            _cards.RemoveRange(0, amount);
            return drawn;
        }
    }
}
