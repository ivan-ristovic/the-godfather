using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class Exempts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exempt_antispam",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "exempt_logging",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "exempt_ratelimit",
                schema: "gf");
        }
    }
}
