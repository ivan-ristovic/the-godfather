using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class TemporaryActionTimeCustomization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "temp_ban_cooldown",
                schema: "gf",
                table: "guild_cfg",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "temp_mute_cooldown",
                schema: "gf",
                table: "guild_cfg",
                type: "interval",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "temp_ban_cooldown",
                schema: "gf",
                table: "guild_cfg");

            migrationBuilder.DropColumn(
                name: "temp_mute_cooldown",
                schema: "gf",
                table: "guild_cfg");
        }
    }
}
