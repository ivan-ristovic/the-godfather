#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Helpers.DataManagers
{
    public class FilterManager
    {
        public IReadOnlyDictionary<ulong, List<Regex>> Filters => _filters;
        private ConcurrentDictionary<ulong, List<Regex>> _filters = new ConcurrentDictionary<ulong, List<Regex>>();
        private bool _ioerr = false;


        public FilterManager()
        {

        }


        public void Load(DebugLogger log)
        {
            if (File.Exists("Resources/filters.json")) {
                try {
                    _filters = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, List<Regex>>>(File.ReadAllText("Resources/filters.json"));
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Error, "TheGodfather", "Filter loading error, check file formatting. Details:\n" + e.ToString(), DateTime.Now);
                    _ioerr = true;
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "filters.json is missing.", DateTime.Now);
            }
        }

        public bool Save(DebugLogger log)
        {
            if (_ioerr) {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "Filter saving skipped until file conflicts are resolved!", DateTime.Now);
                return false;
            }

            try {
                File.WriteAllText("Resources/filters.json", JsonConvert.SerializeObject(_filters));
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "IO Filter save error. Details:\n" + e.ToString(), DateTime.Now);
                return false;
            }

            return true;
        }

        public bool Contains(ulong gid, string message)
        {
            message = message.ToLower();
            if (_filters.ContainsKey(gid) && _filters[gid].Any(f => f.Match(message).Success))
                return true;
            else
                return false;
        }

        public bool TryAdd(ulong gid, Regex regex)
        {
            if (!_filters.ContainsKey(gid))
                if (!_filters.TryAdd(gid, new List<Regex>()))
                    return false;

            if (_filters[gid].Any(r => r.ToString() == regex.ToString()))
                return false;

            _filters[gid].Add(regex);
            return true;
        }

        public bool TryRemoveAt(ulong gid, int index)
        {
            if (!_filters.ContainsKey(gid))
                return false;

            if (index < 0 || index > _filters[gid].Count)
                return false;

            _filters[gid].RemoveAt(index);
            return true;
        }

        public bool ClearGuildFilters(ulong gid)
        {
            if (!_filters.ContainsKey(gid))
                return false;

            return _filters.TryRemove(gid, out _);
        }

        public void ClearAllFilters()
        {
            _filters.Clear();
        }
    }
}
