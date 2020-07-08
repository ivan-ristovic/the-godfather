using System;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database.Models;
using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.Database
{
    public class TheGodfatherDbContext : DbContext
    {
        public virtual DbSet<BlockedChannel> BlockedChannels { get; protected set; } 
        public virtual DbSet<BlockedUser> BlockedUsers { get; protected set; }
        public virtual DbSet<BotStatus> BotStatuses { get; protected set; }
        public virtual DbSet<GuildConfig> GuildConfigs { get; protected set; }
        public virtual DbSet<Filter> Filters { get; protected set; }

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

            mb.Entity<BlockedChannel>().Property(bc => bc.Reason).HasDefaultValue(null);
            mb.Entity<BlockedUser>().Property(bu => bu.Reason).HasDefaultValue(null);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.AntifloodAction).HasDefaultValue(PunishmentAction.PermanentBan);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.AntifloodCooldown).HasDefaultValue(10);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.AntifloodEnabled).HasDefaultValue(false);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.AntifloodSensitivity).HasDefaultValue(5);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.AntiInstantLeaveCooldown).HasDefaultValue(3);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.AntiInstantLeaveEnabled).HasDefaultValue(false);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.AntispamAction).HasDefaultValue(PunishmentAction.PermanentMute);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.AntispamEnabled).HasDefaultValue(false);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.AntispamSensitivity).HasDefaultValue(5);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.Currency).HasDefaultValue(null);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.LogChannelIdDb).HasDefaultValue();
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.MuteRoleIdDb).HasDefaultValue(null);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.LeaveChannelIdDb).HasDefaultValue(null);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.LeaveMessage).HasDefaultValue(null);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.LinkfilterBootersEnabled).HasDefaultValue(true);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.LinkfilterDiscordInvitesEnabled).HasDefaultValue(false);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.LinkfilterDisturbingWebsitesEnabled).HasDefaultValue(true);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.LinkfilterEnabled).HasDefaultValue(false);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.LinkfilterIpLoggersEnabled).HasDefaultValue(true);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.LinkfilterUrlShortenersEnabled).HasDefaultValue(true);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.Prefix).HasDefaultValue(null);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.RatelimitAction).HasDefaultValue(PunishmentAction.TemporaryMute);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.RatelimitEnabled).HasDefaultValue(false);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.RatelimitSensitivity).HasDefaultValue(5);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.ReactionResponse).HasDefaultValue(false);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.SuggestionsEnabled).HasDefaultValue(false);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.WelcomeChannelIdDb).HasDefaultValue(null);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.WelcomeMessage).HasDefaultValue(null);
        }
    }
}
