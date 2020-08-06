using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;

namespace TheGodfather.Services
{
    public abstract class DbAbstractionServiceBase<TEntity, TEntityId> : ITheGodfatherService
        where TEntity : class, IEquatable<TEntity>
    {
        protected readonly DbContextBuilder dbb;


        public DbAbstractionServiceBase(DbContextBuilder dbb)
        {
            this.dbb = dbb;
        }


        public abstract bool IsDisabled { get; }
        public abstract DbSet<TEntity> DbSetSelector(TheGodfatherDbContext db);
        public abstract TEntity EntityFactory(TEntityId id);
        public abstract TEntityId EntityIdSelector(TEntity entity);
        public abstract object[] EntityPrimaryKeySelector(TEntityId id);


        public IEnumerable<TEntity> EntityFactory(IEnumerable<TEntityId> ids)
            => ids.Select(id => this.EntityFactory(id));

        public async Task AddAsync(IEnumerable<TEntityId> ids)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                DbSet<TEntity> set = this.DbSetSelector(db);
                set.AddRange(this.EntityFactory(ids).Except(set));
                await db.SaveChangesAsync();
            }
        }

        public Task RemoveAsync(params TEntityId[] ids)
            => ids is { } ? this.RemoveAsync(idCollection: ids) : Task.CompletedTask;

        public async Task RemoveAsync(IEnumerable<TEntityId> idCollection)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                this.DbSetSelector(db).RemoveRange(this.EntityFactory(idCollection));
                await db.SaveChangesAsync();
            }
        }

        public async Task ClearAsync()
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                DbSet<TEntity> set = this.DbSetSelector(db);
                set.RemoveRange(set);
                await db.SaveChangesAsync();
            }
        }

        public async Task<bool> ContainsAsync(TEntityId id)
        {
            TEntity? entity = null;
            using (TheGodfatherDbContext db = this.dbb.CreateContext())
                entity = await this.DbSetSelector(db).FindAsync(this.EntityPrimaryKeySelector(id));
            return entity is { };
        }

        public IReadOnlyList<TEntityId> Get()
        {
            List<TEntityId> rids;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                DbSet<TEntity> set = this.DbSetSelector(db);
                rids = set
                    .AsEnumerable()
                    .Select(this.EntityIdSelector)
                    .ToList();
            }
            return rids.AsReadOnly();
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
        public abstract IQueryable<TEntity> GroupSelector(IQueryable<TEntity> entities, TGroupId gid);
        public abstract TEntity EntityFactory(TGroupId gid, TEntityId id);
        public abstract TEntityId EntityIdSelector(TEntity entity);
        public abstract object[] EntityPrimaryKeySelector(TGroupId gid, TEntityId id);


        public IEnumerable<TEntity> EntityFactory(TGroupId gid, IEnumerable<TEntityId> ids)
            => ids.Select(id => this.EntityFactory(gid, id));

        public async Task AddAsync(TGroupId gid, IEnumerable<TEntityId> ids)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                DbSet<TEntity> set = this.DbSetSelector(db);
                set.AddRange(this.EntityFactory(gid, ids).Except(set));
                await db.SaveChangesAsync();
            }
        }

        public Task RemoveAsync(TGroupId gid, params TEntityId[] ids)
            => ids is { } ? this.RemoveAsync(gid, idCollection: ids) : Task.CompletedTask;

        public async Task RemoveAsync(TGroupId gid, IEnumerable<TEntityId> idCollection)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                this.DbSetSelector(db).RemoveRange(this.EntityFactory(gid, idCollection));
                await db.SaveChangesAsync();
            }
        }

        public async Task ClearAsync(TGroupId gid)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                DbSet<TEntity> set = this.DbSetSelector(db);
                set.RemoveRange(this.GroupSelector(set, gid));
                await db.SaveChangesAsync();
            }
        }

        public async Task<bool> ContainsAsync(TGroupId gid, TEntityId id)
        {
            TEntity? entity = null;
            using (TheGodfatherDbContext db = this.dbb.CreateContext())
                entity = await this.DbSetSelector(db).FindAsync(this.EntityPrimaryKeySelector(gid, id));
            return entity is { };
        }

        public IReadOnlyList<TEntityId> Get(TGroupId gid)
        {
            List<TEntityId> rids;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                DbSet<TEntity> set = this.DbSetSelector(db);
                rids = this.GroupSelector(set, gid)
                    .AsEnumerable()
                    .Select(this.EntityIdSelector)
                    .ToList();
            }
            return rids.AsReadOnly();
        }
    }
}
