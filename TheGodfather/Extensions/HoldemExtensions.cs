#region USING_DIRECTIVES
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
    }
}
