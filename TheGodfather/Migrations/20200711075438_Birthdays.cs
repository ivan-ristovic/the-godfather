using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class Birthdays : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "birthdays",
                schema: "gf");
        }
    }
}
