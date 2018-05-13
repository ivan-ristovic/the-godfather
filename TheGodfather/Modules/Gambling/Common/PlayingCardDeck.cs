#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Linq;

using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Gambling.Common
{
    public class PlayingCardDeck
    {
        private List<PlayingCard> _cards;
        public IReadOnlyList<PlayingCard> Cards => _cards.AsReadOnly();
        public int CardCount => _cards.Count;


        public PlayingCardDeck()
        {
            OpenNew();
        }


        public void OpenNew()
        {
            _cards = new List<PlayingCard>(52);
            for (var i = 1; i < 5; i++)
                for (var j = 1; j < 14; j++)
                    _cards.Add(new PlayingCard((PlayingCardSuit)i, j));
        }

        public void Shuffle()
        {
            if (CardCount <= 1)
                return;
            var shuffled = _cards.Shuffle();
            _cards = shuffled as List<PlayingCard> ?? _cards;
        }

        public PlayingCard Draw()
        {
            var card = _cards[0];
            _cards.RemoveAt(0);
            return card;
        }

        public List<PlayingCard> Draw(int amount)
        {
            if (amount <= 0)
                return new List<PlayingCard>();
            var drawn = _cards.Take(amount);
            _cards.RemoveRange(0, amount);
            return drawn.ToList();
        }
    }
}
