using System.Linq;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class SelfRoleService : DbAbstractionServiceBase<SelfRole, ulong, ulong>
    {
        public override bool IsDisabled => false;


        public SelfRoleService(DbContextBuilder dbb) 
            : base(dbb) { }


        public override DbSet<SelfRole> DbSetSelector(TheGodfatherDbContext db) 
            => db.SelfRoles;
        
        public override IQueryable<SelfRole> GroupSelector(IQueryable<SelfRole> srs, ulong gid) 
            => srs.Where(sr => sr.GuildIdDb == (long)gid);
        
        public override SelfRole EntityFactory(ulong gid, ulong rid) 
            => new SelfRole { GuildId = gid, RoleId = rid };

        public override ulong EntityIdSelector(SelfRole sr)
            => sr.RoleId;

        public override ulong EntityGroupSelector(SelfRole sr)
            => sr.GuildId;

        public override object[] EntityPrimaryKeySelector(ulong gid, ulong rid) 
            => new object[] { (long)gid, (long)rid };
    }
}
