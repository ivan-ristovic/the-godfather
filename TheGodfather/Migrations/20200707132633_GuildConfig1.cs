using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class GuildConfig1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "guild_cfg",
                schema: "gf",
                columns: table => new
                {
                    gid = table.Column<long>(nullable: false),
                    prefix = table.Column<string>(maxLength: 8, nullable: true),
                    locale = table.Column<string>(maxLength: 8, nullable: true),
                    currency = table.Column<string>(maxLength: 32, nullable: true),
                    suggestions_enabled = table.Column<bool>(nullable: false),
                    log_cid = table.Column<long>(nullable: true),
                    mute_rid = table.Column<long>(nullable: true),
                    silent_response_enabled = table.Column<bool>(nullable: false),
                    welcome_cid = table.Column<long>(nullable: true),
                    leave_cid = table.Column<long>(nullable: true),
                    welcome_msg = table.Column<string>(maxLength: 128, nullable: true),
                    leave_msg = table.Column<string>(maxLength: 128, nullable: true),
                    linkfilter_enabled = table.Column<bool>(nullable: false),
                    linkfilter_booters = table.Column<bool>(nullable: false),
                    linkfilter_disturbing = table.Column<bool>(nullable: false),
                    linkfilter_invites = table.Column<bool>(nullable: false),
                    linkfilter_loggers = table.Column<bool>(nullable: false),
                    linkfilter_shorteners = table.Column<bool>(nullable: false),
                    antiflood_enabled = table.Column<bool>(nullable: false),
                    antiflood_action = table.Column<byte>(nullable: false),
                    antiflood_sensitivity = table.Column<short>(nullable: false),
                    antiflood_cooldown = table.Column<short>(nullable: false),
                    antiinstantleave_enabled = table.Column<bool>(nullable: false),
                    antiinstantleave_cooldown = table.Column<short>(nullable: false),
                    antispam_enabled = table.Column<bool>(nullable: false),
                    antispam_action = table.Column<byte>(nullable: false),
                    antispam_sensitivity = table.Column<short>(nullable: false),
                    ratelimit_enabled = table.Column<bool>(nullable: false),
                    ratelimit_action = table.Column<byte>(nullable: false),
                    ratelimit_sensitivity = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_cfg", x => x.gid);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "guild_cfg",
                schema: "gf");
        }
    }
}
