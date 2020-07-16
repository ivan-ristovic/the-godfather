using System;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database.Models;
using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.Database
{
    public class TheGodfatherDbContext : DbContext
    {
        public virtual DbSet<AutoRole> AutoAssignableRoles { get; protected set; }
        public virtual DbSet<BankAccount> BankAccounts { get; protected set; }
        public virtual DbSet<Birthday> Birthdays { get; protected set; }
        public virtual DbSet<BlockedChannel> BlockedChannels { get; protected set; }
        public virtual DbSet<BlockedUser> BlockedUsers { get; protected set; }
        public virtual DbSet<BotStatus> BotStatuses { get; protected set; }
        public virtual DbSet<Chicken> Chickens { get; protected set; }
        public virtual DbSet<ChickenUpgrade> ChickenUpgrades { get; protected set; }
        public virtual DbSet<ChickenBoughtUpgrade> ChickensBoughtUpgrades { get; protected set; }
        public virtual DbSet<CommandRule> CommandRules { get; protected set; }
        public virtual DbSet<GameStats> GameStats { get; protected set; }
        public virtual DbSet<GuildConfig> Configs { get; protected set; }
        public virtual DbSet<EmojiReaction> EmojiReactions { get; protected set; }
        public virtual DbSet<ExemptedAntispamEntity> ExemptsAntispam { get; protected set; }
        public virtual DbSet<ExemptedLoggingEntity> ExemptsLogging { get; protected set; }
        public virtual DbSet<ExemptedRatelimitEntity> ExemptsRatelimit { get; protected set; }
        public virtual DbSet<Filter> Filters { get; protected set; }
        public virtual DbSet<ForbiddenName> ForbiddenNames { get; protected set; }
        public virtual DbSet<GuildTask> GuildTasks { get; protected set; }
        public virtual DbSet<Insult> Insults { get; protected set; }
        public virtual DbSet<Meme> Memes { get; protected set; }
        public virtual DbSet<PurchasableItem> PurchasableItems { get; protected set; }
        public virtual DbSet<PurchasedItem> PurchasedItems { get; protected set; }
        public virtual DbSet<PrivilegedUser> PrivilegedUsers { get; protected set; }
        public virtual DbSet<Reminder> Reminders { get; protected set; }
        public virtual DbSet<RssFeed> RssFeeds { get; protected set; }
        public virtual DbSet<RssSubscription> RssSubscriptions { get; protected set; }
        public virtual DbSet<SelfRole> SelfAssignableRoles { get; protected set; }
        public virtual DbSet<SwatPlayer> SwatPlayers { get; set; }
        public virtual DbSet<SwatServer> SwatServers { get; protected set; }
        public virtual DbSet<TextReaction> TextReactions { get; protected set; }
        public virtual DbSet<XpCount> XpCounts { get; protected set; }
        public virtual DbSet<XpRank> XpRanks { get; protected set; }


        private DbProvider Provider { get; }
        private string ConnectionString { get; }


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

            optionsBuilder.UseLazyLoadingProxies();

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

            mb.Entity<AutoRole>().HasKey(ar => new { ar.GuildIdDb, ar.RoleIdDb });
            mb.Entity<BankAccount>().HasKey(acc => new { acc.GuildIdDb, acc.UserIdDb });
            mb.Entity<Birthday>().HasKey(b => new { b.GuildIdDb, b.ChannelIdDb, b.UserIdDb });
            mb.Entity<BlockedChannel>().Property(bc => bc.Reason).HasDefaultValue(null);
            mb.Entity<BlockedUser>().Property(bu => bu.Reason).HasDefaultValue(null);
            mb.Entity<Chicken>().HasKey(c => new { c.GuildIdDb, c.UserIdDb });
            mb.Entity<ChickenBoughtUpgrade>().HasKey(bu => new { bu.Id, bu.GuildIdDb, bu.UserIdDb });
            mb.Entity<ChickenBoughtUpgrade>().HasOne(bu => bu.Upgrade).WithMany(u => u.BoughtUpgrades).HasForeignKey(u => u.Id);
            mb.Entity<ChickenBoughtUpgrade>().HasOne(bu => bu.Chicken).WithMany(u => u.Upgrades).HasForeignKey(bu => new { bu.GuildIdDb, bu.UserIdDb });
            mb.Entity<CommandRule>().HasKey(e => new { e.GuildIdDb, e.ChannelIdDb, e.Command });
            mb.Entity<EmojiReactionTrigger>().HasKey(t => new { t.ReactionId, t.Trigger });
            mb.Entity<ExemptedAntispamEntity>().HasKey(e => new { e.IdDb, e.GuildIdDb, e.Type });
            mb.Entity<ExemptedLoggingEntity>().HasKey(e => new { e.IdDb, e.GuildIdDb, e.Type });
            mb.Entity<ExemptedRatelimitEntity>().HasKey(e => new { e.IdDb, e.GuildIdDb, e.Type });
            mb.Entity<GameStats>().Property(s => s.AnimalRacesWon).HasDefaultValue(0);
            mb.Entity<GameStats>().Property(s => s.CaroLost).HasDefaultValue(0);
            mb.Entity<GameStats>().Property(s => s.CaroWon).HasDefaultValue(0);
            mb.Entity<GameStats>().Property(s => s.Chain4Lost).HasDefaultValue(0);
            mb.Entity<GameStats>().Property(s => s.Chain4Won).HasDefaultValue(0);
            mb.Entity<GameStats>().Property(s => s.DuelLost).HasDefaultValue(0);
            mb.Entity<GameStats>().Property(s => s.DuelWon).HasDefaultValue(0);
            mb.Entity<GameStats>().Property(s => s.HangmanWon).HasDefaultValue(0);
            mb.Entity<GameStats>().Property(s => s.NumberRacesWon).HasDefaultValue(0);
            mb.Entity<GameStats>().Property(s => s.OthelloLost).HasDefaultValue(0);
            mb.Entity<GameStats>().Property(s => s.OthelloWon).HasDefaultValue(0);
            mb.Entity<GameStats>().Property(s => s.QuizWon).HasDefaultValue(0);
            mb.Entity<GameStats>().Property(s => s.TicTacToeLost).HasDefaultValue(0);
            mb.Entity<GameStats>().Property(s => s.TicTacToeWon).HasDefaultValue(0);
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
            mb.Entity<Meme>().HasKey(m => new { m.GuildIdDb, m.Name });
            mb.Entity<PurchasedItem>().HasKey(i => new { i.ItemId, i.UserIdDb });
            mb.Entity<Reminder>().Property(r => r.IsRepeating).HasDefaultValue(false);
            mb.Entity<Reminder>().Property(r => r.RepeatIntervalDb).HasDefaultValue(TimeSpan.FromMilliseconds(-1));
            mb.Entity<RssSubscription>().HasKey(sub => new { sub.Id, sub.GuildIdDb, sub.ChannelIdDb });
            mb.Entity<SelfRole>().HasKey(sr => new { sr.GuildIdDb, sr.RoleIdDb });
            mb.Entity<SwatPlayer>().Property(p => p.IsBlacklisted).HasDefaultValue(false);
            mb.Entity<SwatPlayer>().HasIndex(p => p.Name).IsUnique();
            mb.Entity<SwatPlayerAlias>().HasKey(p => new { p.Alias, p.PlayerId });
            mb.Entity<SwatPlayerIP>().HasKey(p => new { p.IP, p.PlayerId });
            mb.Entity<SwatServer>().HasKey(srv => new { srv.IP, srv.JoinPort, srv.QueryPort });
            mb.Entity<SwatServer>().Property(srv => srv.JoinPort).HasDefaultValue(10480);
            mb.Entity<TextReactionTrigger>().HasKey(t => new { t.ReactionId, t.Trigger });
            mb.Entity<XpCount>().Property(xpc => xpc.XpDb).HasDefaultValue(1);
            mb.Entity<XpRank>().HasKey(xpr => new { xpr.GuildIdDb, xpr.Rank });
        }
    }
}
