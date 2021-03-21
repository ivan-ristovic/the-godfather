using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class ForbiddenNamesService : ITheGodfatherService
    {
        private static ImmutableArray<ulong> _ids = new ulong[] {
            125649888611401728,
            201315884709576705,
            379378609942560770,
            479378612343120770,
            515098985770385419,
            621356153163285419,
        }.ToImmutableArray();

        public bool IsDisabled => false;

        private readonly DbContextBuilder dbb;


        public ForbiddenNamesService(DbContextBuilder dbb)
        {
            this.dbb = dbb;
        }


        public bool IsSafePattern(Regex regex)
            => _ids.All(u => !regex.IsMatch(u.ToString()));

        public bool IsNameForbidden(ulong gid, string name, out ForbiddenName? match)
        {
            match = null;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                match = this.InternalGetForbiddenNamesForGuild(db, gid)
                    .AsEnumerable()
                    .FirstOrDefault(fn => fn.Regex.IsMatch(name))
                    ;
            }
            return match is { };
        }

        public IReadOnlyList<ForbiddenName> GetGuildForbiddenNames(ulong gid)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            return this.InternalGetForbiddenNamesForGuild(db, gid).ToList().AsReadOnly();
        }

        public Task<bool> AddForbiddenNameAsync(ulong gid, string regexString)
        {
            return regexString.TryParseRegex(out Regex? regex)
                ? this.AddForbiddenNameAsync(gid, regex)
                : throw new ArgumentException($"Invalid regex string: {regexString}", nameof(regexString));
        }

        public async Task<bool> AddForbiddenNameAsync(ulong gid, Regex? regex)
        {
            if (regex is null)
                return false;

            string regexString = regex.ToString();

            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                IEnumerable<ForbiddenName> fnames = this.InternalGetForbiddenNamesForGuild(db, gid).AsEnumerable();
                if (fnames.Any(fname => string.Compare(fname.RegexString, regexString, true) == 0))
                    return false;
                var fname = new ForbiddenName {
                    GuildId = gid,
                    RegexString = regexString,
                    RegexLazy = regex,
                };
                db.ForbiddenNames.Add(fname);
                await db.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> AddForbiddenNamesAsync(ulong gid, IEnumerable<string> regexStrings)
        {
            bool[] res = await Task.WhenAll(regexStrings.Select(s => s.ToRegex()).Select(r => this.AddForbiddenNameAsync(gid, r)));
            return res.All(r => r);
        }

        public async Task<bool> AddForbiddenNamesAsync(ulong gid, IEnumerable<Regex> regexes)
        {
            bool[] res = await Task.WhenAll(regexes.Select(r => this.AddForbiddenNameAsync(gid, r)));
            return res.All(r => r);
        }

        public Task<int> RemoveForbiddenNamesAsync(ulong gid)
            => this.InternalRemoveByPredicateAsync(gid, _ => true);

        public Task<int> RemoveForbiddenNamesAsync(ulong gid, IEnumerable<int> ids)
            => this.InternalRemoveByPredicateAsync(gid, fn => ids.Contains(fn.Id));

        public Task<int> RemoveForbiddenNamesAsync(ulong gid, IEnumerable<string> regexStrings)
            => this.InternalRemoveByPredicateAsync(gid, fn => regexStrings.Any(rstr => string.Compare(rstr, fn.RegexString, true) == 0));

        public Task<int> RemoveForbiddenNamesMatchingAsync(ulong gid, string match)
            => this.InternalRemoveByPredicateAsync(gid, fn => fn.Regex.IsMatch(match));


        private IQueryable<ForbiddenName> InternalGetForbiddenNamesForGuild(TheGodfatherDbContext db, ulong gid)
            => db.ForbiddenNames.AsQueryable().Where(n => n.GuildIdDb == (long)gid);

        private async Task<int> InternalRemoveByPredicateAsync(ulong gid, Func<ForbiddenName, bool> predicate)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            var fnames = this.InternalGetForbiddenNamesForGuild(db, gid)
                .AsEnumerable()
                .Where(predicate)
                .ToList();
            db.ForbiddenNames.RemoveRange(fnames);
            await db.SaveChangesAsync();
            return fnames.Count;
        }
    }
}
