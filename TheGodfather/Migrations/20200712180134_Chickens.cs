using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TheGodfather.Migrations
{
    public partial class Chickens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bank_accounts",
                schema: "gf",
                columns: table => new
                {
                    uid = table.Column<long>(nullable: false),
                    gid = table.Column<long>(nullable: false),
                    balance = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_accounts", x => new { x.gid, x.uid });
                    table.ForeignKey(
                        name: "FK_bank_accounts_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chickens",
                schema: "gf",
                columns: table => new
                {
                    uid = table.Column<long>(nullable: false),
                    gid = table.Column<long>(nullable: false),
                    name = table.Column<string>(maxLength: 32, nullable: false),
                    str = table.Column<int>(nullable: false),
                    vit = table.Column<int>(nullable: false),
                    max_vit = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chickens", x => new { x.gid, x.uid });
                    table.ForeignKey(
                        name: "FK_chickens_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chicken_upgrades",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(maxLength: 32, nullable: false),
                    cost = table.Column<long>(nullable: false),
                    stat = table.Column<int>(nullable: false),
                    mod = table.Column<int>(nullable: false),
                    ChickenGuildIdDb = table.Column<long>(nullable: false),
                    ChickenUserIdDb = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chicken_upgrades", x => x.id);
                    table.ForeignKey(
                        name: "FK_chicken_upgrades_chickens_ChickenGuildIdDb_ChickenUserIdDb",
                        columns: x => new { x.ChickenGuildIdDb, x.ChickenUserIdDb },
                        principalSchema: "gf",
                        principalTable: "chickens",
                        principalColumns: new[] { "gid", "uid" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chicken_upgrades_ChickenGuildIdDb_ChickenUserIdDb",
                schema: "gf",
                table: "chicken_upgrades",
                columns: new[] { "ChickenGuildIdDb", "ChickenUserIdDb" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bank_accounts",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "chicken_upgrades",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "chickens",
                schema: "gf");
        }
    }
}
