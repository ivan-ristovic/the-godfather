using System.Collections.Immutable;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using TheGodfather.Common;

namespace TheGodfather.Modules.Currency.Common
{
    public sealed class SlotMachineRoll
    {
        public static readonly ImmutableArray<int> Multipliers = 
            new[] { 10, 7, 5, 4, 3, 2 }.ToImmutableArray();

        public int[,] Result { get; private set; }
        public long BidAmount { get; }
        public long WonAmount { get; }

        public SlotMachineRoll(long bid)
        {
            this.Result = this.Roll();
            this.BidAmount = bid;
            this.WonAmount = this.EvaluateSlotResult();
        }


        public int[,] Roll()
        {
            var rng = new SecureRandom();
            this.Result = new int[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    this.Result[i, j] = rng.Next(Multipliers.Length);
            return this.Result;
        }

        public long EvaluateSlotResult()
        {
            long pts = this.BidAmount;

            for (int i = 0; i < 3; i++)
                if (this.Result[i, 0] == this.Result[i, 1] && this.Result[i, 1] == this.Result[i, 2])
                    pts *= Multipliers[this.Result[i, 0]];

            for (int i = 0; i < 3; i++)
                if (this.Result[0, i] == this.Result[1, i] && this.Result[1, i] == this.Result[2, i])
                    pts *= Multipliers[this.Result[0, i]];

            return pts == this.BidAmount ? 0L : pts;
        }
    }
}
