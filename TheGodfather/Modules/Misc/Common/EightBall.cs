using System;

namespace TheGodfather.Modules.Misc.Common
{
    public static class EightBall
    {
        private static string[] _answers = {
            "Definitely no.",
            "No.",
            "Possibly.",
            "Maybe.",
            "Perhaps.",
            "Yes.",
            "Definitely yes.",
            "More than you can imagine."
        };

        public static string Answer => _answers[GFRandom.Generator.Next(_answers.Length)];
    }
}
