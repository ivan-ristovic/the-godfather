﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TheGodfather.Database;

namespace TheGodfather.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("gf")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.0-preview3-35497")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseAutoRole", b =>
                {
                    b.Property<long>("GuildIdDb")
                        .HasColumnName("gid");

                    b.Property<long>("RoleIdDb")
                        .HasColumnName("rid");

                    b.HasKey("GuildIdDb", "RoleIdDb");

                    b.ToTable("auto_roles");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseBankAccount", b =>
                {
                    b.Property<long>("GuildIdDb")
                        .HasColumnName("gid");

                    b.Property<long>("UserIdDb")
                        .HasColumnName("uid");

                    b.Property<long>("Balance")
                        .HasColumnName("balance");

                    b.HasKey("GuildIdDb", "UserIdDb");

                    b.ToTable("bank_accounts");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseBirthday", b =>
                {
                    b.Property<long>("GuildIdDb")
                        .HasColumnName("gid");

                    b.Property<long>("ChannelIdDb")
                        .HasColumnName("cid");

                    b.Property<long>("UserIdDb")
                        .HasColumnName("uid");

                    b.Property<DateTime>("Date")
                        .HasColumnName("date")
                        .HasColumnType("date");

                    b.Property<int>("LastUpdateYear")
                        .HasColumnName("last_update_year");

                    b.HasKey("GuildIdDb", "ChannelIdDb", "UserIdDb");

                    b.ToTable("birthdays");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseBlockedChannel", b =>
                {
                    b.Property<long>("ChannelIdDb")
                        .HasColumnName("cid");

                    b.Property<string>("Reason")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("reason")
                        .HasMaxLength(64)
                        .HasDefaultValue(null);

                    b.HasKey("ChannelIdDb");

                    b.ToTable("blocked_channels");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseBlockedUser", b =>
                {
                    b.Property<long>("UserIdDb")
                        .HasColumnName("uid");

                    b.Property<string>("Reason")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("reason")
                        .HasMaxLength(64)
                        .HasDefaultValue(null);

                    b.HasKey("UserIdDb");

                    b.ToTable("blocked_users");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseBotStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<int>("Activity")
                        .HasColumnName("activity_type");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnName("status")
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.ToTable("bot_statuses");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseChicken", b =>
                {
                    b.Property<long>("GuildIdDb")
                        .HasColumnName("gid");

                    b.Property<long>("UserIdDb")
                        .HasColumnName("uid");

                    b.Property<int>("MaxVitality")
                        .HasColumnName("max_vit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasMaxLength(32);

                    b.Property<int>("Strength")
                        .HasColumnName("str");

                    b.Property<int>("Vitality")
                        .HasColumnName("vit");

                    b.HasKey("GuildIdDb", "UserIdDb");

                    b.ToTable("chickens");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseChickenBoughtUpgrade", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnName("id");

                    b.Property<long>("GuildIdDb")
                        .HasColumnName("gid");

                    b.Property<long>("UserIdDb")
                        .HasColumnName("uid");

                    b.HasKey("Id", "GuildIdDb", "UserIdDb");

                    b.HasIndex("GuildIdDb", "UserIdDb");

                    b.ToTable("chicken_bought_upgrades");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseChickenUpgrade", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnName("id");

                    b.Property<long>("Cost")
                        .HasColumnName("cost");

                    b.Property<int>("Modifier")
                        .HasColumnName("mod");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasMaxLength(32);

                    b.Property<int>("UpgradesStat")
                        .HasColumnName("stat");

                    b.HasKey("Id");

                    b.ToTable("chicken_upgrades");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseEmojiReaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<long>("GuildIdDb")
                        .HasColumnName("gid");

                    b.Property<string>("Reaction")
                        .IsRequired()
                        .HasColumnName("reaction")
                        .HasMaxLength(128);

                    b.HasKey("Id");

                    b.HasIndex("GuildIdDb");

                    b.ToTable("reactions_emoji");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseEmojiReactionTrigger", b =>
                {
                    b.Property<int>("ReactionId")
                        .HasColumnName("id");

                    b.Property<string>("Trigger")
                        .HasColumnName("trigger")
                        .HasMaxLength(128);

                    b.HasKey("ReactionId", "Trigger");

                    b.ToTable("reactions_emoji_triggers");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseExemptAntispam", b =>
                {
                    b.Property<long>("IdDb")
                        .HasColumnName("xid");

                    b.Property<long>("GuildIdDb")
                        .HasColumnName("gid");

                    b.Property<byte>("Type")
                        .HasColumnName("type");

                    b.HasKey("IdDb", "GuildIdDb", "Type");

                    b.HasIndex("GuildIdDb");

                    b.ToTable("exempt_antispam");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseExemptLogging", b =>
                {
                    b.Property<long>("IdDb")
                        .HasColumnName("xid");

                    b.Property<long>("GuildIdDb")
                        .HasColumnName("gid");

                    b.Property<byte>("Type")
                        .HasColumnName("type");

                    b.HasKey("IdDb", "GuildIdDb", "Type");

                    b.HasIndex("GuildIdDb");

                    b.ToTable("exempt_logging");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseExemptRatelimit", b =>
                {
                    b.Property<long>("IdDb")
                        .HasColumnName("xid");

                    b.Property<long>("GuildIdDb")
                        .HasColumnName("gid");

                    b.Property<byte>("Type")
                        .HasColumnName("type");

                    b.HasKey("IdDb", "GuildIdDb", "Type");

                    b.HasIndex("GuildIdDb");

                    b.ToTable("exempt_ratelimit");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseFilter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<long>("GuildIdDb")
                        .HasColumnName("gid");

                    b.Property<string>("Trigger")
                        .IsRequired()
                        .HasColumnName("trigger")
                        .HasMaxLength(128);

                    b.HasKey("Id");

                    b.HasIndex("GuildIdDb");

                    b.ToTable("filters");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseGameStats", b =>
                {
                    b.Property<long>("UserIdDb")
                        .HasColumnName("uid");

                    b.Property<int>("AnimalRacesWon")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("animalraces_won")
                        .HasDefaultValue(0);

                    b.Property<int>("CaroLost")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("caro_lost")
                        .HasDefaultValue(0);

                    b.Property<int>("CaroWon")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("caro_won")
                        .HasDefaultValue(0);

                    b.Property<int>("Chain4Lost")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("chain4_lost")
                        .HasDefaultValue(0);

                    b.Property<int>("Chain4Won")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("chain4_won")
                        .HasDefaultValue(0);

                    b.Property<int>("DuelsLost")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("duel_lost")
                        .HasDefaultValue(0);

                    b.Property<int>("DuelsWon")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("duel_won")
                        .HasDefaultValue(0);

                    b.Property<int>("HangmanWon")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("hangman_won")
                        .HasDefaultValue(0);

                    b.Property<int>("NumberRacesWon")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("numberraces_won")
                        .HasDefaultValue(0);

                    b.Property<int>("OthelloLost")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("othello_lost")
                        .HasDefaultValue(0);

                    b.Property<int>("OthelloWon")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("othello_won")
                        .HasDefaultValue(0);

                    b.Property<int>("QuizesWon")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("quizes_won")
                        .HasDefaultValue(0);

                    b.Property<int>("TicTacToeLost")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("ttt_lost")
                        .HasDefaultValue(0);

                    b.Property<int>("TicTacToeWon")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("ttt_won")
                        .HasDefaultValue(0);

                    b.HasKey("UserIdDb");

                    b.ToTable("game_stats");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseGuildConfig", b =>
                {
                    b.Property<long>("GuildIdDb")
                        .HasColumnName("gid");

                    b.Property<short>("AntiInstantLeaveCooldown")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("antiinstantleave_cooldown")
                        .HasDefaultValue((short)3);

                    b.Property<bool>("AntiInstantLeaveEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("antilnstantleave_enabled")
                        .HasDefaultValue(false);

                    b.Property<byte>("AntifloodAction")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("antiflood_action")
                        .HasDefaultValue((byte)4);

                    b.Property<short>("AntifloodCooldown")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("antiflood_cooldown")
                        .HasDefaultValue((short)10);

                    b.Property<bool>("AntifloodEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("antiflood_enabled")
                        .HasDefaultValue(false);

                    b.Property<short>("AntifloodSensitivity")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("antiflood_sensitivity")
                        .HasDefaultValue((short)5);

                    b.Property<byte>("AntispamAction")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("antispam_action")
                        .HasDefaultValue((byte)0);

                    b.Property<bool>("AntispamEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("antispam_enabled")
                        .HasDefaultValue(false);

                    b.Property<short>("AntispamSensitivity")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("antispam_sensitivity")
                        .HasDefaultValue((short)5);

                    b.Property<string>("Currency")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("currency")
                        .HasMaxLength(32)
                        .HasDefaultValue(null);

                    b.Property<long?>("LeaveChannelIdDb")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("leave_cid")
                        .HasDefaultValue(null);

                    b.Property<string>("LeaveMessage")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("leave_msg")
                        .HasMaxLength(128)
                        .HasDefaultValue(null);

                    b.Property<bool>("LinkfilterBootersEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("linkfilter_booters")
                        .HasDefaultValue(true);

                    b.Property<bool>("LinkfilterDiscordInvitesEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("linkfilter_invites")
                        .HasDefaultValue(false);

                    b.Property<bool>("LinkfilterDisturbingWebsitesEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("linkfilter_disturbing")
                        .HasDefaultValue(true);

                    b.Property<bool>("LinkfilterEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("linkfilter_enabled")
                        .HasDefaultValue(false);

                    b.Property<bool>("LinkfilterIpLoggersEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("linkfilter_loggers")
                        .HasDefaultValue(true);

                    b.Property<bool>("LinkfilterUrlShortenersEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("linkfilter_shorteners")
                        .HasDefaultValue(true);

                    b.Property<long?>("LogChannelIdDb")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("log_cid")
                        .HasDefaultValue(null);

                    b.Property<long?>("MuteRoleIdDb")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("mute_rid")
                        .HasDefaultValue(null);

                    b.Property<string>("Prefix")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("prefix")
                        .HasMaxLength(16)
                        .HasDefaultValue(null);

                    b.Property<byte>("RatelimitAction")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("ratelimit_action")
                        .HasDefaultValue((byte)1);

                    b.Property<bool>("RatelimitEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("ratelimit_enabled")
                        .HasDefaultValue(false);

                    b.Property<short>("RatelimitSensitivity")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("ratelimit_sensitivity")
                        .HasDefaultValue((short)5);

                    b.Property<bool>("ReactionResponse")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("silent_response_enabled")
                        .HasDefaultValue(false);

                    b.Property<bool>("SuggestionsEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("suggestions_enabled")
                        .HasDefaultValue(false);

                    b.Property<long?>("WelcomeChannelIdDb")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("welcome_cid")
                        .HasDefaultValue(null);

                    b.Property<string>("WelcomeMessage")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("welcome_msg")
                        .HasMaxLength(128)
                        .HasDefaultValue(null);

                    b.HasKey("GuildIdDb");

                    b.ToTable("guild_cfg");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseGuildRank", b =>
                {
                    b.Property<long>("GuildIdDb")
                        .HasColumnName("gid");

                    b.Property<short>("Rank")
                        .HasColumnName("rank");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasMaxLength(32);

                    b.HasKey("GuildIdDb", "Rank");

                    b.ToTable("guild_ranks");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseInsult", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnName("content")
                        .HasMaxLength(128);

                    b.HasKey("Id");

                    b.ToTable("insults");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseMeme", b =>
                {
                    b.Property<long>("GuildIdDb")
                        .HasColumnName("gid");

                    b.Property<string>("Name")
                        .HasColumnName("name")
                        .HasMaxLength(32);

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnName("url")
                        .HasMaxLength(128);

                    b.HasKey("GuildIdDb", "Name");

                    b.ToTable("memes");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseMessageCount", b =>
                {
                    b.Property<long>("UserIdDb")
                        .HasColumnName("uid");

                    b.Property<int>("MessageCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("message_count")
                        .HasDefaultValue(1);

                    b.HasKey("UserIdDb");

                    b.ToTable("user_info");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabasePrivilegedUser", b =>
                {
                    b.Property<long>("UserIdDb")
                        .HasColumnName("uid");

                    b.HasKey("UserIdDb");

                    b.ToTable("privileged_users");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabasePurchasableItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<long>("GuildIdDb")
                        .HasColumnName("gid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasMaxLength(64);

                    b.Property<long>("Price")
                        .HasColumnName("price");

                    b.HasKey("Id");

                    b.HasIndex("GuildIdDb");

                    b.ToTable("purchasable_items");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabasePurchasedItem", b =>
                {
                    b.Property<int>("ItemId")
                        .HasColumnName("id");

                    b.Property<long>("UserIdDb")
                        .HasColumnName("uid");

                    b.HasKey("ItemId", "UserIdDb");

                    b.ToTable("purchased_items");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseReminder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<long?>("ChannelIdDb")
                        .HasColumnName("cid");

                    b.Property<DateTimeOffset>("ExecutionTime")
                        .HasColumnName("execution_time")
                        .HasColumnType("timestamptz");

                    b.Property<bool>("IsRepeating")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("is_repeating")
                        .HasDefaultValue(false);

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnName("message")
                        .HasMaxLength(256);

                    b.Property<TimeSpan?>("RepeatIntervalDb")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("repeat_interval")
                        .HasColumnType("interval")
                        .HasDefaultValue(new TimeSpan(0, 0, 0, 0, -1));

                    b.Property<long>("UserIdDb")
                        .HasColumnName("uid");

                    b.HasKey("Id");

                    b.ToTable("reminders");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseRssFeed", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("LastPostUrl")
                        .IsRequired()
                        .HasColumnName("last_post_url");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnName("url");

                    b.HasKey("Id");

                    b.ToTable("rss_feeds");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseRssSubscription", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnName("id");

                    b.Property<long>("GuildIdDb")
                        .HasColumnName("gid");

                    b.Property<long>("ChannelIdDb")
                        .HasColumnName("cid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasMaxLength(64);

                    b.HasKey("Id", "GuildIdDb", "ChannelIdDb");

                    b.HasIndex("GuildIdDb");

                    b.ToTable("rss_subscriptions");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseSavedTask", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("ExecutionTime")
                        .HasColumnName("execution_time")
                        .HasColumnType("timestamptz");

                    b.Property<long>("GuildIdDb")
                        .HasColumnName("gid");

                    b.Property<long?>("RoleIdDb")
                        .HasColumnName("rid");

                    b.Property<byte>("Type")
                        .HasColumnName("type");

                    b.Property<long>("UserIdDb")
                        .HasColumnName("uid");

                    b.HasKey("Id");

                    b.HasIndex("GuildIdDb");

                    b.ToTable("saved_tasks");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseSelfRole", b =>
                {
                    b.Property<long>("GuildIdDb")
                        .HasColumnName("gid");

                    b.Property<long>("RoleIdDb")
                        .HasColumnName("rid");

                    b.HasKey("GuildIdDb", "RoleIdDb");

                    b.ToTable("self_roles");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseSwatPlayer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string[]>("AliasesDb")
                        .HasColumnName("aliases");

                    b.Property<string[]>("IPs")
                        .IsRequired()
                        .HasColumnName("ip");

                    b.Property<string>("Info")
                        .HasColumnName("additional_info");

                    b.Property<bool>("IsBlacklisted")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("is_blacklisted")
                        .HasDefaultValue(false);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasMaxLength(32);

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("swat_players");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseSwatServer", b =>
                {
                    b.Property<string>("IP")
                        .HasColumnName("ip")
                        .HasMaxLength(16);

                    b.Property<int>("JoinPort")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("join_port")
                        .HasDefaultValue(10480);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasMaxLength(32);

                    b.Property<int>("QueryPort")
                        .HasColumnName("query_port");

                    b.HasKey("IP");

                    b.ToTable("swat_servers");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseTextReaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<long>("GuildIdDb")
                        .HasColumnName("gid");

                    b.Property<string>("Response")
                        .IsRequired()
                        .HasColumnName("response")
                        .HasMaxLength(128);

                    b.HasKey("Id");

                    b.HasIndex("GuildIdDb");

                    b.ToTable("reactions_text");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseTextReactionTrigger", b =>
                {
                    b.Property<int>("ReactionId")
                        .HasColumnName("id");

                    b.Property<string>("Trigger")
                        .HasColumnName("trigger")
                        .HasMaxLength(128);

                    b.HasKey("ReactionId", "Trigger");

                    b.ToTable("reactions_text_triggers");
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseAutoRole", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabaseGuildConfig", "DbGuildConfig")
                        .WithMany("AutoRoles")
                        .HasForeignKey("GuildIdDb")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseBankAccount", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabaseGuildConfig", "DbGuildConfig")
                        .WithMany("Accounts")
                        .HasForeignKey("GuildIdDb")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseBirthday", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabaseGuildConfig", "DbGuildConfig")
                        .WithMany("Birthdays")
                        .HasForeignKey("GuildIdDb")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseChicken", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabaseGuildConfig", "DbGuildConfig")
                        .WithMany("Chickens")
                        .HasForeignKey("GuildIdDb")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseChickenBoughtUpgrade", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabaseGuildConfig")
                        .WithMany("ChickensBoughtUpgrades")
                        .HasForeignKey("GuildIdDb")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TheGodfather.Database.Entities.DatabaseChickenUpgrade", "DbChickenUpgrade")
                        .WithMany("BoughtUpgrades")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TheGodfather.Database.Entities.DatabaseChicken", "DbChicken")
                        .WithMany("DbUpgrades")
                        .HasForeignKey("GuildIdDb", "UserIdDb")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseEmojiReaction", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabaseGuildConfig", "DbGuildConfig")
                        .WithMany("EmojiReactions")
                        .HasForeignKey("GuildIdDb")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseEmojiReactionTrigger", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabaseEmojiReaction", "DbReaction")
                        .WithMany("DbTriggers")
                        .HasForeignKey("ReactionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseExemptAntispam", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabaseGuildConfig", "DbGuildConfig")
                        .WithMany("AntispamExempts")
                        .HasForeignKey("GuildIdDb")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseExemptLogging", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabaseGuildConfig", "DbGuildConfig")
                        .WithMany("LoggingExempts")
                        .HasForeignKey("GuildIdDb")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseExemptRatelimit", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabaseGuildConfig", "DbGuildConfig")
                        .WithMany("RatelimitExempts")
                        .HasForeignKey("GuildIdDb")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseFilter", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabaseGuildConfig", "DbGuildConfig")
                        .WithMany("Filters")
                        .HasForeignKey("GuildIdDb")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseGuildRank", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabaseGuildConfig", "DbGuildConfig")
                        .WithMany("Ranks")
                        .HasForeignKey("GuildIdDb")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseMeme", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabaseGuildConfig", "DbGuildConfig")
                        .WithMany("Memes")
                        .HasForeignKey("GuildIdDb")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabasePurchasableItem", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabaseGuildConfig", "DbGuildConfig")
                        .WithMany("PurchasableItems")
                        .HasForeignKey("GuildIdDb")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabasePurchasedItem", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabasePurchasableItem", "DbPurchasableItem")
                        .WithMany("Purchases")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseRssSubscription", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabaseGuildConfig", "DbGuildConfig")
                        .WithMany("Subscriptions")
                        .HasForeignKey("GuildIdDb")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TheGodfather.Database.Entities.DatabaseRssFeed", "DbRssFeed")
                        .WithMany("Subscriptions")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseSavedTask", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabaseGuildConfig", "DbGuildConfig")
                        .WithMany("SavedTasks")
                        .HasForeignKey("GuildIdDb")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseSelfRole", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabaseGuildConfig", "DbGuildConfig")
                        .WithMany("SelfRoles")
                        .HasForeignKey("GuildIdDb")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseTextReaction", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabaseGuildConfig", "DbGuildConfig")
                        .WithMany("TextReactions")
                        .HasForeignKey("GuildIdDb")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TheGodfather.Database.Entities.DatabaseTextReactionTrigger", b =>
                {
                    b.HasOne("TheGodfather.Database.Entities.DatabaseTextReaction", "DbReaction")
                        .WithMany("DbTriggers")
                        .HasForeignKey("ReactionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
