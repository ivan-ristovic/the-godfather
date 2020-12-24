using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Modules.Currency.Services
{
    public sealed class ShopService : DbAbstractionServiceBase<PurchasableItem, ulong, int>
    {
        public override bool IsDisabled => false;
        
        public PurchasedItemsService Purchases { get; }


        public ShopService(DbContextBuilder dbb)
            : base(dbb) 
        {
            this.Purchases = new PurchasedItemsService(dbb);
        }


        public override DbSet<PurchasableItem> DbSetSelector(TheGodfatherDbContext db) 
            => db.PurchasableItems;

        public override IQueryable<PurchasableItem> GroupSelector(IQueryable<PurchasableItem> entities, ulong grid)
            => entities.Where(i => i.GuildIdDb == (long)grid);

        public override PurchasableItem EntityFactory(ulong grid, int id)
            => new PurchasableItem { GuildId = grid, Id = id };

        public override int EntityIdSelector(PurchasableItem entity) 
            => entity.Id;

        public override ulong EntityGroupSelector(PurchasableItem entity)
            => entity.GuildId;

        public override object[] EntityPrimaryKeySelector(ulong grid, int id)
            => new object[] { id };


        public sealed class PurchasedItemsService : DbAbstractionServiceBase<PurchasedItem, ulong, int>
        {
            public override bool IsDisabled => false;


            public PurchasedItemsService(DbContextBuilder dbb)
                : base(dbb) { }


            public override DbSet<PurchasedItem> DbSetSelector(TheGodfatherDbContext db)
                => db.PurchasedItems;

            public override PurchasedItem EntityFactory(ulong uid, int id)
                => new PurchasedItem { ItemId = id, UserId = uid };

            public override ulong EntityGroupSelector(PurchasedItem entity)
                => entity.UserId;

            public override int EntityIdSelector(PurchasedItem entity)
                => entity.ItemId;

            public override object[] EntityPrimaryKeySelector(ulong uid, int id)
                => new object[] { id, (long)uid };

            public override IQueryable<PurchasedItem> GroupSelector(IQueryable<PurchasedItem> entities, ulong uid)
                => entities.Where(i => i.UserIdDb == (long)uid);

            public async Task<IReadOnlyList<PurchasedItem>> GetAllCompleteAsync(ulong uid)
            {
                List<PurchasedItem> res;
                using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                    DbSet<PurchasedItem> set = this.DbSetSelector(db);
                    res = await this.GroupSelector(set, uid)
                        .Include(i => i.Item)
                        .ToListAsync();
                }
                return res.AsReadOnly();
            }
        }
    }
}
