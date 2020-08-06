using System.Linq;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class AutoRoleService : DbAbstractionServiceBase<AutoRole, ulong, ulong>
    {
        public override bool IsDisabled => false;


        public AutoRoleService(DbContextBuilder dbb) 
            : base(dbb) { }


        public override DbSet<AutoRole> DbSetSelector(TheGodfatherDbContext db) 
            => db.AutoRoles;
        
        public override IQueryable<AutoRole> GroupSelector(IQueryable<AutoRole> entities, ulong gid) 
            => entities.Where(ar => ar.GuildIdDb == (long)gid);
        
        public override AutoRole EntityFactory(ulong gid, ulong id) 
            => new AutoRole { GuildId = gid, RoleId = id };

        public override ulong EntityIdSelector(AutoRole entity) 
            => entity.RoleId;
    }
}
