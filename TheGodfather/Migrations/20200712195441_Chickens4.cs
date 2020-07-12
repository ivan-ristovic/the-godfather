using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class Chickens4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_chicken_upgrades_chickens_ChickenGuildIdDb_ChickenUserIdDb",
                schema: "gf",
                table: "chicken_upgrades");

            migrationBuilder.DropForeignKey(
                name: "FK_cmd_rules_guild_cfg_GuildConfigGuildIdDb",
                schema: "gf",
                table: "cmd_rules");

            migrationBuilder.DropIndex(
                name: "IX_chicken_upgrades_ChickenGuildIdDb_ChickenUserIdDb",
                schema: "gf",
                table: "chicken_upgrades");

            migrationBuilder.DropColumn(
                name: "ChickenGuildIdDb",
                schema: "gf",
                table: "chicken_upgrades");

            migrationBuilder.DropColumn(
                name: "ChickenUserIdDb",
                schema: "gf",
                table: "chicken_upgrades");

            migrationBuilder.AlterColumn<long>(
                name: "GuildConfigGuildIdDb",
                schema: "gf",
                table: "cmd_rules",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "chicken_bought_upgrades",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    uid = table.Column<long>(nullable: false),
                    gid = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chicken_bought_upgrades", x => new { x.id, x.gid, x.uid });
                    table.ForeignKey(
                        name: "FK_chicken_bought_upgrades_chicken_upgrades_id",
                        column: x => x.id,
                        principalSchema: "gf",
                        principalTable: "chicken_upgrades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_chicken_bought_upgrades_chickens_gid_uid",
                        columns: x => new { x.gid, x.uid },
                        principalSchema: "gf",
                        principalTable: "chickens",
                        principalColumns: new[] { "gid", "uid" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chicken_bought_upgrades_gid_uid",
                schema: "gf",
                table: "chicken_bought_upgrades",
                columns: new[] { "gid", "uid" });

            migrationBuilder.AddForeignKey(
                name: "FK_cmd_rules_guild_cfg_GuildConfigGuildIdDb",
                schema: "gf",
                table: "cmd_rules",
                column: "GuildConfigGuildIdDb",
                principalSchema: "gf",
                principalTable: "guild_cfg",
                principalColumn: "gid",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cmd_rules_guild_cfg_GuildConfigGuildIdDb",
                schema: "gf",
                table: "cmd_rules");

            migrationBuilder.DropTable(
                name: "chicken_bought_upgrades",
                schema: "gf");

            migrationBuilder.AlterColumn<long>(
                name: "GuildConfigGuildIdDb",
                schema: "gf",
                table: "cmd_rules",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AddColumn<long>(
                name: "ChickenGuildIdDb",
                schema: "gf",
                table: "chicken_upgrades",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ChickenUserIdDb",
                schema: "gf",
                table: "chicken_upgrades",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_chicken_upgrades_ChickenGuildIdDb_ChickenUserIdDb",
                schema: "gf",
                table: "chicken_upgrades",
                columns: new[] { "ChickenGuildIdDb", "ChickenUserIdDb" });

            migrationBuilder.AddForeignKey(
                name: "FK_chicken_upgrades_chickens_ChickenGuildIdDb_ChickenUserIdDb",
                schema: "gf",
                table: "chicken_upgrades",
                columns: new[] { "ChickenGuildIdDb", "ChickenUserIdDb" },
                principalSchema: "gf",
                principalTable: "chickens",
                principalColumns: new[] { "gid", "uid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_cmd_rules_guild_cfg_GuildConfigGuildIdDb",
                schema: "gf",
                table: "cmd_rules",
                column: "GuildConfigGuildIdDb",
                principalSchema: "gf",
                principalTable: "guild_cfg",
                principalColumn: "gid",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
