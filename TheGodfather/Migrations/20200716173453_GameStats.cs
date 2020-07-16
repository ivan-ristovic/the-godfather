using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class GameStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "game_stats",
                schema: "gf",
                columns: table => new
                {
                    uid = table.Column<long>(nullable: false),
                    duel_won = table.Column<int>(nullable: false, defaultValue: 0),
                    duel_lost = table.Column<int>(nullable: false, defaultValue: 0),
                    hangman_won = table.Column<int>(nullable: false, defaultValue: 0),
                    quiz_won = table.Column<int>(nullable: false, defaultValue: 0),
                    ar_won = table.Column<int>(nullable: false, defaultValue: 0),
                    nr_won = table.Column<int>(nullable: false, defaultValue: 0),
                    ttt_won = table.Column<int>(nullable: false, defaultValue: 0),
                    ttt_lost = table.Column<int>(nullable: false, defaultValue: 0),
                    c4_won = table.Column<int>(nullable: false, defaultValue: 0),
                    c4_lost = table.Column<int>(nullable: false, defaultValue: 0),
                    caro_won = table.Column<int>(nullable: false, defaultValue: 0),
                    caro_lost = table.Column<int>(nullable: false, defaultValue: 0),
                    othello_won = table.Column<int>(nullable: false, defaultValue: 0),
                    othello_lost = table.Column<int>(nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_stats", x => x.uid);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_stats",
                schema: "gf");
        }
    }
}
