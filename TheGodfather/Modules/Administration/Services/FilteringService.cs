using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using TheGodfather.Common.Collections;

namespace TheGodfather.Modules.Administration.Services;

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
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            this.filters = new ConcurrentDictionary<ulong, ConcurrentHashSet<Filter>>(
                db.Filters
                    .AsEnumerable()
                    .GroupBy(f => f.GuildId)
                    .ToDictionary(g => g.Key, g => new ConcurrentHashSet<Filter>(g))
            );
        } catch (Exception e) {
            Log.Error(e, "Loading filters failed");
        }
    }


    public bool TextContainsFilter(ulong gid, string text, out Filter? match)
    {
        match = null;

        if (!this.filters.TryGetValue(gid, out ConcurrentHashSet<Filter>? fs) || fs is null)
            return false;

        match = fs.FirstOrDefault(f => f.Regex.IsMatch(text));
        return match is { };
    }

    public IReadOnlyList<Filter> GetGuildFilters(ulong gid)
        => this.filters.TryGetValue(gid, out ConcurrentHashSet<Filter>? fs) ? fs.ToList() : Array.Empty<Filter>();

    public Task<bool> AddFilterAsync(ulong gid, string regexString, Filter.Action action)
    {
        return regexString.TryParseRegex(out Regex? regex)
            ? this.AddFilterAsync(gid, regex, action)
            : throw new ArgumentException($"Invalid regex string: {regexString}", nameof(regexString));
    }

    public async Task<bool> AddFilterAsync(ulong gid, Regex? regex, Filter.Action action)
    {
        if (regex is null)
            return false;

        string regexString = regex.ToString();

        ConcurrentHashSet<Filter> fs = this.filters.GetOrAdd(gid, new ConcurrentHashSet<Filter>());
        if (fs.Any(f => string.Compare(f.RegexString, regexString, true) == 0))
            return false;

        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        var filter = new Filter {
            GuildId = gid,
            RegexString = regexString,
            RegexLazy = regex,
            OnHitAction = action
        };
        db.Filters.Add(filter);
        await db.SaveChangesAsync();
        return fs.Add(filter);
    }

    public async Task<bool> AddFiltersAsync(ulong gid, IEnumerable<string> regexStrings, Filter.Action action)
    {
        bool[] res = await Task.WhenAll(regexStrings.Select(s => s.ToRegex()).Select(r => this.AddFilterAsync(gid, r, action)));
        return res.All(r => r);
    }

    public Task<int> RemoveFiltersAsync(ulong gid)
        => this.InternalRemoveByPredicateAsync(gid, _ => true);

    public Task<int> RemoveFiltersAsync(ulong gid, IEnumerable<int> ids)
        => this.InternalRemoveByPredicateAsync(gid, f => ids.Contains(f.Id));

    public Task<int> RemoveFiltersAsync(ulong gid, IEnumerable<string> regexStrings)
        => this.InternalRemoveByPredicateAsync(gid, f => regexStrings.Any(rstr => string.Compare(rstr, f.RegexString, true) == 0));

    public Task<int> RemoveFiltersMatchingAsync(ulong gid, string match)
        => this.InternalRemoveByPredicateAsync(gid, f => f.Regex.IsMatch(match));


    private IQueryable<Filter> InternalGetFiltersForGuild(TheGodfatherDbContext db, ulong gid)
        => db.Filters.AsQueryable().Where(n => n.GuildIdDb == (long)gid);

    private async Task<int> InternalRemoveByPredicateAsync(ulong gid, Func<Filter, bool> predicate)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        var filters = this.InternalGetFiltersForGuild(db, gid)
            .AsEnumerable()
            .Where(predicate)
            .ToList();
        db.Filters.RemoveRange(filters);
        await db.SaveChangesAsync();
        return this.filters.GetValueOrDefault(gid)?.RemoveWhere(predicate) ?? 0;
    }
}