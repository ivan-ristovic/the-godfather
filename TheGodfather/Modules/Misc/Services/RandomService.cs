using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Serilog;
using TheGodfather.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Misc.Services
{
    public sealed class RandomService : ITheGodfatherService
    {
        private static ImmutableArray<string> _regularAnswers = new[] {
            "8b-00", "8b-01", "8b-02", "8b-03", "8b-04", "8b-05",
            "8b-06", "8b-07", "8b-08", "8b-09", "8b-10", "8b-11",
            "8b-12", "8b-13",
        }.ToImmutableArray();

        private static ImmutableArray<string> _timeAnswers = new[] {
            "8b-t-00", "8b-t-01", "8b-t-02", "8b-t-03", "8b-t-04", "8b-t-05",
            "8b-t-06", "8b-t-07", "8b-t-08", "8b-t-09", "8b-t-10",
        }.ToImmutableArray();

        private static ImmutableArray<string> _quantityAnswers = new[] {
            "8b-q-00", "8b-q-01", "8b-q-02", "8b-q-03", "8b-q-04",
            "8b-q-05", "8b-q-06", "8b-q-07", "8b-q-08",
        }.ToImmutableArray();

        public bool IsDisabled => false;

        private readonly SecureRandom rng;
        private ImmutableDictionary<char, string> leetAlphabet;


        public RandomService(bool loadData = true)
        {
            this.rng = new SecureRandom();
            this.leetAlphabet = new Dictionary<char, string>() {
                { 'i' , "i1" },
                { 'l' , "l1" },
                { 'e' , "e3" },
                { 'a' , "@4" },
                { 't' , "t7" },
                { 'o' , "o0" },
                { 's' , "s5" },
            }.ToImmutableDictionary();
            if (loadData)
                this.LoadData("Resources");
        }


        public void LoadData(string path)
        {
            LoadLeetAlphabet(Path.Combine(path, "leet_alphabet.json"));

            void LoadLeetAlphabet(string alphabetPath)
            {
                try {
                    Log.Debug("Loading leet alphabet from {Path}", alphabetPath);
                    string json = File.ReadAllText(alphabetPath, Encoding.UTF8);
                    this.leetAlphabet = JsonConvert.DeserializeObject<Dictionary<char, string>>(json)
                        .ToImmutableDictionary();
                } catch (Exception e) {
                    Log.Fatal(e, "Failed to load leet alphabet");
                    throw;
                }
            }
        }

        public bool Coinflip(int trueRatio = 1)
            => this.rng.NextBool(trueRatio);

        public int Dice(int sides = 6)
            => this.rng.Next(sides) + 1;

        public string ToLeet(string text)
        {
            var sb = new StringBuilder();
            foreach (char c in text) {
                char code = this.rng.NextBool() ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c);
                if (rng.NextBool() && this.leetAlphabet.TryGetValue(code, out string? codes))
                    code = this.rng.ChooseRandomChar(codes);
                sb.Append(code);
            }
            return sb.ToString();
        }

        public string GetRandomYesNoAnswer()
            => this.rng.ChooseRandomElement(_regularAnswers);

        public string GetRandomTimeAnswer()
            => this.rng.ChooseRandomElement(_timeAnswers);

        public string GetRandomQuantityAnswer()
            => this.rng.ChooseRandomElement(_quantityAnswers);
    }
}
