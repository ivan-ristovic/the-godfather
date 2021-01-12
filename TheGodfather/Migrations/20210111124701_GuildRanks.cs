using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class GuildRanks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_xp_count",
                schema: "gf",
                table: "xp_count");

            migrationBuilder.AddColumn<long>(
                name: "gid",
                schema: "gf",
                table: "xp_count",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_xp_count",
                schema: "gf",
                table: "xp_count",
                columns: new[] { "gid", "uid" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_xp_count",
                schema: "gf",
                table: "xp_count");

            migrationBuilder.DropColumn(
                name: "gid",
                schema: "gf",
                table: "xp_count");

            migrationBuilder.AddPrimaryKey(
                name: "PK_xp_count",
                schema: "gf",
                table: "xp_count",
                column: "uid");
        }
    }
}
