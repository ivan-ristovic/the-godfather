using System;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database.Entities;

namespace TheGodfather.Database
{
    public class DatabaseContext : DbContext
    {
        public virtual DbSet<DatabaseAutoRole> AutoAssignableRoles { get; set; }
        public virtual DbSet<DatabaseForbiddenName> ForbiddenNames { get; set; }
        public virtual DbSet<DatabaseGameStats> GameStats { get; set; }
        public virtual DbSet<DatabaseInsult> Insults { get; set; }
        public virtual DbSet<DatabaseMeme> Memes { get; set; }
        public virtual DbSet<DatabasePrivilegedUser> PrivilegedUsers { get; set; }
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
            mb.Entity<DatabaseMeme>().HasKey(e => new { e.GuildIdDb, e.Name });
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