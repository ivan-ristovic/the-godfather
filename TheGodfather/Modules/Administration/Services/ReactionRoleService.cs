using Microsoft.EntityFrameworkCore;

namespace TheGodfather.Modules.Administration.Services;

public sealed class ReactionRoleService : DbAbstractionServiceBase<ReactionRole, ulong, (string, ulong, ulong)>
{
    public override bool IsDisabled => false;


    public ReactionRoleService(DbContextBuilder dbb)
        : base(dbb) { }


    public override DbSet<ReactionRole> DbSetSelector(TheGodfatherDbContext db)
        => db.ReactionRoles;

    public override IQueryable<ReactionRole> GroupSelector(IQueryable<ReactionRole> rrs, ulong gid)
        => rrs.Where(ar => ar.GuildIdDb == (long)gid);

    public override ReactionRole EntityFactory(ulong gid, (string, ulong, ulong) tup)
        => new() { GuildId = gid, Emoji = tup.Item1, ChannelId = tup.Item2, MessageId = tup.Item3 };

    public override (string, ulong, ulong) EntityIdSelector(ReactionRole rr)
        => (rr.Emoji, rr.ChannelId, rr.MessageId);

    public override ulong EntityGroupSelector(ReactionRole rr)
        => rr.GuildId;

    public override object[] EntityPrimaryKeySelector(ulong gid, (string, ulong, ulong) tup)
        => new object[] { (long)gid, tup.Item1, (long)tup.Item2, (long)tup.Item3 };

    public async Task<ReactionRole?> GetAsync(ulong gid, string emoji, ulong cid, ulong mid)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        List<ReactionRole> rrs = await db.ReactionRoles
            .AsQueryable()
            .Where(rr => rr.GuildIdDb == (long)gid)
            .ToListAsync();
        rrs = rrs.Where(rr => string.Equals(rr.Emoji, emoji)).ToList();

        IEnumerable<ReactionRole> rrExplicit = rrs.Where(rr => rr.ChannelId != 0 && rr.MessageId != 0);
        if (rrExplicit.Any())
            return rrExplicit.FirstOrDefault(rr => rr.ChannelId == cid && rr.MessageId == mid);

        return rrs.FirstOrDefault(rr => rr.ChannelId == cid) ?? rrs.FirstOrDefault();
    }

    public Task<int> RemoveByEmojiAsync(ulong gid, IEnumerable<string> names)
        => this.InternalRemoveByPredicateAsync(gid, rr => names.Any(n => string.Equals(n, rr.Emoji)));

    public Task<int> RemoveByMessageAsync(ulong gid, IEnumerable<(ulong ChannelId, ulong MessageId)> ids)
        => this.InternalRemoveByPredicateAsync(gid, rr => ids.Any(tup => tup.ChannelId == rr.ChannelId && tup.MessageId == rr.MessageId));

    public Task<int> RemoveByChannelAsync(ulong gid, IEnumerable<ulong> ids)
        => this.InternalRemoveByPredicateAsync(gid, rr => ids.Any(cid => cid == rr.ChannelId));


    private async Task<int> InternalRemoveByPredicateAsync(ulong gid, Func<ReactionRole, bool> predicate)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        var rrs = db.ReactionRoles
            .AsQueryable()
            .Where(rr => rr.GuildIdDb == (long)gid)
            .AsEnumerable()
            .Where(predicate)
            .ToList();
        db.ReactionRoles.RemoveRange(rrs);
        await db.SaveChangesAsync();
        return rrs.Count;
    }
}