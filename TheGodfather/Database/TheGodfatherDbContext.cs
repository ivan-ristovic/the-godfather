using System;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database.Models;

namespace TheGodfather.Database
{
    public class TheGodfatherDbContext : DbContext
    {
        public virtual DbSet<BlockedChannel> BlockedChannels { get; protected set; } 
        public virtual DbSet<BlockedUser> BlockedUsers { get; protected set; }
        public virtual DbSet<BotStatus> BotStatuses { get; protected set; }

        private string ConnectionString { get; }
        private DbProvider Provider { get; }


#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public TheGodfatherDbContext(DbProvider provider, string connectionString)
        {
            this.Provider = provider;
            this.ConnectionString = connectionString;
        }

        public TheGodfatherDbContext(DbProvider provider, string connectionString, DbContextOptions<TheGodfatherDbContext> options)
            : base(options)
        {
            this.Provider = provider;
            this.ConnectionString = connectionString;
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            switch (this.Provider) {
                case DbProvider.PostgreSql:
                    optionsBuilder.UseNpgsql(this.ConnectionString);
                    break;
                case DbProvider.Sqlite:
                case DbProvider.SqliteInMemory:
                    optionsBuilder.UseSqlite(this.ConnectionString);
                    break;
                case DbProvider.SqlServer:
                    optionsBuilder.UseSqlServer(this.ConnectionString);
                    break;
                default:
                    throw new NotSupportedException("Selected database provider not supported!");
            }
        }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.HasDefaultSchema("gf");
        }
    }
}
