using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace TheGodfather.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> items)
        {
            using (var provider = RandomNumberGenerator.Create()) {
                var list = items.ToList();
                var n = list.Count;
                while (n > 1) {
                    var box = new byte[(n / Byte.MaxValue) + 1];
                    int boxSum;
                    do {
                        provider.GetBytes(box);
                        boxSum = box.Sum(b => b);
                    }
                    while (!(boxSum < n * ((Byte.MaxValue * box.Length) / n)));
                    var k = (boxSum % n);
                    n--;
                    var value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
                return list;
            }
        }
    }
}
