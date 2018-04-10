#region USING_DIRECTIVES
using System;
using System.Collections.Generic;

using DSharpPlus.Entities;
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
        };

        public static DiscordEmoji CheckMarkSuccess => DiscordEmoji.FromUnicode("\u2705");
        public static DiscordEmoji DuelSwords => DiscordEmoji.FromUnicode("\u2694");
        public static DiscordEmoji BoardSquare => DiscordEmoji.FromUnicode("\u25fb");
        public static DiscordEmoji BoardPieceX => DiscordEmoji.FromUnicode("\u274c");
        public static DiscordEmoji BoardPieceO => DiscordEmoji.FromUnicode("\u2b55");
        public static DiscordEmoji BoardPieceBlueCircle => DiscordEmoji.FromUnicode("\U0001f535");
        public static DiscordEmoji BoardPieceRedCircle => DiscordEmoji.FromUnicode("\U0001f534");
        public static DiscordEmoji BlackSquare => DiscordEmoji.FromUnicode("\u2b1b");
        public static DiscordEmoji WhiteSquare => DiscordEmoji.FromUnicode("\u2b1c");
        public static DiscordEmoji Syringe => DiscordEmoji.FromUnicode("\U0001f489");
        public static DiscordEmoji Trophy => DiscordEmoji.FromUnicode("\U0001f3c6");
        public static DiscordEmoji Joystick => DiscordEmoji.FromUnicode("\U0001f579");
        public static DiscordEmoji Wave => DiscordEmoji.FromUnicode("\U0001f44b");
        public static DiscordEmoji Headphones => DiscordEmoji.FromUnicode("\U0001f3a7");
        public static DiscordEmoji Question => DiscordEmoji.FromUnicode("\u2753");

        public static DiscordEmoji Globe => DiscordEmoji.FromUnicode("\U0001f30d");
        public static DiscordEmoji Ruler => DiscordEmoji.FromUnicode("\U0001f4cf");
        public static DiscordEmoji Cloud => DiscordEmoji.FromUnicode("\u2601");
        public static DiscordEmoji Drops => DiscordEmoji.FromUnicode("\U0001f4a6");
        public static DiscordEmoji Thermometer => DiscordEmoji.FromUnicode("\U0001f321");
        public static DiscordEmoji Wind => DiscordEmoji.FromUnicode("\U0001f4a8");
        

        public static DiscordEmoji GetRandomDuelWeapon()
            => DuelWeapons[GFRandom.Generator.Next(DuelWeapons.Count)];
    }
}
