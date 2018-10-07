#region USING_DIRECTIVES
using Microsoft.EntityFrameworkCore;

using System;

using TheGodfather.Database.Entities;
using TheGodfather.Modules.Administration.Common;
#endregion

namespace TheGodfather.Database
{
    public class DatabaseContext : DbContext
    {
        public virtual DbSet<DatabaseExemptAntispam> AntispamExempts { get; set; }
        public virtual DbSet<DatabaseAutoRole> AutoAssignableRoles { get; set; }
        public virtual DbSet<DatabaseBankAccount> BankAccounts { get; set; }
        public virtual DbSet<DatabaseBirthday> Birthdays { get; set; }
        public virtual DbSet<DatabaseBlockedChannel> BlockedChannels { get; set; }
        public virtual DbSet<DatabaseBlockedUser> BlockedUsers { get; set; }
        public virtual DbSet<DatabaseChicken> Chickens { get; set; }
        public virtual DbSet<DatabaseChickenUpgrade> ChickensUpgrades { get; set; }
        public virtual DbSet<DatabaseChickenBoughtUpgrade> ChickensBoughtUpgrades { get; set; }
        public virtual DbSet<DatabaseChickenUpgrade> ChickenUpgrades { get; set; }
        public virtual DbSet<DatabaseEmojiReaction> EmojiReactions { get; set; }
        public virtual DbSet<DatabaseFilter> Filters { get; set; }
        public virtual DbSet<DatabaseGameStats> GameStats { get; set; }
        public virtual DbSet<DatabaseGuildConfig> GuildConfig { get; set; }
        public virtual DbSet<DatabaseGuildRank> GuildRanks { get; set; }
        public virtual DbSet<DatabaseInsult> Insults { get; set; }
        public virtual DbSet<DatabaseExemptLogging> LoggingExempts { get; set; }
        public virtual DbSet<DatabaseMeme> Memes { get; set; }
        public virtual DbSet<DatabasePrivilegedUser> PrivilegedUsers { get; set; }
        public virtual DbSet<DatabasePurchasableItem> PurchasableItems { get; set; }
        public virtual DbSet<DatabasePurchasedItem> PurchasedItems { get; set; }
        public virtual DbSet<DatabaseExemptRatelimit> RatelimitExempts { get; set; }
        public virtual DbSet<DatabaseReminder> Reminders { get; set; }
        public virtual DbSet<DatabaseRssFeed> RssFeeds { get; set; }
        public virtual DbSet<DatabaseRssSubscription> RssSubscriptions { get; set; }
        public virtual DbSet<DatabaseSavedTask> SavedTasks { get; set; }
        public virtual DbSet<DatabaseSelfRole> SelfAssignableRoles { get; set; }
        public virtual DbSet<DatabaseSwatPlayer> SwatPlayers { get; set; }
        public virtual DbSet<DatabaseSwatServer> SwatServers { get; set; }
        public virtual DbSet<DatabaseTextReaction> TextReactions { get; set; }
        public virtual DbSet<DatabaseUserInfo> UsersInfo { get; set; }

        private string ConnectionString { get; }


