using System;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database.Entities;

namespace TheGodfather.Database
{
    public class DatabaseContext : DbContext
    {
        public virtual DbSet<DatabaseAutoRole> AutoAssignableRoles { get; set; }
        public virtual DbSet<DatabaseBankAccount> BankAccounts { get; set; }
        public virtual DbSet<DatabaseBirthday> Birthdays { get; set; }
        public virtual DbSet<DatabaseChicken> Chickens { get; set; }
        public virtual DbSet<DatabaseChickenBoughtUpgrade> ChickensBoughtUpgrades { get; set; }
        public virtual DbSet<DatabaseChickenUpgrade> ChickenUpgrades { get; set; }
        public virtual DbSet<DatabaseCommandRule> CommandRules { get; set; }
        public virtual DbSet<DatabaseForbiddenName> ForbiddenNames { get; set; }
        public virtual DbSet<DatabaseGameStats> GameStats { get; set; }
        public virtual DbSet<DatabaseGuildRank> GuildRanks { get; set; }
        public virtual DbSet<DatabaseInsult> Insults { get; set; }
        public virtual DbSet<DatabaseMeme> Memes { get; set; }
        public virtual DbSet<DatabaseMessageCount> MessageCount { get; set; }
        public virtual DbSet<DatabasePrivilegedUser> PrivilegedUsers { get; set; }
        public virtual DbSet<DatabasePurchasableItem> PurchasableItems { get; set; }
        public virtual DbSet<DatabasePurchasedItem> PurchasedItems { get; set; }
        public virtual DbSet<DatabaseReminder> Reminders { get; set; }
        public virtual DbSet<DatabaseRssFeed> RssFeeds { get; set; }
        public virtual DbSet<DatabaseRssSubscription> RssSubscriptions { get; set; }
        public virtual DbSet<DatabaseSavedTask> SavedTasks { get; set; }
        public virtual DbSet<DatabaseSelfRole> SelfAssignableRoles { get; set; }
        public virtual DbSet<DatabaseSwatPlayer> SwatPlayers { get; set; }
        public virtual DbSet<DatabaseSwatServer> SwatServers { get; set; }

        private string ConnectionString { get; }
        private DbProvider Provider { get; }


        public DatabaseContext(DbProvider provider, string connectionString)
        {
            this.Provider = provider;
            this.ConnectionString = connectionString;
        }

        public DatabaseContext(DbProvider provider, string connectionString, DbContextOptions<DatabaseContext> options)
            : base(options)
        {
            this.Provider = provider;
            this.ConnectionString = connectionString;
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            //optionsBuilder.EnableSensitiveDataLogging(true);
            //optionsBuilder.UseLazyLoadingProxies();
            //optionsBuilder.ConfigureWarnings(wb => wb.Ignore(CoreEventId.DetachedLazyLoadingWarning));

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
                    throw new NotSupportedException("Provider not supported!");
            }
        }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.HasDefaultSchema("gf");

            mb.ForNpgsqlUseIdentityAlwaysColumns();

            mb.Entity<DatabaseAutoRole>().HasKey(e => new { e.GuildIdDb, e.RoleIdDb });
            mb.Entity<DatabaseBankAccount>().HasKey(e => new { e.GuildIdDb, e.UserIdDb });
            mb.Entity<DatabaseBirthday>().HasKey(e => new { e.GuildIdDb, e.ChannelIdDb, e.UserIdDb });
            mb.Entity<DatabaseChicken>().HasKey(e => new { e.GuildIdDb, e.UserIdDb });
            mb.Entity<DatabaseChickenBoughtUpgrade>().HasKey(e => new { e.Id, e.GuildIdDb, e.UserIdDb });
            mb.Entity<DatabaseChickenBoughtUpgrade>().HasOne(bu => bu.DbChickenUpgrade).WithMany(u => u.BoughtUpgrades).HasForeignKey(u => u.Id);
            mb.Entity<DatabaseChickenBoughtUpgrade>().HasOne(bu => bu.DbChicken).WithMany(u => u.DbUpgrades).HasForeignKey(bu => new { bu.GuildIdDb, bu.UserIdDb });
            mb.Entity<DatabaseCommandRule>().HasKey(e => new { e.GuildIdDb, e.ChannelIdDb, e.Command });
            mb.Entity<DatabaseGameStats>().Property(s => s.AnimalRacesWon).HasDefaultValue(0);
            mb.Entity<DatabaseGameStats>().Property(s => s.CaroLost).HasDefaultValue(0);
            mb.Entity<DatabaseGameStats>().Property(s => s.CaroWon).HasDefaultValue(0);
            mb.Entity<DatabaseGameStats>().Property(s => s.Chain4Lost).HasDefaultValue(0);
            mb.Entity<DatabaseGameStats>().Property(s => s.Chain4Won).HasDefaultValue(0);
            mb.Entity<DatabaseGameStats>().Property(s => s.DuelsLost).HasDefaultValue(0);
            mb.Entity<DatabaseGameStats>().Property(s => s.DuelsWon).HasDefaultValue(0);
            mb.Entity<DatabaseGameStats>().Property(s => s.HangmanWon).HasDefaultValue(0);
            mb.Entity<DatabaseGameStats>().Property(s => s.NumberRacesWon).HasDefaultValue(0);
            mb.Entity<DatabaseGameStats>().Property(s => s.OthelloLost).HasDefaultValue(0);
            mb.Entity<DatabaseGameStats>().Property(s => s.OthelloWon).HasDefaultValue(0);
            mb.Entity<DatabaseGameStats>().Property(s => s.QuizesWon).HasDefaultValue(0);
            mb.Entity<DatabaseGameStats>().Property(s => s.TicTacToeLost).HasDefaultValue(0);
            mb.Entity<DatabaseGameStats>().Property(s => s.TicTacToeWon).HasDefaultValue(0);
            mb.Entity<DatabaseGuildRank>().HasKey(e => new { e.GuildIdDb, e.Rank });
            mb.Entity<DatabaseMeme>().HasKey(e => new { e.GuildIdDb, e.Name });
            mb.Entity<DatabaseMessageCount>().Property(ui => ui.MessageCount).HasDefaultValue(1);
            mb.Entity<DatabasePurchasedItem>().HasKey(e => new { e.ItemId, e.UserIdDb });
            mb.Entity<DatabaseReminder>().Property(r => r.IsRepeating).HasDefaultValue(false);
            mb.Entity<DatabaseReminder>().Property(r => r.RepeatIntervalDb).HasDefaultValue(TimeSpan.FromMilliseconds(-1));
            mb.Entity<DatabaseRssSubscription>().HasKey(e => new { e.Id, e.GuildIdDb, e.ChannelIdDb });
            mb.Entity<DatabaseSelfRole>().HasKey(e => new { e.GuildIdDb, e.RoleIdDb });
            mb.Entity<DatabaseSwatPlayer>().Property(p => p.IsBlacklisted).HasDefaultValue(false);
            mb.Entity<DatabaseSwatPlayer>().HasIndex(p => p.Name).IsUnique();
            mb.Entity<DatabaseSwatPlayerAlias>().HasKey(p => new { p.Alias, p.PlayerId });
            mb.Entity<DatabaseSwatPlayerIP>().HasKey(p => new { p.IP, p.PlayerId });
            mb.Entity<DatabaseSwatServer>().HasKey(srv => new { srv.IP, srv.JoinPort, srv.QueryPort });
            mb.Entity<DatabaseSwatServer>().Property(srv => srv.JoinPort).HasDefaultValue(10480);
        }
    }
}