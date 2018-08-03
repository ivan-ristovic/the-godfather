#region USING_DIRECTIVES
using DSharpPlus.Entities;
using System.Collections.Generic;
#endregion

namespace TheGodfather.Common
{
    public static class StaticDiscordEmoji
    {
        public static IReadOnlyList<DiscordEmoji> Numbers = new List<DiscordEmoji> {
            DiscordEmoji.FromUnicode("1\u20e3"),
            DiscordEmoji.FromUnicode("2\u20e3"),
            DiscordEmoji.FromUnicode("3\u20e3"),
            DiscordEmoji.FromUnicode("4\u20e3"),
            DiscordEmoji.FromUnicode("5\u20e3"),
            DiscordEmoji.FromUnicode("6\u20e3"),
            DiscordEmoji.FromUnicode("7\u20e3"),
            DiscordEmoji.FromUnicode("8\u20e3"),
            DiscordEmoji.FromUnicode("9\u20e3"),
            DiscordEmoji.FromUnicode("\U0001f51f")
        }.AsReadOnly();

        public static IReadOnlyList<DiscordEmoji> DuelWeapons = new List<DiscordEmoji> {
            DiscordEmoji.FromUnicode("\U0001f528"),     // :hammer:
            DiscordEmoji.FromUnicode("\U0001f5e1"),     // :dagger:
            DiscordEmoji.FromUnicode("\u26cf"),         // :pick:
            DiscordEmoji.FromUnicode("\U0001f4a3"),     // :bomb:
            DiscordEmoji.FromUnicode("\U0001f525"),     // :fire:
            DiscordEmoji.FromUnicode("\U0001f3f9"),     // :bow_and_arrow:
            DiscordEmoji.FromUnicode("\U0001f529"),     // :nut_and_bolt:
        }.AsReadOnly();

        public static IReadOnlyList<DiscordEmoji> Animals = new List<DiscordEmoji> {
            DiscordEmoji.FromUnicode("\U0001f436"),     // :dog:
            DiscordEmoji.FromUnicode("\U0001f431"),     // :cat: 
            DiscordEmoji.FromUnicode("\U0001f42d"),     // :mouse:
            DiscordEmoji.FromUnicode("\U0001f439"),     // :hamster:
            DiscordEmoji.FromUnicode("\U0001f430"),     // :rabbit:
            DiscordEmoji.FromUnicode("\U0001f43b"),     // :bear:
            DiscordEmoji.FromUnicode("\U0001f437"),     // :pig:
            DiscordEmoji.FromUnicode("\U0001f42e"),     // :cow:
            DiscordEmoji.FromUnicode("\U0001f428"),     // :koala:
            DiscordEmoji.FromUnicode("\U0001f42f")      // :tiger:
        }.AsReadOnly();

        public static IReadOnlyList<DiscordEmoji> CardValues = new List<DiscordEmoji>() {
            DiscordEmoji.FromUnicode("\U0001f1e6"),     // :regional_indicator_a:
            Numbers[1],
            Numbers[2],
            Numbers[3],
            Numbers[4],
            Numbers[5],
            Numbers[6],
            Numbers[7],
            Numbers[8],
            DiscordEmoji.FromUnicode("\U0001f1f9"),     // :regional_indicator_t:
            DiscordEmoji.FromUnicode("\U0001f1ef"),     // :regional_indicator_j:
            DiscordEmoji.FromUnicode("\U0001f1f6"),     // :regional_indicator_q:
            DiscordEmoji.FromUnicode("\U0001f1f0"),     // :regional_indicator_k:
            DiscordEmoji.FromUnicode("\U0001f1e6"),     // :regional_indicator_a:
        }.AsReadOnly();

        public static IReadOnlyList<DiscordEmoji> CardSuits = new List<DiscordEmoji>() {
            DiscordEmoji.FromUnicode("\u2660"),         // :spades:
            DiscordEmoji.FromUnicode("\u2663"),         // :clubs:
            DiscordEmoji.FromUnicode("\u2665"),         // :hearts:
            DiscordEmoji.FromUnicode("\u2666"),         // :diamonds:
        }.AsReadOnly();

