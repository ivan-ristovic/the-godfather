using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGodfather.Helpers;
using TheGodfather.Helpers.Collections;

namespace TheGodfather
{
    public sealed class SharedData
    {
        public ConcurrentDictionary<ulong, string> GuildPrefixes { get; }
        public ConcurrentDictionary<ulong, ConcurrentHashSet<string>> GuildFilters { get; }


        public SharedData(ConcurrentDictionary<ulong, string> gp,
                          ConcurrentDictionary<ulong, ConcurrentHashSet<string>> gf = null)
        {
            GuildPrefixes = gp;
            GuildFilters = gf;
        }
    }
}
