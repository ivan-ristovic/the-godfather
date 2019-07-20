using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Extensions;
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


        public IReadOnlyCollection<Filter> GetGuildFilters(ulong gid)
        {
            if (this.filters.TryGetValue(gid, out ConcurrentHashSet<Filter> fs))
                return fs.ToList();
            else
                return Array.Empty<Filter>();
        }

        public async Task<bool> AddFilterAsync(ulong gid, string regexString)
        {
            ConcurrentHashSet<Filter> fs = this.filters.GetOrAdd(gid, new ConcurrentHashSet<Filter>());

            using (DatabaseContext db = this.dbb.CreateContext()) {
                var filter = new DatabaseFilter { GuildId = gid, Trigger = regexString };
                db.Filters.Add(filter);
                await db.SaveChangesAsync();

                return fs.Add(new Filter(filter.Id, regexString));
            }
        }

        public async Task<bool> AddFiltersAsync(ulong gid, IEnumerable<string> regexStrings)
        {
            bool[] res = await Task.WhenAll(regexStrings.Select(s => this.AddFilterAsync(gid, s)));
            return res.All(r => r);
        }

        public async Task RemoveFiltersAsync(ulong gid, IEnumerable<int> ids)
        {
            if (!this.filters.TryGetValue(gid, out ConcurrentHashSet<Filter> fs))
                return;

            fs.RemoveWhere(f => ids.Contains(f.Id));
            using (DatabaseContext db = this.dbb.CreateContext()) {
                db.Filters.RemoveRange(ids.Select(id => new DatabaseFilter { GuildId = gid, Id = id }));
                await db.SaveChangesAsync();
            }
        }

        public async Task RemoveFiltersAsync(ulong gid, IEnumerable<string> regexStrings)
        {
            if (!this.filters.TryGetValue(gid, out ConcurrentHashSet<Filter> fs))
                return;

            var rstrs = regexStrings
                .Select(rstr => rstr.CreateWordBoundaryRegex().ToString())
                .ToList();

            fs.RemoveWhere(f => rstrs.Any(rstr => string.Compare(rstr, f.BaseRegexString, true) == 0));
            using (DatabaseContext db = this.dbb.CreateContext()) {
                db.Filters.RemoveRange(db.Filters.Where(f => f.GuildId == gid && rstrs.Any(rstr => string.Compare(rstr, f.Trigger, true) == 0)));
                await db.SaveChangesAsync();
            }
        }

        public async Task<int> RemoveAllFilters(ulong gid)
        {
            this.filters.TryRemove(gid, out ConcurrentHashSet<Filter> fs);

            using (DatabaseContext db = this.dbb.CreateContext()) {
                db.Filters.RemoveRange(db.Filters.Where(f => f.GuildId == gid));
                await db.SaveChangesAsync();
            }

            return fs?.Count ?? 0;
        }
    }
}
