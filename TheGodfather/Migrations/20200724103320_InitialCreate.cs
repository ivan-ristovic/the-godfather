using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TheGodfather.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "gf");

            migrationBuilder.CreateTable(
                name: "blocked_channels",
                schema: "gf",
                columns: table => new
                {
                    cid = table.Column<long>(nullable: false),
                    reason = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blocked_channels", x => x.cid);
                });

            migrationBuilder.CreateTable(
                name: "blocked_users",
                schema: "gf",
                columns: table => new
                {
                    uid = table.Column<long>(nullable: false),
                    reason = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blocked_users", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "bot_statuses",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    status = table.Column<string>(maxLength: 64, nullable: false),
                    activity_type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bot_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "chicken_upgrades",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(maxLength: 32, nullable: false),
                    cost = table.Column<long>(nullable: false),
                    stat = table.Column<int>(nullable: false),
                    mod = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chicken_upgrades", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "game_stats",
                schema: "gf",
                columns: table => new
                {
                    uid = table.Column<long>(nullable: false),
                    duel_won = table.Column<int>(nullable: false, defaultValue: 0),
                    duel_lost = table.Column<int>(nullable: false, defaultValue: 0),
                    hangman_won = table.Column<int>(nullable: false, defaultValue: 0),
                    quiz_won = table.Column<int>(nullable: false, defaultValue: 0),
                    ar_won = table.Column<int>(nullable: false, defaultValue: 0),
                    nr_won = table.Column<int>(nullable: false, defaultValue: 0),
                    ttt_won = table.Column<int>(nullable: false, defaultValue: 0),
                    ttt_lost = table.Column<int>(nullable: false, defaultValue: 0),
                    c4_won = table.Column<int>(nullable: false, defaultValue: 0),
                    c4_lost = table.Column<int>(nullable: false, defaultValue: 0),
                    caro_won = table.Column<int>(nullable: false, defaultValue: 0),
                    caro_lost = table.Column<int>(nullable: false, defaultValue: 0),
                    othello_won = table.Column<int>(nullable: false, defaultValue: 0),
                    othello_lost = table.Column<int>(nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_stats", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "guild_cfg",
                schema: "gf",
                columns: table => new
                {
                    gid = table.Column<long>(nullable: false),
                    prefix = table.Column<string>(maxLength: 8, nullable: true),
                    locale = table.Column<string>(maxLength: 8, nullable: true),
                    timezone_id = table.Column<string>(maxLength: 8, nullable: true),
                    currency = table.Column<string>(maxLength: 32, nullable: true),
                    suggestions_enabled = table.Column<bool>(nullable: false, defaultValue: false),
                    log_cid = table.Column<long>(nullable: true),
                    mute_rid = table.Column<long>(nullable: true),
                    silent_response_enabled = table.Column<bool>(nullable: false, defaultValue: false),
                    welcome_cid = table.Column<long>(nullable: true),
                    leave_cid = table.Column<long>(nullable: true),
                    welcome_msg = table.Column<string>(maxLength: 128, nullable: true),
                    leave_msg = table.Column<string>(maxLength: 128, nullable: true),
                    linkfilter_enabled = table.Column<bool>(nullable: false, defaultValue: false),
                    linkfilter_booters = table.Column<bool>(nullable: false, defaultValue: true),
                    linkfilter_disturbing = table.Column<bool>(nullable: false, defaultValue: true),
                    linkfilter_invites = table.Column<bool>(nullable: false, defaultValue: false),
                    linkfilter_loggers = table.Column<bool>(nullable: false, defaultValue: true),
                    linkfilter_shorteners = table.Column<bool>(nullable: false, defaultValue: true),
                    antiflood_enabled = table.Column<bool>(nullable: false, defaultValue: false),
                    antiflood_action = table.Column<byte>(nullable: false, defaultValue: (byte)4),
                    antiflood_sensitivity = table.Column<short>(nullable: false, defaultValue: (short)5),
                    antiflood_cooldown = table.Column<short>(nullable: false, defaultValue: (short)10),
                    antiinstantleave_enabled = table.Column<bool>(nullable: false, defaultValue: false),
                    antiinstantleave_cooldown = table.Column<short>(nullable: false, defaultValue: (short)3),
                    antispam_enabled = table.Column<bool>(nullable: false, defaultValue: false),
                    antispam_action = table.Column<byte>(nullable: false, defaultValue: (byte)0),
                    antispam_sensitivity = table.Column<short>(nullable: false, defaultValue: (short)5),
                    ratelimit_enabled = table.Column<bool>(nullable: false, defaultValue: false),
                    ratelimit_action = table.Column<byte>(nullable: false, defaultValue: (byte)1),
                    ratelimit_sensitivity = table.Column<short>(nullable: false, defaultValue: (short)5)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_cfg", x => x.gid);
                });

            migrationBuilder.CreateTable(
                name: "privileged_users",
                schema: "gf",
                columns: table => new
                {
                    uid = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_privileged_users", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "reminders",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uid = table.Column<long>(nullable: false),
                    execution_time = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    cid = table.Column<long>(nullable: true),
                    message = table.Column<string>(maxLength: 256, nullable: false),
                    is_repeating = table.Column<bool>(nullable: false, defaultValue: false),
                    repeat_interval = table.Column<TimeSpan>(type: "interval", nullable: true, defaultValue: new TimeSpan(0, 0, 0, 0, -1))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rss_feeds",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    url = table.Column<string>(nullable: false),
                    last_post_url = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rss_feeds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "swat_players",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(maxLength: 32, nullable: false),
                    additional_info = table.Column<string>(nullable: false),
                    is_blacklisted = table.Column<bool>(nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_swat_players", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "swat_servers",
                schema: "gf",
                columns: table => new
                {
                    ip = table.Column<string>(maxLength: 16, nullable: false),
                    join_port = table.Column<int>(nullable: false, defaultValue: 10480),
                    query_port = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_swat_servers", x => new { x.ip, x.join_port, x.query_port });
                });

            migrationBuilder.CreateTable(
                name: "xp_count",
                schema: "gf",
                columns: table => new
                {
                    uid = table.Column<long>(nullable: false),
                    xp = table.Column<int>(nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_xp_count", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "auto_roles",
                schema: "gf",
                columns: table => new
                {
                    gid = table.Column<long>(nullable: false),
                    rid = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auto_roles", x => new { x.gid, x.rid });
                    table.ForeignKey(
                        name: "FK_auto_roles_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bank_accounts",
                schema: "gf",
                columns: table => new
                {
                    uid = table.Column<long>(nullable: false),
                    gid = table.Column<long>(nullable: false),
                    balance = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_accounts", x => new { x.gid, x.uid });
                    table.ForeignKey(
                        name: "FK_bank_accounts_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "birthdays",
                schema: "gf",
                columns: table => new
                {
                    gid = table.Column<long>(nullable: false),
                    uid = table.Column<long>(nullable: false),
                    cid = table.Column<long>(nullable: false),
                    date = table.Column<DateTime>(type: "date", nullable: false),
                    last_update_year = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_birthdays", x => new { x.gid, x.cid, x.uid });
                    table.ForeignKey(
                        name: "FK_birthdays_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chickens",
                schema: "gf",
                columns: table => new
                {
                    uid = table.Column<long>(nullable: false),
                    gid = table.Column<long>(nullable: false),
                    name = table.Column<string>(maxLength: 32, nullable: false),
                    str = table.Column<int>(nullable: false),
                    vit = table.Column<int>(nullable: false),
                    max_vit = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chickens", x => new { x.gid, x.uid });
                    table.ForeignKey(
                        name: "FK_chickens_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cmd_rules",
                schema: "gf",
                columns: table => new
                {
                    gid = table.Column<long>(nullable: false),
                    cid = table.Column<long>(nullable: false),
                    command = table.Column<string>(maxLength: 32, nullable: false),
                    allow = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cmd_rules", x => new { x.gid, x.cid, x.command });
                    table.ForeignKey(
                        name: "FK_cmd_rules_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exempt_antispam",
                schema: "gf",
                columns: table => new
                {
                    xid = table.Column<long>(nullable: false),
                    gid = table.Column<long>(nullable: false),
                    type = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exempt_antispam", x => new { x.xid, x.gid, x.type });
                    table.ForeignKey(
                        name: "FK_exempt_antispam_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exempt_logging",
                schema: "gf",
                columns: table => new
                {
                    xid = table.Column<long>(nullable: false),
                    gid = table.Column<long>(nullable: false),
                    type = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exempt_logging", x => new { x.xid, x.gid, x.type });
                    table.ForeignKey(
                        name: "FK_exempt_logging_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exempt_ratelimit",
                schema: "gf",
                columns: table => new
                {
                    xid = table.Column<long>(nullable: false),
                    gid = table.Column<long>(nullable: false),
                    type = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exempt_ratelimit", x => new { x.xid, x.gid, x.type });
                    table.ForeignKey(
                        name: "FK_exempt_ratelimit_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "filters",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gid = table.Column<long>(nullable: false),
                    trigger = table.Column<string>(maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_filters", x => x.id);
                    table.ForeignKey(
                        name: "FK_filters_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "forbidden_names",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gid = table.Column<long>(nullable: false),
                    name_regex = table.Column<string>(maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forbidden_names", x => x.id);
                    table.ForeignKey(
                        name: "FK_forbidden_names_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "guild_ranks",
                schema: "gf",
                columns: table => new
                {
                    gid = table.Column<long>(nullable: false),
                    rank = table.Column<short>(nullable: false),
                    name = table.Column<string>(maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_ranks", x => new { x.gid, x.rank });
                    table.ForeignKey(
                        name: "FK_guild_ranks_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "insults",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gid = table.Column<long>(nullable: false),
                    content = table.Column<string>(maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_insults", x => x.id);
                    table.ForeignKey(
                        name: "FK_insults_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "memes",
                schema: "gf",
                columns: table => new
                {
                    gid = table.Column<long>(nullable: false),
                    name = table.Column<string>(maxLength: 32, nullable: false),
                    url = table.Column<string>(maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memes", x => new { x.gid, x.name });
                    table.ForeignKey(
                        name: "FK_memes_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchasable_items",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gid = table.Column<long>(nullable: false),
                    name = table.Column<string>(maxLength: 64, nullable: false),
                    price = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchasable_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_purchasable_items_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reactions_emoji",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gid = table.Column<long>(nullable: false),
                    reaction = table.Column<string>(maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reactions_emoji", x => x.id);
                    table.ForeignKey(
                        name: "FK_reactions_emoji_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reactions_text",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gid = table.Column<long>(nullable: false),
                    reaction = table.Column<string>(maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reactions_text", x => x.id);
                    table.ForeignKey(
                        name: "FK_reactions_text_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scheduled_tasks",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uid = table.Column<long>(nullable: false),
                    execution_time = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    gid = table.Column<long>(nullable: false),
                    rid = table.Column<long>(nullable: true),
                    type = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduled_tasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_scheduled_tasks_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "self_roles",
                schema: "gf",
                columns: table => new
                {
                    gid = table.Column<long>(nullable: false),
                    rid = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_self_roles", x => new { x.gid, x.rid });
                    table.ForeignKey(
                        name: "FK_self_roles_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rss_subscriptions",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    gid = table.Column<long>(nullable: false),
                    cid = table.Column<long>(nullable: false),
                    name = table.Column<string>(maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rss_subscriptions", x => new { x.id, x.gid, x.cid });
                    table.ForeignKey(
                        name: "FK_rss_subscriptions_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rss_subscriptions_rss_feeds_id",
                        column: x => x.id,
                        principalSchema: "gf",
                        principalTable: "rss_feeds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "swat_aliases",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    alias = table.Column<string>(maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_swat_aliases", x => new { x.alias, x.id });
                    table.ForeignKey(
                        name: "FK_swat_aliases_swat_players_id",
                        column: x => x.id,
                        principalSchema: "gf",
                        principalTable: "swat_players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "swat_ips",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    ip = table.Column<string>(maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_swat_ips", x => new { x.ip, x.id });
                    table.ForeignKey(
                        name: "FK_swat_ips_swat_players_id",
                        column: x => x.id,
                        principalSchema: "gf",
                        principalTable: "swat_players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chicken_bought_upgrades",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    uid = table.Column<long>(nullable: false),
                    gid = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chicken_bought_upgrades", x => new { x.id, x.gid, x.uid });
                    table.ForeignKey(
                        name: "FK_chicken_bought_upgrades_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_chicken_bought_upgrades_chicken_upgrades_id",
                        column: x => x.id,
                        principalSchema: "gf",
                        principalTable: "chicken_upgrades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_chicken_bought_upgrades_chickens_gid_uid",
                        columns: x => new { x.gid, x.uid },
                        principalSchema: "gf",
                        principalTable: "chickens",
                        principalColumns: new[] { "gid", "uid" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchased_items",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    uid = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchased_items", x => new { x.id, x.uid });
                    table.ForeignKey(
                        name: "FK_purchased_items_purchasable_items_id",
                        column: x => x.id,
                        principalSchema: "gf",
                        principalTable: "purchasable_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reactions_emoji_triggers",
                schema: "gf",
                columns: table => new
                {
                    trigger = table.Column<string>(maxLength: 128, nullable: false),
                    id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reactions_emoji_triggers", x => new { x.id, x.trigger });
                    table.ForeignKey(
                        name: "FK_reactions_emoji_triggers_reactions_emoji_id",
                        column: x => x.id,
                        principalSchema: "gf",
                        principalTable: "reactions_emoji",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reactions_text_triggers",
                schema: "gf",
                columns: table => new
                {
                    trigger = table.Column<string>(maxLength: 128, nullable: false),
                    id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reactions_text_triggers", x => new { x.id, x.trigger });
                    table.ForeignKey(
                        name: "FK_reactions_text_triggers_reactions_text_id",
                        column: x => x.id,
                        principalSchema: "gf",
                        principalTable: "reactions_text",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chicken_bought_upgrades_gid_uid",
                schema: "gf",
                table: "chicken_bought_upgrades",
                columns: new[] { "gid", "uid" });

            migrationBuilder.CreateIndex(
                name: "IX_exempt_antispam_gid",
                schema: "gf",
                table: "exempt_antispam",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_exempt_logging_gid",
                schema: "gf",
                table: "exempt_logging",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_exempt_ratelimit_gid",
                schema: "gf",
                table: "exempt_ratelimit",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_filters_gid",
                schema: "gf",
                table: "filters",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_forbidden_names_gid",
                schema: "gf",
                table: "forbidden_names",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_insults_gid",
                schema: "gf",
                table: "insults",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_purchasable_items_gid",
                schema: "gf",
                table: "purchasable_items",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_reactions_emoji_gid",
                schema: "gf",
                table: "reactions_emoji",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_reactions_text_gid",
                schema: "gf",
                table: "reactions_text",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_rss_subscriptions_gid",
                schema: "gf",
                table: "rss_subscriptions",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_tasks_gid",
                schema: "gf",
                table: "scheduled_tasks",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_swat_aliases_id",
                schema: "gf",
                table: "swat_aliases",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_swat_ips_id",
                schema: "gf",
                table: "swat_ips",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_swat_players_name",
                schema: "gf",
                table: "swat_players",
                column: "name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auto_roles",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "bank_accounts",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "birthdays",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "blocked_channels",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "blocked_users",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "bot_statuses",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "chicken_bought_upgrades",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "cmd_rules",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "exempt_antispam",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "exempt_logging",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "exempt_ratelimit",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "filters",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "forbidden_names",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "game_stats",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "guild_ranks",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "insults",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "memes",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "privileged_users",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "purchased_items",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "reactions_emoji_triggers",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "reactions_text_triggers",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "reminders",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "rss_subscriptions",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "scheduled_tasks",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "self_roles",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "swat_aliases",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "swat_ips",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "swat_servers",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "xp_count",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "chicken_upgrades",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "chickens",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "purchasable_items",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "reactions_emoji",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "reactions_text",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "rss_feeds",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "swat_players",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "guild_cfg",
                schema: "gf");
        }
    }
}