        // Misc
        public static DiscordEmoji AlarmClock => DiscordEmoji.FromUnicode("\u23f0");
        public static DiscordEmoji Clock1 => DiscordEmoji.FromUnicode("\U0001f550");
        public static DiscordEmoji CheckMarkSuccess => DiscordEmoji.FromUnicode("\u2705");
        public static DiscordEmoji Chicken => DiscordEmoji.FromUnicode("\U0001f414");
        public static DiscordEmoji Medal => DiscordEmoji.FromUnicode("\U0001f3c5");
        public static DiscordEmoji NoEntry => DiscordEmoji.FromUnicode("\u26d4");
        public static DiscordEmoji Question => DiscordEmoji.FromUnicode("\u2753");
        public static DiscordEmoji Wave => DiscordEmoji.FromUnicode("\U0001f44b");

        // Games emoji
        public static DiscordEmoji Blast => DiscordEmoji.FromUnicode("\U0001f4a2");
        public static DiscordEmoji BoardPieceBlueCircle => DiscordEmoji.FromUnicode("\U0001f535");
        public static DiscordEmoji BoardPieceRedCircle => DiscordEmoji.FromUnicode("\U0001f534");
        public static DiscordEmoji BoardPieceO => DiscordEmoji.FromUnicode("\u2b55");
        public static DiscordEmoji BoardPieceX => DiscordEmoji.FromUnicode("\u274c");
        public static DiscordEmoji BoardSquare => DiscordEmoji.FromUnicode("\u25fb");
        public static DiscordEmoji BlackSquare => DiscordEmoji.FromUnicode("\u2b1b");
        public static DiscordEmoji Dead => DiscordEmoji.FromUnicode("\U0001f635");
        public static DiscordEmoji DuelSwords => DiscordEmoji.FromUnicode("\u2694");
        public static DiscordEmoji Gun => DiscordEmoji.FromUnicode("\U0001f52b");
        public static DiscordEmoji Headphones => DiscordEmoji.FromUnicode("\U0001f3a7");
        public static DiscordEmoji Joystick => DiscordEmoji.FromUnicode("\U0001f579");
        public static DiscordEmoji Syringe => DiscordEmoji.FromUnicode("\U0001f489");
        public static DiscordEmoji Trophy => DiscordEmoji.FromUnicode("\U0001f3c6");
        public static DiscordEmoji Relieved => DiscordEmoji.FromUnicode("\U0001f616");
        public static DiscordEmoji WhiteSquare => DiscordEmoji.FromUnicode("\u2b1c");

        public static DiscordEmoji GetRandomDuelWeapon()
            => DuelWeapons[GFRandom.Generator.Next(DuelWeapons.Count)];

        // Slot machine
        public static DiscordEmoji Peach => DiscordEmoji.FromUnicode("\U0001f351");
        public static DiscordEmoji MoneyBag => DiscordEmoji.FromUnicode("\U0001f4b0");
        public static DiscordEmoji Gift => DiscordEmoji.FromUnicode("\U0001f381");
        public static DiscordEmoji LargeBlueDiamond => DiscordEmoji.FromUnicode("\U0001f537");
        public static DiscordEmoji Seven => DiscordEmoji.FromUnicode("7\u20e3");
        public static DiscordEmoji Cherries => DiscordEmoji.FromUnicode("\U0001f352");

        // Weather emoji
        public static DiscordEmoji Cloud => DiscordEmoji.FromUnicode("\u2601");
        public static DiscordEmoji Drops => DiscordEmoji.FromUnicode("\U0001f4a6");
        public static DiscordEmoji Globe => DiscordEmoji.FromUnicode("\U0001f30d");
        public static DiscordEmoji Ruler => DiscordEmoji.FromUnicode("\U0001f4cf");
        public static DiscordEmoji Thermometer => DiscordEmoji.FromUnicode("\U0001f321");
        public static DiscordEmoji Wind => DiscordEmoji.FromUnicode("\U0001f4a8");
    }
}
