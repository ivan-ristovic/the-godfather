using TexasHoldem.Logic.Cards;

namespace TheGodfather.Extensions;

internal static class HoldemExtensions
{
    public static IReadOnlyList<Card> DrawCards(this Deck deck, int amount)
    {
        var cards = new List<Card>();
        try {
            for (int i = 0; i < amount; i++)
                cards.Add(deck.GetNextCard());
        } catch {
            cards.Clear();
        }
        return cards.AsReadOnly();
    }

    public static string ToUserFriendlyString(this Card card)
    {
        return card.Type is >= CardType.Two and <= CardType.Ace
            ? Emojis.Cards.Values[(int)(card.Type - 1)] + card.Suit.ToFriendlyString()
            : Emojis.Question + card.Suit.ToFriendlyString();
    }
}