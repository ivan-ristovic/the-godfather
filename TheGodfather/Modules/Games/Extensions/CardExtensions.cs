using TexasHoldem.Logic.Cards;
using TheGodfather.Common;

namespace TheGodfather.Modules.Games.Extensions
{
    public static class CardExtensions
    {
        public static string ToEmojiString(this Card card)
            => $"{Emojis.Cards.Values[(int)card.Type]}{Emojis.Cards.Suits}";
    }
}
