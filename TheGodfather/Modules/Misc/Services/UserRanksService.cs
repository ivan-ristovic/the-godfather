using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Common.Collections;

namespace TheGodfather.Modules.Misc.Services;

public sealed class UserRanksService : DbAbstractionServiceBase<XpCount, ulong, ulong>
{
    public override bool IsDisabled => false;

    private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, int>> xps;
    private readonly ConcurrentDictionary<ulong, ConcurrentHashSet<ulong>> modified;


    public UserRanksService(DbContextBuilder dbb, bool loadData = true)
        : base(dbb)
    {
        this.xps = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, int>>();
        this.modified = new ConcurrentDictionary<ulong, ConcurrentHashSet<ulong>>();
        if (loadData)
            this.LoadData();
    }


    public void LoadData()
    {
        using TheGodfatherDbContext db = this.dbb.CreateContext();
        this.xps = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, int>>(
            db.XpCounts
                .AsEnumerable()
                .GroupBy(xpc => xpc.GuildId)
                .ToDictionary(g => g.Key, g => new ConcurrentDictionary<ulong, int>(g.ToDictionary(g1 => g1.UserId, g1 => g1.Xp)))
        );
    }

    public static short CalculateRankForXp(int msgcount)
        => (short)Math.Floor(Math.Sqrt(msgcount / 10));

    public static int CalculateXpNeededForRank(short index)
        => index * index * 10;

    public short CalculateRankForUser(ulong gid, ulong uid)
        => this.xps.GetOrAdd(gid, new ConcurrentDictionary<ulong, int>()).TryGetValue(uid, out int count) ? CalculateRankForXp(count) : (short)0;

    public int GetUserXp(ulong? gid, ulong uid)
    {
        if (gid is { })
            return this.xps.GetOrAdd(gid.Value, new ConcurrentDictionary<ulong, int>()).TryGetValue(uid, out int count) ? count : 0;
        return this.xps.Sum(kvp => kvp.Value.GetValueOrDefault(uid));

    }

    public short ChangeXp(ulong gid, ulong uid, int change = 1)
    {
        this.xps.GetOrAdd(gid, new ConcurrentDictionary<ulong, int>()).AddOrUpdate(uid, 1, (_, xp) => xp + change);
        this.modified.GetOrAdd(gid, new ConcurrentHashSet<ulong>()).Add(uid);

        short prev = CalculateRankForXp(this.xps[gid][uid] - change);
        short curr = CalculateRankForXp(this.xps[gid][uid]);

        return curr != prev ? curr : (short)0;
    }

    public async Task Sync()
    {
        if (this.modified.Any()) {
            await using TheGodfatherDbContext db = this.dbb.CreateContext();
            foreach ((ulong gid, ConcurrentHashSet<ulong> uids) in this.modified) {
                foreach (ulong uid in uids) {
                    XpCount? xpc = await db.XpCounts.FindAsync((long)gid, (long)uid);
                    if (xpc is null) {
                        db.XpCounts.Add(new XpCount { GuildId = gid, UserId = uid, Xp = this.xps[gid].GetValueOrDefault(uid) });
                    } else {
                        xpc.Xp = this.xps[gid].GetValueOrDefault(uid);
                        db.XpCounts.Update(xpc);
                    }
                }
                uids.Clear();
            }
            await db.SaveChangesAsync();
        }
    }

    public async Task<IReadOnlyList<XpCount>> GetTopRankedUsersAsync(ulong? gid = null, int count = 10)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        return gid is null
            ? await this.DbSetSelector(db).AsQueryable().OrderByDescending(r => r.Xp).Take(count).ToListAsync()
            : await this.GroupSelector(this.DbSetSelector(db), gid.Value).OrderByDescending(r => r.Xp).Take(count).ToListAsync();
    }

    public async Task RemoveDeletedUsers(IEnumerable<ulong> uids)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        foreach (ulong uid in uids)
            this.DbSetSelector(db).RemoveRange(this.DbSetSelector(db).AsQueryable().Where(xpc => xpc.UserIdDb == (long)uid));
        await db.SaveChangesAsync();
    }

    public override DbSet<XpCount> DbSetSelector(TheGodfatherDbContext db) => db.XpCounts;
    public override XpCount EntityFactory(ulong gid, ulong uid) => new() { GuildId = gid, UserId = uid };
    public override ulong EntityIdSelector(XpCount entity) => entity.UserId;
    public override object[] EntityPrimaryKeySelector(ulong gid, ulong uid) => new object[] { (long)gid, (long)uid };
    public override IQueryable<XpCount> GroupSelector(IQueryable<XpCount> entities, ulong gid) => entities.Where(xpc => xpc.GuildIdDb == (long)gid);
    public override ulong EntityGroupSelector(XpCount entity) => entity.GuildId;
}