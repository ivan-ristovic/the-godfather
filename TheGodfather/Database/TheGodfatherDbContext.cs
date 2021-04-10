using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TheGodfather.Database.Models;
using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.Database
{
    public class TheGodfatherDbContext : DbContext
    {
        #region db sets
        public virtual DbSet<ActionHistoryEntry> ActionHistory { get; protected set; }
        public virtual DbSet<AutoRole> AutoRoles { get; protected set; }
        public virtual DbSet<BankAccount> BankAccounts { get; protected set; }
        public virtual DbSet<Birthday> Birthdays { get; protected set; }
        public virtual DbSet<BlockedChannel> BlockedChannels { get; protected set; }
        public virtual DbSet<BlockedGuild> BlockedGuilds { get; protected set; }
        public virtual DbSet<BlockedUser> BlockedUsers { get; protected set; }
        public virtual DbSet<BotStatus> BotStatuses { get; protected set; }
        public virtual DbSet<Chicken> Chickens { get; protected set; }
        public virtual DbSet<ChickenUpgrade> ChickenUpgrades { get; protected set; }
        public virtual DbSet<ChickenBoughtUpgrade> ChickensBoughtUpgrades { get; protected set; }
        public virtual DbSet<CommandRule> CommandRules { get; protected set; }
        public virtual DbSet<GameStats> GameStats { get; protected set; }
        public virtual DbSet<GuildConfig> Configs { get; protected set; }
        public virtual DbSet<EmojiReaction> EmojiReactions { get; protected set; }
        public virtual DbSet<ExemptedSpamEntity> ExemptsSpam { get; protected set; }
        public virtual DbSet<ExemptedBackupEntity> ExemptsBackup { get; protected set; }
        public virtual DbSet<ExemptedLoggingEntity> ExemptsLogging { get; protected set; }
        public virtual DbSet<ExemptedMentionEntity> ExemptsMention { get; protected set; }
        public virtual DbSet<ExemptedRatelimitEntity> ExemptsRatelimit { get; protected set; }
        public virtual DbSet<Filter> Filters { get; protected set; }
        public virtual DbSet<ForbiddenName> ForbiddenNames { get; protected set; }
        public virtual DbSet<GuildTask> GuildTasks { get; protected set; }
        public virtual DbSet<LevelRole> LevelRoles { get; protected set; }
        public virtual DbSet<Meme> Memes { get; protected set; }
        public virtual DbSet<PurchasableItem> PurchasableItems { get; protected set; }
        public virtual DbSet<PurchasedItem> PurchasedItems { get; protected set; }
        public virtual DbSet<PrivilegedUser> PrivilegedUsers { get; protected set; }
        public virtual DbSet<ReactionRole> ReactionRoles { get; protected set; }
        public virtual DbSet<Reminder> Reminders { get; protected set; }
        public virtual DbSet<RssFeed> RssFeeds { get; protected set; }
        public virtual DbSet<RssSubscription> RssSubscriptions { get; protected set; }
        public virtual DbSet<SelfRole> SelfRoles { get; protected set; }
        public virtual DbSet<StarboardMessage> StarboardMessages { get; protected set; }
        public virtual DbSet<TextReaction> TextReactions { get; protected set; }
        public virtual DbSet<XpCount> XpCounts { get; protected set; }
        public virtual DbSet<XpRank> XpRanks { get; protected set; }
        #endregion

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

            // optionsBuilder.UseLazyLoadingProxies();

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

            if (this.Provider == DbProvider.Sqlite || this.Provider == DbProvider.SqliteInMemory) {
                foreach (IMutableEntityType entityType in mb.Model.GetEntityTypes()) {
                    IEnumerable<PropertyInfo> properties = entityType.ClrType.GetProperties()
                        .Where(p => p.PropertyType == typeof(DateTimeOffset) || p.PropertyType == typeof(DateTimeOffset?))
                        ;
                    foreach (PropertyInfo? property in properties)
                        mb.Entity(entityType.Name).Property(property.Name).HasConversion(new DateTimeOffsetToBinaryConverter());
                }
            }

            mb.Entity<ActionHistoryEntry>().HasKey(ahe => new { ahe.GuildIdDb, ahe.UserIdDb, ahe.Time });
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
            mb.Entity<ExemptedBackupEntity>().HasKey(e => new { e.GuildIdDb, e.ChannelIdDb });
            mb.Entity<ExemptedLoggingEntity>().HasKey(e => new { e.IdDb, e.GuildIdDb, e.Type });
            mb.Entity<ExemptedMentionEntity>().HasKey(e => new { e.IdDb, e.GuildIdDb, e.Type });
            mb.Entity<ExemptedRatelimitEntity>().HasKey(e => new { e.IdDb, e.GuildIdDb, e.Type });
            mb.Entity<ExemptedSpamEntity>().HasKey(e => new { e.IdDb, e.GuildIdDb, e.Type });
            mb.Entity<GameStats>().Property(s => s.AnimalRacesWon).HasDefaultValue(0);
            mb.Entity<GameStats>().Property(s => s.CaroLost).HasDefaultValue(0);
            mb.Entity<GameStats>().Property(s => s.CaroWon).HasDefaultValue(0);
            mb.Entity<GameStats>().Property(s => s.Connect4Lost).HasDefaultValue(0);
            mb.Entity<GameStats>().Property(s => s.Connect4Won).HasDefaultValue(0);
            mb.Entity<GameStats>().Property(s => s.DuelsLost).HasDefaultValue(0);
            mb.Entity<GameStats>().Property(s => s.DuelsWon).HasDefaultValue(0);
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
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.LogChannelIdDb).HasDefaultValue(0);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.MuteRoleIdDb).HasDefaultValue(0);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.LeaveChannelIdDb).HasDefaultValue(0);
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
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.WelcomeChannelIdDb).HasDefaultValue(0);
            mb.Entity<GuildConfig>().Property(gcfg => gcfg.WelcomeMessage).HasDefaultValue(null);
            mb.Entity<LevelRole>().HasKey(lr => new { lr.GuildIdDb, lr.Rank });
            mb.Entity<Meme>().HasKey(m => new { m.GuildIdDb, m.Name });
            mb.Entity<PurchasedItem>().HasKey(i => new { i.ItemId, i.UserIdDb });
            mb.Entity<ReactionRole>().HasKey(lr => new { lr.GuildIdDb, lr.Emoji });
            mb.Entity<Reminder>().Property(r => r.IsRepeating).HasDefaultValue(false);
            mb.Entity<Reminder>().Property(r => r.RepeatIntervalDb).HasDefaultValue(TimeSpan.FromMilliseconds(-1));
            mb.Entity<RssSubscription>().HasKey(sub => new { sub.Id, sub.GuildIdDb, sub.ChannelIdDb });
            mb.Entity<SelfRole>().HasKey(sr => new { sr.GuildIdDb, sr.RoleIdDb });
            mb.Entity<StarboardMessage>().HasKey(sm => new { sm.GuildIdDb, sm.ChannelIdDb, sm.MessageIdDb });
            mb.Entity<TextReactionTrigger>().HasKey(t => new { t.ReactionId, t.Trigger });
            mb.Entity<XpCount>().HasKey(xpc => new { xpc.GuildIdDb, xpc.UserIdDb });
            mb.Entity<XpCount>().Property(xpc => xpc.Xp).HasDefaultValue(1);
            mb.Entity<XpRank>().HasKey(xpr => new { xpr.GuildIdDb, xpr.Rank });
        }
    }
}
