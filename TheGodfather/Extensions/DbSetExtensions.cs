﻿using Microsoft.EntityFrameworkCore;

namespace TheGodfather.Extensions;

public static class DbSetExtensions
{
    public static async Task<int> SafeAddRangeAsync<TEntity>(this DbSet<TEntity> set, IEnumerable<TEntity> entities, Func<TEntity, object[]> idSelector)
        where TEntity : class, IEquatable<TEntity>
    {
        int added = 0;
        foreach (TEntity entity in entities.Distinct()) {
            TEntity? dbEntity = await set.FindAsync(idSelector(entity));
            if (dbEntity is null) {
                set.Add(entity);
                added++;
            }
        }
        return added;
    }

    public static async Task<int> SafeRemoveRangeAsync<TEntity>(this DbSet<TEntity> set, IEnumerable<TEntity> entities, Func<TEntity, object[]> idSelector)
        where TEntity : class, IEquatable<TEntity>
    {
        int removed = 0;
        foreach (TEntity entity in entities.Distinct()) {
            TEntity? dbEntity = await set.FindAsync(idSelector(entity));
            if (dbEntity is not null) {
                set.Remove(dbEntity);
                removed++;
            }
        }
        return removed;
    }
}