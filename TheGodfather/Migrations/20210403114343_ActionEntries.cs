using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class ActionEntries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "action_history_enabled",
                schema: "gf",
                table: "guild_cfg",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "action_history",
                schema: "gf",
                columns: table => new
                {
                    uid = table.Column<long>(type: "bigint", nullable: false),
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    execution_time = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    action = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    GuildConfigGuildIdDb = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_action_history", x => new { x.gid, x.uid, x.execution_time });
                    table.ForeignKey(
                        name: "FK_action_history_guild_cfg_GuildConfigGuildIdDb",
                        column: x => x.GuildConfigGuildIdDb,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_action_history_GuildConfigGuildIdDb",
                schema: "gf",
                table: "action_history",
                column: "GuildConfigGuildIdDb");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "action_history",
                schema: "gf");

            migrationBuilder.DropColumn(
                name: "action_history_enabled",
                schema: "gf",
                table: "guild_cfg");
        }
    }
}
