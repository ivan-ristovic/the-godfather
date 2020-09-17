using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class FilteringService : ITheGodfatherService
    {
        public bool IsDisabled => false;

        private ConcurrentDictionary<ulong, ConcurrentHashSet<Filter>> filters;
        private readonly DbContextBuilder dbb;


        public FilteringService(DbContextBuilder dbb, bool loadData = true)
        {
            this.dbb = dbb;
            this.filters = new ConcurrentDictionary<ulong, ConcurrentHashSet<Filter>>();
            if (loadData)
                this.LoadData();
        }


        public void LoadData()
        {
            Log.Debug("Loading filters");
            try {
                using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                    this.filters = new ConcurrentDictionary<ulong, ConcurrentHashSet<Filter>>(
                        db.Filters
                            .AsEnumerable()
                            .GroupBy(f => f.GuildId)
                            .ToDictionary(g => g.Key, g => new ConcurrentHashSet<Filter>(g))
                    );
                }
            } catch (Exception e) {
                Log.Error(e, "Loading filters failed");
            }
        }


        public bool TextContainsFilter(ulong gid, string text)
            => this.filters.TryGetValue(gid, out ConcurrentHashSet<Filter>? fs) && fs is { } && fs.Any(f => f.Trigger.IsMatch(text));

        public IReadOnlyCollection<Filter> GetGuildFilters(ulong gid)
            => this.filters.TryGetValue(gid, out ConcurrentHashSet<Filter>? fs) ? fs.ToList() : (IReadOnlyCollection<Filter>)Array.Empty<Filter>();

        public Task<bool> AddFilterAsync(ulong gid, string regexString)
        {
            return regexString.TryParseRegex(out Regex? regex)
                ? this.AddFilterAsync(gid, regex)
                : throw new ArgumentException($"Invalid regex string: {regexString}", nameof(regexString));
        }

        public async Task<bool> AddFilterAsync(ulong gid, Regex? regex)
        {
            if (regex is null)
                return false;

            string regexString = regex.ToString();

            ConcurrentHashSet<Filter> fs = this.filters.GetOrAdd(gid, new ConcurrentHashSet<Filter>());
            if (fs.Any(f => string.Compare(f.TriggerString, regexString, true) == 0))
                return false;

            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                var filter = new Filter {
                    GuildId = gid,
                    TriggerString = regexString,
                    TriggerLazy = regex,
                };
                db.Filters.Add(filter);
                await db.SaveChangesAsync();
                return fs.Add(filter);
            }
        }

        public async Task<bool> AddFiltersAsync(ulong gid, IEnumerable<string> regexStrings)
        {
            bool[] res = await Task.WhenAll(regexStrings.Select(s => s.ToRegex()).Select(r => this.AddFilterAsync(gid, r)));
            return res.All(r => r);
        }

        public async Task<int> RemoveFiltersAsync(ulong gid)
        {
            this.filters.TryRemove(gid, out ConcurrentHashSet<Filter>? fs);

            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                db.Filters.RemoveRange(db.Filters.Where(f => f.GuildIdDb == (long)gid));
                await db.SaveChangesAsync();
            }

            return fs?.Count ?? 0;
        }

        public async Task<int> RemoveFiltersAsync(ulong gid, IEnumerable<int> ids)
        {
            int removed = 0;

            if (this.filters.TryGetValue(gid, out ConcurrentHashSet<Filter>? fs)) {
                removed = fs.RemoveWhere(f => ids.Contains(f.Id));
                using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                    db.Filters.RemoveRange(
                        db.Filters
                          .Where(f => f.GuildIdDb == (long)gid)
                          .AsEnumerable()
                          .Where(f => ids.Contains(f.Id))
                    );
                    await db.SaveChangesAsync();
                }
            }

            return removed;
        }

        public async Task<int> RemoveFiltersAsync(ulong gid, IEnumerable<string> regexStrings)
        {
            int removed = 0;

            if (this.filters.TryGetValue(gid, out ConcurrentHashSet<Filter>? fs)) {
                removed = fs.RemoveWhere(f => regexStrings.Any(rstr => string.Compare(rstr, f.TriggerString, true) == 0));
                using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                    db.Filters.RemoveRange(
                        db.Filters
                          .Where(f => f.GuildIdDb == (long)gid)
                          .AsEnumerable()
                          .Where(f => regexStrings.Any(rstr => string.Compare(rstr, f.TriggerString, true) == 0))
                    );
                    await db.SaveChangesAsync();
                }
            }

            return removed;
        }

        public async Task<int> RemoveFiltersMatchingAsync(ulong gid, string match)
        {
            int removed = 0;

            if (this.filters.TryGetValue(gid, out ConcurrentHashSet<Filter>? fs)) {
                removed = fs.RemoveWhere(f => f.Trigger.IsMatch(match));
                using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                    db.Filters.RemoveRange(
                        db.Filters
                          .Where(f => f.GuildIdDb == (long)gid)
                          .AsEnumerable()
                          .Where(f => f.Trigger.IsMatch(match))
                    );
                    await db.SaveChangesAsync();
                }
            }

            return removed;
        }
    }
}
