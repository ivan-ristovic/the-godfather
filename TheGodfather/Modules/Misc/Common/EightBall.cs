#region USING_DIRECTIVES
using System.Collections.Immutable;
using TheGodfather.Common;
#endregion

namespace TheGodfather.Modules.Misc.Common
{
    public static class EightBall
    {
        private static ImmutableArray<string> _answers = new string[] {
            "Definitely NO.",
            "No.",
            "I don't think so.",
            "I think so.",
            "Yes.",
            "Definitely YES.",
            "More than you can imagine."
        }.ToImmutableArray();

        public static string GenerateRandomAnswer => _answers[GFRandom.Generator.Next(_answers.Length)];
    }
}
