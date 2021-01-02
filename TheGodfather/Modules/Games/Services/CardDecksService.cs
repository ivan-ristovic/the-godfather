using System.Collections.Concurrent;
using TexasHoldem.Logic.Cards;

namespace TheGodfather.Modules.Games.Services
{
    public static class CardDecksService
    {
        private static readonly ConcurrentDictionary<ulong, Deck> _decks = new ConcurrentDictionary<ulong, Deck>();


        public static Deck GetDeckForChannel(ulong cid)
            => _decks.TryGetValue(cid, out Deck? deck) ? deck : new Deck();

        public static void ResetDeckForChannel(ulong cid)
            => _decks.AddOrUpdate(cid, new Deck(), (k, v) => new Deck());
    }
}
