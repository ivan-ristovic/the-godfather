using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class CommandRules : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cmd_rules",
                schema: "gf",
                columns: table => new
                {
                    gid = table.Column<long>(nullable: false),
                    cid = table.Column<long>(nullable: false),
                    commands = table.Column<string>(maxLength: 32, nullable: false),
                    allow = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cmd_rules", x => new { x.gid, x.cid, x.commands });
                    table.ForeignKey(
                        name: "FK_cmd_rules_guild_cfg_gid",
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
                name: "cmd_rules",
                schema: "gf");
        }
    }
}
