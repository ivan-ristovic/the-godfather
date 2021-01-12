using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class LevelRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "level_roles",
                schema: "gf",
                columns: table => new
                {
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    rank = table.Column<short>(type: "smallint", nullable: false),
                    rid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_level_roles", x => new { x.gid, x.rank });
                    table.ForeignKey(
                        name: "FK_level_roles_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "level_roles",
                schema: "gf");
        }
    }
}
