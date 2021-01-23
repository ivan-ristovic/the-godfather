using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class AntiMention : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "antimention_action",
                schema: "gf",
                table: "guild_cfg",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<bool>(
                name: "antimention_enabled",
                schema: "gf",
                table: "guild_cfg",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<short>(
                name: "antimention_sensitivity",
                schema: "gf",
                table: "guild_cfg",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateTable(
                name: "exempt_mention",
                schema: "gf",
                columns: table => new
                {
                    xid = table.Column<long>(type: "bigint", nullable: false),
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exempt_mention", x => new { x.xid, x.gid, x.type });
                    table.ForeignKey(
                        name: "FK_exempt_mention_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exempt_mention_gid",
                schema: "gf",
                table: "exempt_mention",
                column: "gid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exempt_mention",
                schema: "gf");

            migrationBuilder.DropColumn(
                name: "antimention_action",
                schema: "gf",
                table: "guild_cfg");

            migrationBuilder.DropColumn(
                name: "antimention_enabled",
                schema: "gf",
                table: "guild_cfg");

            migrationBuilder.DropColumn(
                name: "antimention_sensitivity",
                schema: "gf",
                table: "guild_cfg");
        }
    }
}
