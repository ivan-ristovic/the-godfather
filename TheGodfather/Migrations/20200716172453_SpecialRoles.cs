using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class SpecialRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auto_roles",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "memes",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "self_roles",
                schema: "gf");
        }
    }
}
