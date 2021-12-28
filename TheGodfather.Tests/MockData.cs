using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace TheGodfather.Tests;

public static class MockData
{
    public static MockIdCollection Ids = new();


    public class MockIdCollection : IReadOnlyCollection<ulong>
    {
        private static readonly ImmutableArray<ulong> _ids = new ulong[] {
            125649888611401728, 201315884709576705, 379378609942560770, 479378612343120770, 515098985770385419,
            621356153163285419
        }.ToImmutableArray();


        public ulong this[int i] => _ids[i % _ids.Length];

        public int Count => _ids.Length;

        public IEnumerator<ulong> GetEnumerator() => _ids.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}