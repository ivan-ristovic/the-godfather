using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class Swat4UniqueName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_swat_players_name",
                schema: "gf",
                table: "swat_players",
                column: "name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_swat_players_name",
                schema: "gf",
                table: "swat_players");
        }
    }
}
