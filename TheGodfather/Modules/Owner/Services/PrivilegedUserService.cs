using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Modules.Owner.Services
{
    public sealed class PrivilegedUserService : DbAbstractionServiceBase<PrivilegedUser, ulong>
    {
        public override bool IsDisabled => false;


        public PrivilegedUserService(DbContextBuilder dbb)
            : base(dbb) { }

        public override DbSet<PrivilegedUser> DbSetSelector(TheGodfatherDbContext db) => db.PrivilegedUsers;
        public override PrivilegedUser EntityFactory(ulong id) => new PrivilegedUser { UserId = id };
        public override ulong EntityIdSelector(PrivilegedUser entity) => entity.UserId;
        public override object[] EntityPrimaryKeySelector(ulong id) => new object[] { id };
    }
}
