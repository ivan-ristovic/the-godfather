#region USING_DIRECTIVES
using Microsoft.EntityFrameworkCore;

using TheGodfather.Database.Entities;
#endregion

namespace TheGodfather.Database
{
    public class DatabaseContext : DbContext
    {
        public virtual DbSet<DatabaseAccounts> Accounts { get; set; }
        public virtual DbSet<DatabaseAntispamExempt> AntispamExempts { get; set; }
        public virtual DbSet<AssignableRoles> AssignableRoles { get; set; }
        public virtual DbSet<DatabaseAutomaticRoles> AutomaticRoles { get; set; }
        public virtual DbSet<DatabaseBirthdays> Birthdays { get; set; }
        public virtual DbSet<DatabaseBlockedChannels> BlockedChannels { get; set; }
        public virtual DbSet<DatabaseBlockedUsers> BlockedUsers { get; set; }
        public virtual DbSet<DatabaseChickenActiveUpgrades> ChickenActiveUpgrades { get; set; }
        public virtual DbSet<DatabaseChickens> Chickens { get; set; }
        public virtual DbSet<DatabaseChickenUpgrades> ChickenUpgrades { get; set; }
        public virtual DbSet<DatabaseEmojiReactions> EmojiReactions { get; set; }
        public virtual DbSet<DatabaseFeeds> Feeds { get; set; }
        public virtual DbSet<DatabaseFilters> Filters { get; set; }
        public virtual DbSet<DatabaseGuildConfig> GuildCfg { get; set; }
        public virtual DbSet<DatabaseInsults> Insults { get; set; }
        public virtual DbSet<DatabaseItems> Items { get; set; }
        public virtual DbSet<DatabaseLogExempt> LogExempts { get; set; }
        public virtual DbSet<DatabaseMemes> Memes { get; set; }
        public virtual DbSet<DatabaseMessageCount> MessageCount { get; set; }
        public virtual DbSet<DatabasePrivileged> Privileged { get; set; }
        public virtual DbSet<DatabasePurchases> Purchases { get; set; }
        public virtual DbSet<DatabaseRanks> Ranks { get; set; }
        public virtual DbSet<DatabaseRatelimitExempt> RatelimitExempts { get; set; }
        public virtual DbSet<DatabaseReminders> Reminders { get; set; }
        public virtual DbSet<DatabaseSavedTasks> SavedTasks { get; set; }
        public virtual DbSet<DatabaseStats> Stats { get; set; }
        public virtual DbSet<DatabaseStatuses> Statuses { get; set; }
        public virtual DbSet<DatabaseSubscriptions> Subscriptions { get; set; }
        public virtual DbSet<DatabaseSwatBanlist> SwatBanlist { get; set; }
        public virtual DbSet<DatabaseSwatIps> SwatIps { get; set; }
        public virtual DbSet<DatabaseSwatServers> SwatServers { get; set; }
        public virtual DbSet<DatabaseTextReactions> TextReactions { get; set; }

        private string ConnectionString { get; }


        public DatabaseContext(string connectionString)
        {
            this.ConnectionString = connectionString;
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

            this.CheckIntegrity(model);
        }


