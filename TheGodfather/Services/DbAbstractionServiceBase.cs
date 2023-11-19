using Microsoft.EntityFrameworkCore;

namespace TheGodfather.Services;

public abstract class DbAbstractionServiceBase<TEntity, TEntityId> : ITheGodfatherService
    where TEntity : class, IEquatable<TEntity>
{
    protected readonly DbContextBuilder dbb;


    protected DbAbstractionServiceBase(DbContextBuilder dbb)
    {
        this.dbb = dbb;
    }


    public abstract bool IsDisabled { get; }
    public abstract DbSet<TEntity> DbSetSelector(TheGodfatherDbContext db);
    public abstract TEntity EntityFactory(TEntityId id);
    public abstract TEntityId EntityIdSelector(TEntity entity);
    public abstract object[] EntityPrimaryKeySelector(TEntityId id);


    public IEnumerable<TEntity> EntityFactory(IEnumerable<TEntityId> ids)
        => ids.Select(this.EntityFactory);

    public Task<int> AddAsync(params TEntityId[] ids)
        => this.AddAsync(idCollection: ids);

    public Task<int> AddAsync(IEnumerable<TEntityId> idCollection)
        => this.AddAsync(this.EntityFactory(idCollection));

    public Task<int> AddAsync(params TEntity[] entities)
        => this.AddAsync(collection: entities);

    public async Task<int> AddAsync(IEnumerable<TEntity> collection)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        int added = await this.DbSetSelector(db).SafeAddRangeAsync(
            collection,
            e => this.EntityPrimaryKeySelector(this.EntityIdSelector(e))
        );
        if (added > 0)
            await db.SaveChangesAsync();
        return added;
    }

    public Task<int> RemoveAsync(params TEntityId[] ids)
        => this.RemoveAsync(idCollection: ids);

    public Task<int> RemoveAsync(IEnumerable<TEntityId> idCollection)
        => this.RemoveAsync(this.EntityFactory(idCollection));

    public Task<int> RemoveAsync(params TEntity[] entities)
        => this.RemoveAsync(collection: entities);

    public async Task<int> RemoveAsync(IEnumerable<TEntity> collection)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        int removed = await this.DbSetSelector(db).SafeRemoveRangeAsync(
            collection,
            e => this.EntityPrimaryKeySelector(this.EntityIdSelector(e))
        );
        if (removed > 0)
            await db.SaveChangesAsync();
        return removed;
    }

    public async Task ClearAsync()
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        DbSet<TEntity> set = this.DbSetSelector(db);
        set.RemoveRange(set);
        await db.SaveChangesAsync();
    }

    public async Task<bool> ContainsAsync(TEntityId id)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        TEntity? entity = await this.DbSetSelector(db).FindAsync(this.EntityPrimaryKeySelector(id));
        return entity is not null;
    }

    public IReadOnlyList<TEntityId> GetIds()
    {
        List<TEntityId> res;
        using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
            res = this.DbSetSelector(db)
                .AsEnumerable()
                .Select(this.EntityIdSelector)
                .ToList();
        }
        return res.AsReadOnly();
    }

    public async Task<IReadOnlyList<TEntity>> GetAsync()
    {
        List<TEntity> res;
        await using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
            res = await this.DbSetSelector(db).AsQueryable().ToListAsync();
        }
        return res.AsReadOnly();
    }

    public async Task<TEntity?> GetAsync(TEntityId id)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        return await this.DbSetSelector(db).FindAsync(this.EntityPrimaryKeySelector(id));
    }
}

public abstract class DbAbstractionServiceBase<TEntity, TGroupId, TEntityId> : ITheGodfatherService
    where TEntity : class, IEquatable<TEntity>
{
    protected readonly DbContextBuilder dbb;


    public DbAbstractionServiceBase(DbContextBuilder dbb)
    {
        this.dbb = dbb;
    }


    public abstract bool IsDisabled { get; }
    public abstract DbSet<TEntity> DbSetSelector(TheGodfatherDbContext db);
    public abstract IQueryable<TEntity> GroupSelector(IQueryable<TEntity> entities, TGroupId grid);
    public abstract TEntity EntityFactory(TGroupId grid, TEntityId id);
    public abstract TEntityId EntityIdSelector(TEntity entity);
    public abstract TGroupId EntityGroupSelector(TEntity entity);
    public abstract object[] EntityPrimaryKeySelector(TGroupId grid, TEntityId id);


    public IEnumerable<TEntity> EntityFactory(TGroupId gid, IEnumerable<TEntityId> ids)
        => ids.Select(id => this.EntityFactory(gid, id));

    public Task<int> AddAsync(TGroupId grid, params TEntityId[] ids)
        => this.AddAsync(grid, idCollection: ids);

    public Task<int> AddAsync(TGroupId grid, IEnumerable<TEntityId> idCollection)
        => this.AddAsync(this.EntityFactory(grid, idCollection));

    public Task<int> AddAsync(params TEntity[] entities)
        => this.AddAsync(collection: entities);

    public async Task<int> AddAsync(IEnumerable<TEntity> collection)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        int added = await this.DbSetSelector(db).SafeAddRangeAsync(
            collection,
            e => this.EntityPrimaryKeySelector(this.EntityGroupSelector(e), this.EntityIdSelector(e))
        );
        if (added > 0)
            await db.SaveChangesAsync();
        return added;
    }

    public Task<int> RemoveAsync(TGroupId grid, params TEntityId[] ids)
        => this.RemoveAsync(grid, idCollection: ids);

    public Task<int> RemoveAsync(TGroupId grid, IEnumerable<TEntityId> idCollection)
        => this.RemoveAsync(this.EntityFactory(grid, idCollection));

    public Task<int> RemoveAsync(params TEntity[] entities)
        => this.RemoveAsync(collection: entities);

    public async Task<int> RemoveAsync(IEnumerable<TEntity> collection)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        int removed = await this.DbSetSelector(db).SafeRemoveRangeAsync(
            collection,
            e => this.EntityPrimaryKeySelector(this.EntityGroupSelector(e), this.EntityIdSelector(e))
        );
        if (removed > 0)
            await db.SaveChangesAsync();
        return removed;
    }

    public async Task ClearAsync(TGroupId grid)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        DbSet<TEntity> set = this.DbSetSelector(db);
        set.RemoveRange(this.GroupSelector(set, grid));
        await db.SaveChangesAsync();
    }

    public async Task<bool> ContainsAsync(TGroupId grid, TEntityId id)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        TEntity? entity = await this.DbSetSelector(db).FindAsync(this.EntityPrimaryKeySelector(grid, id));
        return entity is not null;
    }

    public IReadOnlyList<TEntityId> GetIds(TGroupId grid)
    {
        List<TEntityId> res;
        using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
            DbSet<TEntity> set = this.DbSetSelector(db);
            res = this.GroupSelector(set, grid)
                .AsEnumerable()
                .Select(this.EntityIdSelector)
                .ToList();
        }
        return res.AsReadOnly();
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(TGroupId grid)
    {
        List<TEntity> res;
        await using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
            DbSet<TEntity> set = this.DbSetSelector(db);
            res = await this.GroupSelector(set, grid).ToListAsync();
        }
        return res.AsReadOnly();
    }

    public async Task<TEntity?> GetAsync(TGroupId grid, TEntityId entityId)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        DbSet<TEntity> set = this.DbSetSelector(db);
        return await set.FindAsync(this.EntityPrimaryKeySelector(grid, entityId));
    }
}