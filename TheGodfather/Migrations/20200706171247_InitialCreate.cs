using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TheGodfather.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "gf");

            migrationBuilder.CreateTable(
                name: "blocked_channels",
                schema: "gf",
                columns: table => new
                {
                    cid = table.Column<long>(nullable: false),
                    reason = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blocked_channels", x => x.cid);
                });

            migrationBuilder.CreateTable(
                name: "blocked_users",
                schema: "gf",
                columns: table => new
                {
                    uid = table.Column<long>(nullable: false),
                    reason = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blocked_users", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "bot_statuses",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    status = table.Column<string>(maxLength: 64, nullable: false),
                    activity_type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bot_statuses", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blocked_channels",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "blocked_users",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "bot_statuses",
                schema: "gf");
        }
    }
}
