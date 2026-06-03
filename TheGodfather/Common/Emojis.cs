namespace TheGodfather.Common;

public static class Emojis
{
    public static class Animals
    {
        public static IReadOnlyList<string> All { get; } = new[] {
            "\U0001f436",
            "\U0001f431",
            "\U0001f42d",
            "\U0001f439",
            "\U0001f430",
            "\U0001f43b",
            "\U0001f437",
            "\U0001f42e",
            "\U0001f428",
            "\U0001f42f"
        };
    }

    public static class Cards
    {
        public static IReadOnlyList<string> Values { get; } = new[] {
            "\U0001f1e6",
            Numbers.Get(1),
            Numbers.Get(2),
            Numbers.Get(3),
            Numbers.Get(4),
            Numbers.Get(5),
            Numbers.Get(6),
            Numbers.Get(7),
            Numbers.Get(8),
            Numbers.Get(9),
            "\U0001f1ef",
            "\U0001f1f6",
            "\U0001f1f0",
            "\U0001f1e6"
        };

        public static IReadOnlyList<string> Suits { get; } = new[] {
            "\u2663",
            "\u2666",
            "\u2665",
            "\u2660"
        };
    }

    public static class Numbers
    {
        public static IReadOnlyList<string> All { get; } = new[] {
            "0\u20e3",
            "1\u20e3",
            "2\u20e3",
            "3\u20e3",
            "4\u20e3",
            "5\u20e3",
            "6\u20e3",
            "7\u20e3",
            "8\u20e3",
            "9\u20e3",
            "\U0001f51f"
        };

        public static string Get(int index)
            => index >= 0 && index < All.Count ? All[index] : All[0];
    }

    public static class Weapons
    {
        private static readonly IReadOnlyList<string> _all = new[] {
            "\U0001f528",
            "\U0001f5e1",
            "\u26cf",
            "\U0001f4a3",
            "\U0001f525",
            "\U0001f3f9",
            "\U0001f529"
        };

        public static string GetRandomDuelWeapon()
            => _all[new SecureRandom().Next(_all.Count)];
    }

    public const string AlarmClock = "\u23f0";
    public const string ArrowDown = "\u2b07";
    public const string ArrowUp = "\u2b06";
    public const string Bicyclist = "\U0001f6b4";
    public const string BlackSquare = "\u2b1b";
    public const string Blast = "\U0001f4a2";
    public const string BoardPieceBlueCircle = "\U0001f535";
    public const string BoardPieceRedCircle = "\U0001f534";
    public const string BoardSquare = "\u25fb";
    public const string Bomb = "\U0001f4a3";
    public const string Cake = "\U0001f370";
    public const string CheckMarkSuccess = "\u2705";
    public const string Cherries = "\U0001f352";
    public const string Chicken = "\U0001f414";
    public const string Clock1 = "\U0001f550";
    public const string Cloud = "\u2601";
    public const string Dead = "\U0001f635";
    public const string Dice = "\U0001f3b2";
    public const string Drops = "\U0001f4a6";
    public const string DuelSwords = "\u2694";
    public const string EightBall = "\U0001f3b1";
    public const string Gift = "\U0001f381";
    public const string Globe = "\U0001f30d";
    public const string Gun = "\U0001f52b";
    public const string Headphones = "\U0001f3a7";
    public const string Heartbeat = "\U0001f493";
    public const string Information = "\u2139";
    public const string Joystick = "\U0001f579";
    public const string Joy = "\U0001f602";
    public const string LargeBlueDiamond = "\U0001f537";
    public const string LargeOrangeDiamond = "\U0001f536";
    public const string Loudspeaker = "\U0001f4e2";
    public const string Medal = "\U0001f3c5";
    public const string MoneyBag = "\U0001f4b0";
    public const string NewMoon = "\U0001f311";
    public const string NoEntry = "\u26d4";
    public const string O = "\u2b55";
    public const string Paper = "\U0001f9fb";
    public const string Peach = "\U0001f351";
    public const string Question = "\u2753";
    public const string Relieved = "\U0001f616";
    public const string Rock = "\U0001faa8";
    public const string Ruler = "\U0001f4cf";
    public const string Scissors = "\U00002702\U0000fe0f";
    public const string Seven = "7\u20e3";
    public const string SmallBlueDiamond = "\U0001f539";
    public const string SmallOrangeDiamond = "\U0001f538";
    public const string Star = "\U00002b50";
    public const string Syringe = "\U0001f489";
    public const string Tada = "\U0001f389";
    public const string Thermometer = "\U0001f321";
    public const string Trophy = "\U0001f3c6";
    public const string Wave = "\U0001f44b";
    public const string WhiteSquare = "\u2b1c";
    public const string Wind = "\U0001f4a8";
    public const string X = "\u274c";
    public const string Zap = "\U000026a1";
}
