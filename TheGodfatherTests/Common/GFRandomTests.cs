#region USING_DIRECTIVES
using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;

using TheGodfather.Common;
#endregion

namespace TheGodfatherTests.Common
{
    [TestFixture]
    public class GFRandomTests
    {
        private static readonly int RNG_THRESHOLD = 1000;

        private readonly GFRandom rng = GFRandom.Generator;


        [Test]
        public void NextBoolTests()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => this.rng.NextBool(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => this.rng.NextBool(-1));
            Assert.DoesNotThrow(() => this.rng.NextBool(1));

            bool[] values = new[] { true, false };
            this.TryGenerateAll(values, () => this.rng.NextBool());
            this.TryGenerateAll(values, () => this.rng.NextBool(1));
            this.TryGenerateAll(values, () => this.rng.NextBool(2));
            this.TryGenerateAll(values, () => this.rng.NextBool(3));
        }

        [Test]
        public void GetBytesTests()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => this.rng.GetBytes(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => this.rng.GetBytes(-1));
            Assert.DoesNotThrow(() => this.rng.GetBytes(1));

            for (int i = 1; i < 8; i++)
                Assert.That(this.rng.GetBytes(i).Length == i);
        }

        [Test]
        public void NextTests()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => this.rng.Next(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => this.rng.Next(0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => this.rng.Next(1, 0));
            Assert.DoesNotThrow(() => this.rng.Next(1));
            Assert.DoesNotThrow(() => this.rng.Next(1, 5));
            Assert.DoesNotThrow(() => this.rng.Next(-5, -3));
            Assert.DoesNotThrow(() => this.rng.Next(-5, 3));

            this.TryGenerateAll(GetIntegers(0, 5), () => this.rng.Next(5));
            this.TryGenerateAll(GetIntegers(-5, -3), () => this.rng.Next(-5, -3));
            this.TryGenerateAll(GetIntegers(-3, 3), () => this.rng.Next(-3, 3));


            int[] GetIntegers(int start, int end)
            {
                int[] arr = new int[end - start];
                for (int i = 0; i < arr.Length; i++)
                    arr[i] = start + i;
                return arr;
            }
        }


        private void TryGenerateAll<T>(IEnumerable<T> values, Func<T> unit, bool checkOutOfBounds = true)
        {
            int count = values.Count();
            var generated = new HashSet<T>(count);
            for (int i = 0; i < RNG_THRESHOLD; i++) {
                T g = unit();
                if (checkOutOfBounds && !values.Contains(g))
                    throw new Exception($"Element generated which was not present in the original collection: {g}");
                generated.Add(g);
            }

            if (generated.Count != count)
                throw new Exception($"Random element generation did not generate every given value ({generated.Count}/{count}) in {RNG_THRESHOLD} iterations.");
        }
    }
}
