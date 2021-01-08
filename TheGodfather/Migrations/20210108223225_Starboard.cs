using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class Starboard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "starboard_cid",
                schema: "gf",
                table: "guild_cfg",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "starboard_emoji",
                schema: "gf",
                table: "guild_cfg",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "starboard_sens",
                schema: "gf",
                table: "guild_cfg",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "starboard",
                schema: "gf",
                columns: table => new
                {
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    cid = table.Column<long>(type: "bigint", nullable: false),
                    mid = table.Column<long>(type: "bigint", nullable: false),
                    smid = table.Column<long>(type: "bigint", nullable: false),
                    stars = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_starboard", x => new { x.gid, x.cid, x.mid });
                    table.ForeignKey(
                        name: "FK_starboard_guild_cfg_gid",
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
                name: "starboard",
                schema: "gf");

            migrationBuilder.DropColumn(
                name: "starboard_cid",
                schema: "gf",
                table: "guild_cfg");

            migrationBuilder.DropColumn(
                name: "starboard_emoji",
                schema: "gf",
                table: "guild_cfg");

            migrationBuilder.DropColumn(
                name: "starboard_sens",
                schema: "gf",
                table: "guild_cfg");
        }
    }
}
