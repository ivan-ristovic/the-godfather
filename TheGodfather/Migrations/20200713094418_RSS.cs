using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TheGodfather.Migrations
{
    public partial class RSS : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rss_feeds",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    url = table.Column<string>(nullable: false),
                    last_post_url = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rss_feeds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rss_subscriptions",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    gid = table.Column<long>(nullable: false),
                    cid = table.Column<long>(nullable: false),
                    name = table.Column<string>(maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rss_subscriptions", x => new { x.id, x.gid, x.cid });
                    table.ForeignKey(
                        name: "FK_rss_subscriptions_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rss_subscriptions_rss_feeds_id",
                        column: x => x.id,
                        principalSchema: "gf",
                        principalTable: "rss_feeds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rss_subscriptions_gid",
                schema: "gf",
                table: "rss_subscriptions",
                column: "gid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rss_subscriptions",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "rss_feeds",
                schema: "gf");
        }
    }
}
