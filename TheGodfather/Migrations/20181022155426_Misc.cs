using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class Misc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "quiz_won",
                schema: "gf",
                table: "game_stats",
                newName: "quizes_won");

            migrationBuilder.RenameColumn(
                name: "numraces_won",
                schema: "gf",
                table: "game_stats",
                newName: "numberraces_won");

            migrationBuilder.RenameColumn(
                name: "animalrace_won",
                schema: "gf",
                table: "game_stats",
                newName: "animalraces_won");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                schema: "gf",
                table: "chickens",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "quizes_won",
                schema: "gf",
                table: "game_stats",
                newName: "quiz_won");

            migrationBuilder.RenameColumn(
                name: "numberraces_won",
                schema: "gf",
                table: "game_stats",
                newName: "numraces_won");

            migrationBuilder.RenameColumn(
                name: "animalraces_won",
                schema: "gf",
                table: "game_stats",
                newName: "animalrace_won");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                schema: "gf",
                table: "chickens",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 32);
        }
    }
}
