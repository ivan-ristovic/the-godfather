using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class ReactionRolesEnhancement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_reaction_roles",
                schema: "gf",
                table: "reaction_roles");

            migrationBuilder.AddColumn<long>(
                name: "cid",
                schema: "gf",
                table: "reaction_roles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "mid",
                schema: "gf",
                table: "reaction_roles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_reaction_roles",
                schema: "gf",
                table: "reaction_roles",
                columns: new[] { "gid", "emoji", "cid", "mid" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_reaction_roles",
                schema: "gf",
                table: "reaction_roles");

            migrationBuilder.DropColumn(
                name: "cid",
                schema: "gf",
                table: "reaction_roles");

            migrationBuilder.DropColumn(
                name: "mid",
                schema: "gf",
                table: "reaction_roles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_reaction_roles",
                schema: "gf",
                table: "reaction_roles",
                columns: new[] { "gid", "emoji" });
        }
    }
}
