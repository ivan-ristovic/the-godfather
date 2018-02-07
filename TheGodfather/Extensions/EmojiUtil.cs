using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace TheGodfather.Extensions
{
    public static class EmojiUtil
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
            DiscordEmoji.FromUnicode("\U0001f528"), // :hammer:
            DiscordEmoji.FromUnicode("\U0001f5e1"), // :dagger:
            DiscordEmoji.FromUnicode("\u26cf"),     // :pick:
            DiscordEmoji.FromUnicode("\U0001f4a3"), // :bomb:
            DiscordEmoji.FromUnicode("\U0001f525"), // :fire:
            DiscordEmoji.FromUnicode("\U0001f3f9"), // :bow_and_arrow:
            DiscordEmoji.FromUnicode("\U0001f529"), // :nut_and_bolt:
        }.AsReadOnly();
        public static string DuelSwords => DiscordEmoji.FromUnicode("\u2694");
        public static string BoardSquare => DiscordEmoji.FromUnicode("\u25fb");
        public static string BoardPieceX => DiscordEmoji.FromUnicode("\u274c");
        public static string BoardPieceO => DiscordEmoji.FromUnicode("\u2b55");
        public static string BoardPieceBlueCircle => DiscordEmoji.FromUnicode("\U0001f535");
        public static string BoardPieceRedCircle => DiscordEmoji.FromUnicode("\U0001f534");
        public static string BlackSquare => DiscordEmoji.FromUnicode("\u2b1b");
        public static string WhiteSquare => DiscordEmoji.FromUnicode("\u2b1c");
        public static string Syringe => DiscordEmoji.FromUnicode("\U0001f489");

        public static DiscordEmoji GetRandomDuelWeapon(Random rng = null)
            => DuelWeapons[rng != null ? rng.Next(DuelWeapons.Count) : new Random().Next(DuelWeapons.Count)];
    }
}
