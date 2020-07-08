using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class Xp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "guild_ranks",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "xp_count",
                schema: "gf");
        }
    }
}
