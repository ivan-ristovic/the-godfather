#region USING_DIRECTIVES
using System.Collections.Generic;
#endregion

namespace TheGodfather.Extensions
{
    internal static class KeyValuePairExtensions
    {
        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> kvp, out T1 key, out T2 value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
}
