using System;
using System.Collections.Generic;
using System.Linq;
using TheGodfather.Common;

namespace TheGodfather.Extensions
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) 
            => source.Shuffle(new SecureRandom());

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, SecureRandom rng)
        {
            if (source is null) 
                throw new ArgumentNullException(nameof(source));
            if (rng is null) 
                throw new ArgumentNullException(nameof(rng));

            return source.ShuffleIterator(rng);
        }


        private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source, SecureRandom rng)
        {
            var buffer = source.ToList();
            for (int i = 0; i < buffer.Count; i++) {
                int j = rng.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }
    }
}
