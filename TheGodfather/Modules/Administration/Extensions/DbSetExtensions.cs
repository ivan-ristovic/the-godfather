using DSharpPlus.Entities;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

using TheGodfather.Database.Entities;
using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.Modules.Administration.Extensions
{
    public static class ExemptedEntityExtensions
    {
        public static void SafeAddRange<TEntity>(this DbSet<TEntity> set, IEnumerable<TEntity> entities) 
            where TEntity : class, IEquatable<TEntity>
        {
            set.AddRange(entities.Except(set));
        }

        public static void AddExemptions<TEntity, TExempt>(this DbSet<TEntity> set, ulong gid, IEnumerable<TExempt> exempts, ExemptedEntityType type) 
            where TEntity : DatabaseExemptedEntity, new()
            where TExempt : SnowflakeObject
        {
            set.AddRange(exempts
                .Where(e => !set.Where(dbe => dbe.GuildId == gid).Any(dbe => dbe.Type == type && dbe.Id == e.Id))
                .Select(e => new TEntity() {
                    GuildId = gid,
                    Id = e.Id,
                    Type = ExemptedEntityType.Channel
                })
            );
        }
    }
}
