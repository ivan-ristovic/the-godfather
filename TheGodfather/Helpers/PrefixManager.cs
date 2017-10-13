#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Helpers
{
    public class PrefixManager
    {
        public IReadOnlyDictionary<ulong, string> Prefixes => _prefixes;
        private ConcurrentDictionary<ulong, string> _prefixes = new ConcurrentDictionary<ulong, string>();


        public PrefixManager()
        {

        }


        public void Load(DebugLogger log)
        {
            // TODO
        }

        public bool Save(DebugLogger log)
        {
            // TODO
            return true;
        }

        public string GetPrefixForChannelId(ulong cid)
        {
            if (_prefixes.ContainsKey(cid))
                return _prefixes[cid];
            else
                return TheGodfather.Config.DefaultPrefix;
        }

        public void SetPrefixForChannelId(ulong cid, string prefix)
        {
            if (_prefixes.ContainsKey(cid))
                _prefixes[cid] = prefix;
            _prefixes.TryAdd(cid, prefix);
        }
    }
}
