using System;
using System.Collections.Concurrent;
using System.Linq;
using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class FilteringService : ITheGodfatherService
    {
        public bool IsDisabled => false;


        private readonly DatabaseContextBuilder dbb;
        private readonly Logger log;
        private ConcurrentDictionary<ulong, ConcurrentHashSet<Filter>> filters;

        // TODO remove
        public ConcurrentDictionary<ulong, ConcurrentHashSet<Filter>> Filters => this.filters;


        public FilteringService(DatabaseContextBuilder dbb, Logger log, bool loadData = true)
        {
            this.dbb = dbb;
            this.log = log;
            this.filters = new ConcurrentDictionary<ulong, ConcurrentHashSet<Filter>>();
            if (loadData)
                this.LoadData();
        }


        public void LoadData()
        {
            try {
                using (DatabaseContext db = this.dbb.CreateContext()) {
                    this.filters = new ConcurrentDictionary<ulong, ConcurrentHashSet<Filter>>(
                        db.Filters
                            .GroupBy(f => f.GuildId)
                            .ToDictionary(g => g.Key, g => new ConcurrentHashSet<Filter>(g.Select(f => new Filter(f.Id, f.Trigger))))
                    );
                }
            } catch (Exception e) {
                this.log.Log(DSharpPlus.LogLevel.Error, e);
            }
        }


        public bool ContainsFilter(ulong gid, string text)
        {
            return this.filters.TryGetValue(gid, out ConcurrentHashSet<Filter> fs) && !(fs is null)
                ? fs.Any(f => f.Trigger.IsMatch(text))
                : false;
        }
    }
}
