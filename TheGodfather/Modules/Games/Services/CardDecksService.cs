#region USING_DIRECTIVES
using System.Collections.Concurrent;

using TexasHoldem.Logic.Cards;
#endregion

namespace TheGodfather.Modules.Games.Services
{
    public static class CardDecksService
    {
        private static readonly ConcurrentDictionary<ulong, Deck> _decks = new ConcurrentDictionary<ulong, Deck>();


        public static Deck GetDeckInChannel(ulong cid)
            => _decks.TryGetValue(cid, out Deck deck) ? deck : null;

        public static void ResetDeckInChannel(ulong cid)
            => _decks.AddOrUpdate(cid, new Deck(), (k, v) => new Deck());
    }
}
