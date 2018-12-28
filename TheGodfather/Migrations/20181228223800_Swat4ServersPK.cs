using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class Swat4ServersPK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_swat_servers",
                schema: "gf",
                table: "swat_servers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_swat_servers",
                schema: "gf",
                table: "swat_servers",
                columns: new[] { "ip", "join_port", "query_port" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_swat_servers",
                schema: "gf",
                table: "swat_servers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_swat_servers",
                schema: "gf",
                table: "swat_servers",
                column: "ip");
        }
    }
}
