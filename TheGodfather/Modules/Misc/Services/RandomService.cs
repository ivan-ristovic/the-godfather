using System;
using System.Collections.Immutable;
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


        public RandomService(bool loadData = true)
        {
            this.rng = new SecureRandom();
            if (loadData)
                this.LoadData("Resources");
        }


        public void LoadData(string path)
        {
            try {
                Log.Debug("Loading random service data from {Folder}", path);

                // TODO

            } catch (Exception e) {
                Log.Fatal(e, "Failed to load command translations");
                throw;
            }
        }

        public bool Coinflip(int trueRatio = 1)
            => this.rng.NextBool(trueRatio);

        public int Dice(int sides = 6)
            => this.rng.Next(sides) + 1;

        public string GetRandomYesNoAnswer()
            => this.rng.ChooseRandomElement(_regularAnswers);

        public string GetRandomTimeAnswer()
            => this.rng.ChooseRandomElement(_timeAnswers);

        public string GetRandomQuantityAnswer()
            => this.rng.ChooseRandomElement(_quantityAnswers);
    }
}
