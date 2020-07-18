#region USING_DIRECTIVES
using System.Collections.Immutable;
using DSharpPlus;
using TheGodfather.Common;
#endregion

namespace TheGodfather.Modules.Currency.Common
{
    public static class WorkHandler
    {
        private static ImmutableArray<string> _workAnswers = new[] {
            "hard",
            "as a taxi driver",
            "in a grocery shop",
            "for the local mafia",
            "as bathroom cleaner"
        }.ToImmutableArray();

        private static ImmutableArray<string> _workStreetsPositiveAnswers = new[] {
            "danced whole night",
            "made a good use of the party night",
            "got hired for a bachelor's party",
            "lurked around the singles bar"
        }.ToImmutableArray();
        private static ImmutableArray<string> _workStreetsNegativeAnswers = new[] {
            "got robbed",
            "got ambushed by the local mafia",
            "got his wallet stolen",
            "drank too much - gambled away all the earnings"
        }.ToImmutableArray();

        private static ImmutableArray<string> _crimePositiveAnswers = new[] {
            "successfully robbed the bank",
            "took up a hit-man contract",
            "stole some wallets",
        }.ToImmutableArray();


        public static string GetWorkString(int earned, string currency)
            => $"worked {new SecureRandom().ChooseRandomElement(_workAnswers)} and earned {Formatter.Bold(earned.ToString())} {currency}!";

        public static string GetWorkStreetsString(int change, string currency)
        {
            var rng = new SecureRandom();
            if (change > 0)
                return $"{rng.ChooseRandomElement(_workStreetsPositiveAnswers)} and earned {Formatter.Bold(change.ToString())} {currency}!";
            else if (change < 0)
                return $"{rng.ChooseRandomElement(_workStreetsNegativeAnswers)} and lost {Formatter.Bold((-change).ToString())} {currency}!";
            else
                return "had no luck and got nothing this evening!";
        }

        public static string GetCrimeString(int change, string currency)
        {
            if (change > 0)
                return $"{new SecureRandom().ChooseRandomElement(_crimePositiveAnswers)} and got away with {Formatter.Bold(change.ToString())} {currency}!";
            else if (change < 0)
                return $"got caught and got bailed out of jail for {Formatter.Bold((-change).ToString())} {currency}!";
            else
                return "had no luck and got nothing this evening!";
        }
    }
}
