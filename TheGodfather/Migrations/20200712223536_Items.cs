using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TheGodfather.Migrations
{
    public partial class Items : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "purchasable_items",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gid = table.Column<long>(nullable: false),
                    name = table.Column<string>(maxLength: 64, nullable: false),
                    price = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchasable_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_purchasable_items_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchased_items",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    uid = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchased_items", x => new { x.id, x.uid });
                    table.ForeignKey(
                        name: "FK_purchased_items_purchasable_items_id",
                        column: x => x.id,
                        principalSchema: "gf",
                        principalTable: "purchasable_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_purchasable_items_gid",
                schema: "gf",
                table: "purchasable_items",
                column: "gid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "purchased_items",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "purchasable_items",
                schema: "gf");
        }
    }
}
