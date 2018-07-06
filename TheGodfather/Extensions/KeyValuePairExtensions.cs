using System.Collections.Generic;

namespace TheGodfather.Extensions
{
    public static class KeyValuePairExtensions
    {
        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> kvp, out T1 key, out T2 value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
}
