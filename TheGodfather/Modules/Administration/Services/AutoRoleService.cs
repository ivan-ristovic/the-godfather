using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class AutoRoleService : ITheGodfatherService
    {
        public bool IsDisabled => false;

        private readonly DbContextBuilder dbb;


        public AutoRoleService(DbContextBuilder dbb)
        {
            this.dbb = dbb;
        }


        public async Task AddAsync(ulong gid, IEnumerable<ulong> rids)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                db.AutoRoles.SafeAddRange(
                    rids.Select(rid => new AutoRole {
                        RoleId = rid,
                        GuildId = gid
                    })
                );
                await db.SaveChangesAsync();
            }
        }

        public async Task RemoveAsync(ulong gid, IEnumerable<ulong> rids)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                db.AutoRoles.RemoveRange(
                    db.AutoRoles
                      .Where(ar => ar.GuildIdDb == (long)gid)
                      .AsEnumerable()
                      .Where(r => rids.Contains(r.RoleId))
                );
                await db.SaveChangesAsync();
            }
        }

        public async Task ClearAsync(ulong gid)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                db.AutoRoles.RemoveRange(db.AutoRoles.Where(ar => ar.GuildIdDb == (long)gid));
                await db.SaveChangesAsync();
            }
        }

        public async Task<IReadOnlyList<ulong>> GetRolesForGuildAsync(ulong gid)
        {
            List<ulong> rids;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                rids = await db.AutoRoles
                               .Where(r => r.GuildIdDb == (long)gid)
                               .Select(r => r.RoleId)
                               .ToListAsync()
                               ;
            }
            return rids.AsReadOnly();
        }
    }
}