        private void CheckIntegrity(ModelBuilder model)
        {
            model.HasPostgresExtension("fuzzystrmatch");
            model.Entity<DatabaseAccounts>(entity => {
                entity.HasKey(e => new { e.Uid, e.Gid })
                    .HasName("accounts_pkey");

                entity.ToTable("accounts", "gf");

                entity.HasIndex(e => e.Gid)
                    .HasName("fki_accounts_fkey_gid");

                entity.Property(e => e.Uid).HasColumnName("uid");

                entity.Property(e => e.Gid).HasColumnName("gid");

                entity.Property(e => e.Balance).HasColumnName("balance");

                entity.HasOne(d => d.G)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.Gid)
                    .HasConstraintName("accounts_fkey_gid");
            });
            model.Entity<AssignableRoles>(entity => {
                entity.HasKey(e => new { e.Gid, e.Rid })
                    .HasName("assignable_roles_pkey");

                entity.ToTable("assignable_roles", "gf");

                entity.Property(e => e.Gid).HasColumnName("gid");

                entity.Property(e => e.Rid).HasColumnName("rid");

                entity.HasOne(d => d.G)
                    .WithMany(p => p.AssignableRoles)
                    .HasForeignKey(d => d.Gid)
                    .HasConstraintName("sar_fkey");
            });
            model.Entity<DatabaseAutomaticRoles>(entity => {
                entity.HasKey(e => new { e.Gid, e.Rid })
                    .HasName("automatic_roles_pkey");

                entity.ToTable("automatic_roles", "gf");

                entity.Property(e => e.Gid).HasColumnName("gid");

                entity.Property(e => e.Rid).HasColumnName("rid");

                entity.HasOne(d => d.G)
                    .WithMany(p => p.AutomaticRoles)
                    .HasForeignKey(d => d.Gid)
                    .HasConstraintName("ar_fkey");
            });
            model.Entity<DatabaseBirthdays>(entity => {
                entity.HasKey(e => new { e.Uid, e.Cid })
                    .HasName("birthdays_pkey");

                entity.ToTable("birthdays", "gf");

                entity.HasIndex(e => e.Bday)
                    .HasName("index_bday");

                entity.Property(e => e.Uid).HasColumnName("uid");

                entity.Property(e => e.Cid).HasColumnName("cid");

                entity.Property(e => e.Bday)
                    .HasColumnName("bday")
                    .HasColumnType("date");

                entity.Property(e => e.LastUpdated).HasColumnName("last_updated");
            });
            model.Entity<DatabaseBlockedChannels>(entity => {
                entity.HasKey(e => e.Cid)
                    .HasName("blocked_channels_pkey");

                entity.ToTable("blocked_channels", "gf");

                entity.Property(e => e.Cid)
                    .HasColumnName("cid")
                    .ValueGeneratedNever();

                entity.Property(e => e.Reason)
                    .HasColumnName("reason")
                    .HasMaxLength(64);
            });
            model.Entity<DatabaseBlockedUsers>(entity => {
                entity.HasKey(e => e.Uid)
                    .HasName("blocked_users_pkey");

                entity.ToTable("blocked_users", "gf");

                entity.Property(e => e.Uid)
                    .HasColumnName("uid")
                    .ValueGeneratedNever();

                entity.Property(e => e.Reason)
                    .HasColumnName("reason")
                    .HasMaxLength(64);
            });
            model.Entity<DatabaseChickenActiveUpgrades>(entity => {
                entity.HasKey(e => new { e.Uid, e.Gid, e.Wid })
                    .HasName("chicken_active_upgrades_pkey");

                entity.ToTable("chicken_active_upgrades", "gf");

                entity.Property(e => e.Uid).HasColumnName("uid");

                entity.Property(e => e.Gid).HasColumnName("gid");

                entity.Property(e => e.Wid).HasColumnName("wid");

                entity.HasOne(d => d.W)
                    .WithMany(p => p.ChickenActiveUpgrades)
                    .HasForeignKey(d => d.Wid)
                    .HasConstraintName("chicken_upgrades_wid_fkey");

                entity.HasOne(d => d.Chickens)
                    .WithMany(p => p.ChickenActiveUpgrades)
                    .HasForeignKey(d => new { d.Uid, d.Gid })
                    .HasConstraintName("chicken_upgrades_uid_fkey");
            });
            model.Entity<DatabaseChickens>(entity => {
                entity.HasKey(e => new { e.Uid, e.Gid })
                    .HasName("chickens_pkey");

                entity.ToTable("chickens", "gf");

                entity.HasIndex(e => e.Gid)
                    .HasName("fki_chickens_fkey_gid");

                entity.Property(e => e.Uid).HasColumnName("uid");

                entity.Property(e => e.Gid).HasColumnName("gid");

                entity.Property(e => e.MaxVitality)
                    .HasColumnName("max_vitality")
                    .HasDefaultValueSql("100")
                    .ForNpgsqlHasComment(@"
");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(32);

                entity.Property(e => e.Strength)
                    .HasColumnName("strength")
                    .HasDefaultValueSql("50");

                entity.Property(e => e.Vitality)
                    .HasColumnName("vitality")
                    .HasDefaultValueSql("100");

                entity.HasOne(d => d.G)
                    .WithMany(p => p.Chickens)
                    .HasForeignKey(d => d.Gid)
                    .HasConstraintName("chicken_fkey_gid");
            });
            model.Entity<DatabaseChickenUpgrades>(entity => {
                entity.HasKey(e => e.Wid)
                    .HasName("chicken_weapons_pkey");

                entity.ToTable("chicken_upgrades", "gf");

                entity.Property(e => e.Wid)
                    .HasColumnName("wid")
                    .HasDefaultValueSql("nextval('gf.chicken_weapons_wid_seq'::regclass)");

                entity.Property(e => e.Modifier).HasColumnName("modifier");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(32);

                entity.Property(e => e.Price).HasColumnName("price");

                entity.Property(e => e.UpgradesStat).HasColumnName("upgrades_stat");
            });
            model.Entity<DatabaseEmojiReactions>(entity => {
                entity.ToTable("emoji_reactions", "gf");

                entity.HasIndex(e => e.Gid)
                    .HasName("index_er_gid");

                entity.HasIndex(e => e.Trigger)
                    .HasName("emoji_reactions_trigger_idx");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('gf.emoji_reactions_id_seq'::regclass)");

                entity.Property(e => e.Gid).HasColumnName("gid");

                entity.Property(e => e.Reaction)
                    .HasColumnName("reaction")
                    .HasMaxLength(64);

                entity.Property(e => e.Trigger)
                    .IsRequired()
                    .HasColumnName("trigger")
                    .HasMaxLength(128);

                entity.HasOne(d => d.G)
                    .WithMany(p => p.EmojiReactions)
                    .HasForeignKey(d => d.Gid)
                    .HasConstraintName("er_fkey");
            });
            model.Entity<DatabaseFeeds>(entity => {
                entity.ToTable("feeds", "gf");

                entity.HasIndex(e => e.Url)
                    .HasName("feeds_url_key")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('gf.feeds_id_seq'::regclass)");

                entity.Property(e => e.Savedurl)
                    .IsRequired()
                    .HasColumnName("savedurl")
                    .HasDefaultValueSql("''::text");

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasColumnName("url");
            });
            model.Entity<DatabaseFilters>(entity => {
                entity.ToTable("filters", "gf");

                entity.HasIndex(e => e.Gid)
                    .HasName("index_filters_gid");

                entity.HasIndex(e => new { e.Filter, e.Gid })
                    .HasName("filters_unique")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('gf.filters_id_seq'::regclass)");

                entity.Property(e => e.Filter)
                    .IsRequired()
                    .HasColumnName("filter")
                    .HasMaxLength(64);

                entity.Property(e => e.Gid).HasColumnName("gid");

                entity.HasOne(d => d.G)
                    .WithMany(p => p.Filters)
                    .HasForeignKey(d => d.Gid)
                    .HasConstraintName("f_fkey");
            });
            model.Entity<DatabaseGuildConfig>(entity => {
                entity.HasKey(e => e.Gid)
                    .HasName("guild_cfg_pkey");

                entity.ToTable("guild_cfg", "gf");

                entity.Property(e => e.Gid)
                    .HasColumnName("gid")
                    .ValueGeneratedNever();

                entity.Property(e => e.AntifloodAction)
                    .HasColumnName("antiflood_action")
                    .HasDefaultValueSql("2");

                entity.Property(e => e.AntifloodCooldown)
                    .HasColumnName("antiflood_cooldown")
                    .HasDefaultValueSql("10");

                entity.Property(e => e.AntifloodEnabled).HasColumnName("antiflood_enabled");

                entity.Property(e => e.AntifloodSens)
                    .HasColumnName("antiflood_sens")
                    .HasDefaultValueSql("5");

                entity.Property(e => e.AntijoinleaveCooldown)
                    .HasColumnName("antijoinleave_cooldown")
                    .HasDefaultValueSql("3");

                entity.Property(e => e.AntijoinleaveEnabled).HasColumnName("antijoinleave_enabled");

                entity.Property(e => e.AntispamAction)
                    .HasColumnName("antispam_action")
                    .HasDefaultValueSql("1");

                entity.Property(e => e.AntispamEnabled).HasColumnName("antispam_enabled");

                entity.Property(e => e.AntispamSens)
                    .HasColumnName("antispam_sens")
                    .HasDefaultValueSql("5");

                entity.Property(e => e.Currency)
                    .HasColumnName("currency")
                    .HasMaxLength(32)
                    .HasDefaultValueSql("NULL::character varying");

                entity.Property(e => e.LeaveCid).HasColumnName("leave_cid");

                entity.Property(e => e.LeaveMsg)
                    .HasColumnName("leave_msg")
                    .HasMaxLength(128);

                entity.Property(e => e.LinkfilterBooters)
                    .IsRequired()
                    .HasColumnName("linkfilter_booters")
                    .HasDefaultValueSql("true");

                entity.Property(e => e.LinkfilterDisturbing)
                    .IsRequired()
                    .HasColumnName("linkfilter_disturbing")
                    .HasDefaultValueSql("true");

                entity.Property(e => e.LinkfilterEnabled).HasColumnName("linkfilter_enabled");

                entity.Property(e => e.LinkfilterInvites).HasColumnName("linkfilter_invites");

                entity.Property(e => e.LinkfilterIploggers)
                    .IsRequired()
                    .HasColumnName("linkfilter_iploggers")
                    .HasDefaultValueSql("true");

                entity.Property(e => e.LinkfilterShorteners)
                    .IsRequired()
                    .HasColumnName("linkfilter_shorteners")
                    .HasDefaultValueSql("true");

                entity.Property(e => e.LogCid).HasColumnName("log_cid");

                entity.Property(e => e.MuteRid).HasColumnName("mute_rid");

                entity.Property(e => e.Prefix)
                    .HasColumnName("prefix")
                    .HasMaxLength(16)
                    .HasDefaultValueSql("NULL::character varying");

                entity.Property(e => e.RatelimitAction).HasColumnName("ratelimit_action");

                entity.Property(e => e.RatelimitEnabled).HasColumnName("ratelimit_enabled");

                entity.Property(e => e.RatelimitSens)
                    .HasColumnName("ratelimit_sens")
                    .HasDefaultValueSql("5");

                entity.Property(e => e.SilentRespond)
                    .IsRequired()
                    .HasColumnName("silent_respond")
                    .HasDefaultValueSql("true");

                entity.Property(e => e.SuggestionsEnabled).HasColumnName("suggestions_enabled");

                entity.Property(e => e.WelcomeCid).HasColumnName("welcome_cid");

                entity.Property(e => e.WelcomeMsg)
                    .HasColumnName("welcome_msg")
                    .HasMaxLength(128);
            });
            model.Entity<DatabaseInsults>(entity => {
                entity.ToTable("insults", "gf");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('gf.insults_id_seq'::regclass)");

                entity.Property(e => e.Insult)
                    .HasColumnName("insult")
                    .HasMaxLength(128);
            });
            model.Entity<DatabaseItems>(entity => {
                entity.ToTable("items", "gf");

                entity.HasIndex(e => e.Gid)
                    .HasName("items_gid_index");

                entity.HasIndex(e => e.Name)
                    .HasName("items_name_unique")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('gf.items_id_seq'::regclass)");

                entity.Property(e => e.Gid).HasColumnName("gid");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(64);

                entity.Property(e => e.Price).HasColumnName("price");

                entity.HasOne(d => d.G)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.Gid)
                    .HasConstraintName("items_fkey");
            });
            model.Entity<DatabaseExempt>(entity => {
                entity.HasKey(e => new { e.Id, e.Type, e.Gid })
                    .HasName("log_exempt_pkey");

                entity.ToTable("log_exempt", "gf");

                entity.HasIndex(e => new { e.Gid, e.Type })
                    .HasName("log_exempt_clustered_index");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.Gid).HasColumnName("gid");

                entity.HasOne(d => d.G)
                    .WithMany(p => p.LogExempt)
                    .HasForeignKey(d => d.Gid)
                    .HasConstraintName("log_exempt_gid_fkey");
            });
            model.Entity<DatabaseMemes>(entity => {
                entity.HasKey(e => new { e.Gid, e.Name })
                    .HasName("memes_pkey");

                entity.ToTable("memes", "gf");

                entity.HasIndex(e => new { e.Gid, e.Name })
                    .HasName("index_memes_cluster")
                    .IsUnique();

                entity.Property(e => e.Gid).HasColumnName("gid");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(32);

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasColumnName("url")
                    .HasMaxLength(128);

                entity.HasOne(d => d.G)
                    .WithMany(p => p.Memes)
                    .HasForeignKey(d => d.Gid)
                    .HasConstraintName("memes_fkey");
            });
            model.Entity<DatabaseMessageCount>(entity => {
                entity.HasKey(e => e.Uid)
                    .HasName("msgcount_pkey");

                entity.ToTable("msgcount", "gf");

                entity.Property(e => e.Uid)
                    .HasColumnName("uid")
                    .ValueGeneratedNever();

                entity.Property(e => e.Count)
                    .HasColumnName("count")
                    .HasDefaultValueSql("1");
            });
            model.Entity<DatabasePrivileged>(entity => {
                entity.HasKey(e => e.Uid)
                    .HasName("priviledged_pkey");

                entity.ToTable("privileged", "gf");

                entity.Property(e => e.Uid)
                    .HasColumnName("uid")
                    .ValueGeneratedNever();
            });
            model.Entity<DatabasePurchases>(entity => {
                entity.HasKey(e => new { e.Id, e.Uid })
                    .HasName("purchases_pkey");

                entity.ToTable("purchases", "gf");

                entity.HasIndex(e => e.Id)
                    .HasName("purchases_id_index")
                    .ForNpgsqlHasMethod("hash");

                entity.HasIndex(e => e.Uid)
                    .HasName("purchases_uid_index");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('gf.purchases_id_seq'::regclass)");

                entity.Property(e => e.Uid).HasColumnName("uid");

                entity.HasOne(d => d.IdNavigation)
                    .WithMany(p => p.Purchases)
                    .HasForeignKey(d => d.Id)
                    .HasConstraintName("purchases_id_fkey");
            });
            model.Entity<DatabaseRanks>(entity => {
                entity.HasKey(e => new { e.Gid, e.Rank })
                    .HasName("ranks_pkey");

                entity.ToTable("ranks", "gf");

                entity.Property(e => e.Gid).HasColumnName("gid");

                entity.Property(e => e.Rank).HasColumnName("rank");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(32);

                entity.HasOne(d => d.G)
                    .WithMany(p => p.Ranks)
                    .HasForeignKey(d => d.Gid)
                    .HasConstraintName("ranks_gid_fkey");
            });
            model.Entity<DatabaseReminders>(entity => {
                entity.ToTable("reminders", "gf");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('gf.reminders_id_seq'::regclass)");

                entity.Property(e => e.Cid).HasColumnName("cid");

                entity.Property(e => e.ExecutionTime)
                    .HasColumnName("execution_time")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Interval).HasColumnName("interval");

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasColumnName("message")
                    .HasMaxLength(256);

                entity.Property(e => e.Repeat).HasColumnName("repeat");

                entity.Property(e => e.Uid).HasColumnName("uid");
            });
            model.Entity<DatabaseSavedTasks>(entity => {
                entity.ToTable("saved_tasks", "gf");

                entity.HasIndex(e => e.Gid)
                    .HasName("fki_saved_tasks_fkey_gid");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('gf.saved_tasks_id_seq'::regclass)");

                entity.Property(e => e.ExecutionTime)
                    .HasColumnName("execution_time")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Gid).HasColumnName("gid");

                entity.Property(e => e.Rid)
                    .HasColumnName("rid")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.Uid).HasColumnName("uid");

                entity.HasOne(d => d.G)
                    .WithMany(p => p.SavedTasks)
                    .HasForeignKey(d => d.Gid)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("saved_tasks_fkey_gid");
            });
            model.Entity<DatabaseStats>(entity => {
                entity.HasKey(e => e.Uid)
                    .HasName("stats_pkey");

                entity.ToTable("stats", "gf");

                entity.Property(e => e.Uid)
                    .HasColumnName("uid")
                    .ValueGeneratedNever();

                entity.Property(e => e.CaroLost).HasColumnName("caro_lost");

                entity.Property(e => e.CaroWon).HasColumnName("caro_won");

                entity.Property(e => e.Chain4Lost).HasColumnName("chain4_lost");

                entity.Property(e => e.Chain4Won).HasColumnName("chain4_won");

                entity.Property(e => e.DuelsLost).HasColumnName("duels_lost");

                entity.Property(e => e.DuelsWon).HasColumnName("duels_won");

                entity.Property(e => e.HangmanWon).HasColumnName("hangman_won");

                entity.Property(e => e.NumracesWon).HasColumnName("numraces_won");

                entity.Property(e => e.OthelloLost).HasColumnName("othello_lost");

                entity.Property(e => e.OthelloWon).HasColumnName("othello_won");

                entity.Property(e => e.QuizesWon).HasColumnName("quizes_won");

                entity.Property(e => e.RacesWon).HasColumnName("races_won");

                entity.Property(e => e.TttLost).HasColumnName("ttt_lost");

                entity.Property(e => e.TttWon).HasColumnName("ttt_won");
            });
            model.Entity<DatabaseStatuses>(entity => {
                entity.ToTable("statuses", "gf");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('gf.statuses_id_seq'::regclass)");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasMaxLength(64);

                entity.Property(e => e.Type).HasColumnName("type");
            });
            model.Entity<DatabaseSubscriptions>(entity => {
                entity.HasKey(e => new { e.Id, e.Cid })
                    .HasName("subscriptions_pkey");

                entity.ToTable("subscriptions", "gf");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('gf.subscriptions_id_seq'::regclass)");

                entity.Property(e => e.Cid).HasColumnName("cid");

                entity.Property(e => e.Qname)
                    .HasColumnName("qname")
                    .HasMaxLength(64);

                entity.HasOne(d => d.IdNavigation)
                    .WithMany(p => p.Subscriptions)
                    .HasForeignKey(d => d.Id)
                    .HasConstraintName("subscriptions_id_fkey");
            });
            model.Entity<DatabaseSwatBanlist>(entity => {
                entity.HasKey(e => e.Ip)
                    .HasName("swat_banlist_pkey");

                entity.ToTable("swat_banlist", "gf");

                entity.HasIndex(e => new { e.Name, e.Ip })
                    .HasName("swat_banlist_unique")
                    .IsUnique();

                entity.Property(e => e.Ip)
                    .HasColumnName("ip")
                    .HasMaxLength(16)
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(32);

                entity.Property(e => e.Reason)
                    .HasColumnName("reason")
                    .HasMaxLength(64)
                    .HasDefaultValueSql("NULL::character varying");
            });
            model.Entity<DatabaseSwatIps>(entity => {
                entity.HasKey(e => new { e.Name, e.Ip })
                    .HasName("swat_ips_pkey");

                entity.ToTable("swat_ips", "gf");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(32);

                entity.Property(e => e.Ip)
                    .HasColumnName("ip")
                    .HasMaxLength(16);

                entity.Property(e => e.AdditionalInfo)
                    .HasColumnName("additional_info")
                    .HasMaxLength(128)
                    .HasDefaultValueSql("NULL::character varying");
            });
            model.Entity<DatabaseSwatServers>(entity => {
                entity.HasKey(e => e.Ip)
                    .HasName("swat_servers_pkey");

                entity.ToTable("swat_servers", "gf");

                entity.HasIndex(e => e.Name)
                    .HasName("swat_servers_name_key")
                    .IsUnique();

                entity.Property(e => e.Ip)
                    .HasColumnName("ip")
                    .HasMaxLength(32)
                    .ValueGeneratedNever();

                entity.Property(e => e.Joinport).HasColumnName("joinport");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(32);

                entity.Property(e => e.Queryport).HasColumnName("queryport");
            });
            model.Entity<DatabaseTextReactions>(entity => {
                entity.ToTable("text_reactions", "gf");

                entity.HasIndex(e => e.Gid)
                    .HasName("index_tr_gid");

                entity.HasIndex(e => e.Trigger)
                    .HasName("trigger_index");

                entity.HasIndex(e => new { e.Gid, e.Trigger })
                    .HasName("text_reactions_gid_trigger_key")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('gf.text_reactions_id_seq'::regclass)");

                entity.Property(e => e.Gid).HasColumnName("gid");

                entity.Property(e => e.Response)
                    .IsRequired()
                    .HasColumnName("response")
                    .HasMaxLength(128);

                entity.Property(e => e.Trigger)
                    .IsRequired()
                    .HasColumnName("trigger")
                    .HasMaxLength(128);

                entity.HasOne(d => d.G)
                    .WithMany(p => p.TextReactions)
                    .HasForeignKey(d => d.Gid)
                    .HasConstraintName("tr_fkey");
            });

            model.HasSequence<int>("chicken_weapons_wid_seq");
            model.HasSequence("emoji_reactions_id_seq");
            model.HasSequence("feeds_id_seq");
            model.HasSequence<int>("filters_id_seq");
            model.HasSequence("insults_id_seq");
            model.HasSequence("items_id_seq");
            model.HasSequence("purchases_id_seq");
            model.HasSequence<int>("reminders_id_seq");
            model.HasSequence<int>("saved_tasks_id_seq");
            model.HasSequence("statuses_id_seq");
            model.HasSequence("subscriptions_id_seq");
            model.HasSequence("text_reactions_id_seq");
        }
    }
}