        public DatabaseContext(string connectionString)
        {
            this.ConnectionString = connectionString;
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            // optionsBuilder.EnableSensitiveDataLogging(true);

            optionsBuilder.UseNpgsql(this.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder model)
        {
            model.HasDefaultSchema("gf");

            model.Entity<DatabaseAutoRole>().HasKey(e => new { e.GuildIdDb, e.RoleIdDb });
            model.Entity<DatabaseBankAccount>().HasKey(e => new { e.GuildIdDb, e.UserIdDb });
            model.Entity<DatabaseBirthday>().HasKey(e => new { e.GuildIdDb, e.ChannelIdDb, e.UserIdDb });
            model.Entity<DatabaseBirthday>().Property(b => b.LastUpdateYear).HasDefaultValue(0);
            model.Entity<DatabaseBlockedChannel>().Property(bc => bc.Reason).HasDefaultValue(null);
            model.Entity<DatabaseBlockedUser>().Property(bu => bu.Reason).HasDefaultValue(null);
            model.Entity<DatabaseChicken>().HasKey(e => new { e.GuildIdDb, e.UserIdDb });
            model.Entity<DatabaseChickenBoughtUpgrade>().HasKey(e => new { e.Id, e.GuildIdDb, e.UserIdDb });
            model.Entity<DatabaseChickenBoughtUpgrade>().HasOne(bu => bu.DbChickenUpgrade).WithMany(u => u.BoughtUpgrades).HasForeignKey(u => u.Id);
            model.Entity<DatabaseChickenBoughtUpgrade>().HasOne(bu => bu.DbChicken).WithMany(u => u.DbUpgrades).HasForeignKey(bu => new { bu.UserIdDb, bu.GuildIdDb });
            model.Entity<DatabaseExemptAntispam>().HasKey(e => new { e.IdDb, e.GuildIdDb, e.Type });
            model.Entity<DatabaseExemptLogging>().HasKey(e => new { e.IdDb, e.GuildIdDb, e.Type });
            model.Entity<DatabaseExemptRatelimit>().HasKey(e => new { e.IdDb, e.GuildIdDb, e.Type });
            model.Entity<DatabaseGameStats>().Property(s => s.AnimalRaceWon).HasDefaultValue(0);
            model.Entity<DatabaseGameStats>().Property(s => s.CaroLost).HasDefaultValue(0);
            model.Entity<DatabaseGameStats>().Property(s => s.CaroWon).HasDefaultValue(0);
            model.Entity<DatabaseGameStats>().Property(s => s.Chain4Lost).HasDefaultValue(0);
            model.Entity<DatabaseGameStats>().Property(s => s.Chain4Won).HasDefaultValue(0);
            model.Entity<DatabaseGameStats>().Property(s => s.DuelsLost).HasDefaultValue(0);
            model.Entity<DatabaseGameStats>().Property(s => s.DuelsWon).HasDefaultValue(0);
            model.Entity<DatabaseGameStats>().Property(s => s.HangmanLost).HasDefaultValue(0);
            model.Entity<DatabaseGameStats>().Property(s => s.HangmanWon).HasDefaultValue(0);
            model.Entity<DatabaseGameStats>().Property(s => s.OthelloLost).HasDefaultValue(0);
            model.Entity<DatabaseGameStats>().Property(s => s.OthelloWon).HasDefaultValue(0);
            model.Entity<DatabaseGameStats>().Property(s => s.QuizWon).HasDefaultValue(0);
            model.Entity<DatabaseGameStats>().Property(s => s.TicTacToeLost).HasDefaultValue(0);
            model.Entity<DatabaseGameStats>().Property(s => s.TicTacToeWon).HasDefaultValue(0);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.AntifloodAction).HasDefaultValue(PunishmentActionType.PermanentBan);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.AntifloodCooldown).HasDefaultValue(10);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.AntifloodEnabled).HasDefaultValue(false);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.AntifloodSensitivity).HasDefaultValue(5);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.AntiInstantLeaveCooldown).HasDefaultValue(3);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.AntiInstantLeaveEnabled).HasDefaultValue(false);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.AntispamAction).HasDefaultValue(PunishmentActionType.PermanentMute);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.AntispamEnabled).HasDefaultValue(false);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.AntispamSensitivity).HasDefaultValue(5);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.Currency).HasDefaultValue(null);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.LogChannelIdDb).HasDefaultValue();
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.MuteRoleIdDb).HasDefaultValue(null);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.LeaveChannelIdDb).HasDefaultValue(null);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.LeaveMessage).HasDefaultValue(null);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.LinkfilterBootersEnabled).HasDefaultValue(true);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.LinkfilterDiscordInvitesEnabled).HasDefaultValue(false);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.LinkfilterDisturbingWebsitesEnabled).HasDefaultValue(true);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.LinkfilterEnabled).HasDefaultValue(false);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.LinkfilterIpLoggersEnabled).HasDefaultValue(true);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.LinkfilterUrlShortenersEnabled).HasDefaultValue(true);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.Prefix).HasDefaultValue(null);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.RatelimitAction).HasDefaultValue(PunishmentActionType.TemporaryMute);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.RatelimitEnabled).HasDefaultValue(false);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.RatelimitSensitivity).HasDefaultValue(5);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.ReactionResponse).HasDefaultValue(false);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.SuggestionsEnabled).HasDefaultValue(false);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.WelcomeChannelIdDb).HasDefaultValue(null);
            model.Entity<DatabaseGuildConfig>().Property(gcfg => gcfg.WelcomeMessage).HasDefaultValue(null);
            model.Entity<DatabaseGuildRank>().HasKey(e => new { e.GuildIdDb, e.Rank });
            model.Entity<DatabaseMeme>().HasKey(e => new { e.GuildIdDb, e.Name });
            model.Entity<DatabasePurchasedItem>().HasKey(e => new { e.ItemId, e.UserIdDb });
            model.Entity<DatabaseReminder>().Property(r => r.IsRepeating).HasDefaultValue(false);
            model.Entity<DatabaseReminder>().Property(r => r.RepeatIntervalDb).HasDefaultValue(TimeSpan.FromMilliseconds(-1));
            model.Entity<DatabaseRssSubscription>().HasKey(e => new { e.Id, e.GuildIdDb, e.ChannelIdDb });
            model.Entity<DatabaseSelfRole>().HasKey(e => new { e.GuildIdDb, e.RoleIdDb });
            model.Entity<DatabaseSwatPlayer>().Property(p => p.IsBlacklisted).HasDefaultValue(false);
            model.Entity<DatabaseSwatServer>().Property(srv => srv.JoinPort).HasDefaultValue(10480);
            model.Entity<DatabaseUserInfo>().Property(ui => ui.MessageCount).HasDefaultValue(1);
        }
    }
}