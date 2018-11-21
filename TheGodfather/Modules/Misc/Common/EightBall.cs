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
            "Are you crazy? No.",
            "As I see it, no.",
            "Unlikely.",
            "No.",
            "I don't think so.",
            "I need time to think, ask me later."
            "I think so.",
            "Yes.",
            "As I see it, yes.",
            "Hell yeah!",
            "Most likely.",
            "Definitely YES.",
            "More than you can imagine."
        }.ToImmutableArray();

        public static string GenerateRandomAnswer => _answers[GFRandom.Generator.Next(_answers.Length)];
    }
}
