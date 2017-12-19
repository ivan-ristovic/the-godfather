using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheGodfather.Helpers;
using TheGodfather.Helpers.Collections;

namespace TheGodfather
{
    public sealed class SharedData
    {
        public ConcurrentDictionary<ulong, string> GuildPrefixes { get; }
        public ConcurrentDictionary<ulong, ConcurrentHashSet<Regex>> GuildFilters { get; }

        private BotConfig _cfg { get; }


        public SharedData(BotConfig cfg,
                          ConcurrentDictionary<ulong, string> gp,
                          ConcurrentDictionary<ulong, ConcurrentHashSet<Regex>> gf)
        {
            _cfg = cfg;
            GuildPrefixes = gp;
            GuildFilters = gf;
        }


        #region PREFIXES
        public string GetGuildPrefix(ulong gid)
        {
            if (GuildPrefixes.ContainsKey(gid) && !string.IsNullOrWhiteSpace(GuildPrefixes[gid]))
                return GuildPrefixes[gid];
            else
                return _cfg.DefaultPrefix;
        }

        public bool TrySetGuildPrefix(ulong gid, string prefix)
        {
            if (GuildPrefixes.ContainsKey(gid)) {
                GuildPrefixes[gid] = prefix;
                return true;
            } else {
                return GuildPrefixes.TryAdd(gid, prefix);
            }
        }
        #endregion

        #region FILTERS
        public IReadOnlyCollection<Regex> GetFiltersForGuild(ulong gid)
        {
            if (GuildFilters.ContainsKey(gid) && GuildFilters[gid] != null)
                return GuildFilters[gid];
            else
                return null;
        }

        public bool ContainsFilter(ulong gid, string message)
        {
            if (!GuildFilters.ContainsKey(gid) || GuildFilters[gid] == null)
                return false;

            message = message.ToLower();
            return GuildFilters[gid].Any(f => f.Match(message).Success);
        }

        public bool TryAddGuildFilter(ulong gid, Regex regex)
        {
            if (GuildFilters.ContainsKey(gid)) {
                if (GuildFilters[gid] == null)
                    GuildFilters[gid] = new ConcurrentHashSet<Regex>();
            } else {
                if (!GuildFilters.TryAdd(gid, new ConcurrentHashSet<Regex>()))
                    return false;
            }
            
            if (GuildFilters[gid].Any(r => r.ToString() == regex.ToString()))
                return false;

            return GuildFilters[gid].Add(regex);
        }

        public bool TryRemoveGuildFilter(ulong gid, string filter)
        {
            if (!GuildFilters.ContainsKey(gid))
                return false;

            var rstr = $@"\b{filter}\b";
            return GuildFilters[gid].RemoveWhere(r => r.ToString() == rstr) > 0;
        }

        public void ClearGuildFilters(ulong gid)
        {
            if (!GuildFilters.ContainsKey(gid))
                return;

            GuildFilters[gid].Clear();
        }
        #endregion
    }
}
