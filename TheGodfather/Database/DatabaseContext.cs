#region USING_DIRECTIVES
using Microsoft.EntityFrameworkCore;

using TheGodfather.Database.Entities;
#endregion

namespace TheGodfather.Database
{
    public class DatabaseContext : DbContext
    {
        public virtual DbSet<DatabaseBlockedChannel> BlockedChannels { get; set; }
        public virtual DbSet<DatabaseBlockedUser> BlockedUsers { get; set; }
        public virtual DbSet<DatabaseGuildConfig> GuildConfigurations { get; set; }

        private string ConnectionString { get; }


        public DatabaseContext(string cstring)
        {
            this.ConnectionString = cstring;
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            optionsBuilder.EnableSensitiveDataLogging(true);

            optionsBuilder.UseNpgsql(this.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder model)
        {
            model.HasDefaultSchema("gf");
        }
    }
}