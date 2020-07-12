using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class Chickens3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_chicken_upgrades_chickens_ChickenGuildIdDb_ChickenUserIdDb",
                schema: "gf",
                table: "chicken_upgrades");

            migrationBuilder.AlterColumn<long>(
                name: "ChickenUserIdDb",
                schema: "gf",
                table: "chicken_upgrades",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "ChickenGuildIdDb",
                schema: "gf",
                table: "chicken_upgrades",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_chicken_upgrades_chickens_ChickenGuildIdDb_ChickenUserIdDb",
                schema: "gf",
                table: "chicken_upgrades",
                columns: new[] { "ChickenGuildIdDb", "ChickenUserIdDb" },
                principalSchema: "gf",
                principalTable: "chickens",
                principalColumns: new[] { "gid", "uid" },
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_chicken_upgrades_chickens_ChickenGuildIdDb_ChickenUserIdDb",
                schema: "gf",
                table: "chicken_upgrades");

            migrationBuilder.AlterColumn<long>(
                name: "ChickenUserIdDb",
                schema: "gf",
                table: "chicken_upgrades",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ChickenGuildIdDb",
                schema: "gf",
                table: "chicken_upgrades",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_chicken_upgrades_chickens_ChickenGuildIdDb_ChickenUserIdDb",
                schema: "gf",
                table: "chicken_upgrades",
                columns: new[] { "ChickenGuildIdDb", "ChickenUserIdDb" },
                principalSchema: "gf",
                principalTable: "chickens",
                principalColumns: new[] { "gid", "uid" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
