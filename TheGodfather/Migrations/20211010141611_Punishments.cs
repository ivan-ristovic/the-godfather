using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class Punishments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "ratelimit_action",
                schema: "gf",
                table: "guild_cfg",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "smallint",
                oldDefaultValue: (byte)1);

            migrationBuilder.CreateTable(
                name: "punishments",
                schema: "gf",
                columns: table => new
                {
                    uid = table.Column<long>(type: "bigint", nullable: false),
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    action = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_punishments", x => new { x.gid, x.uid, x.action });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "punishments",
                schema: "gf");

            migrationBuilder.AlterColumn<byte>(
                name: "ratelimit_action",
                schema: "gf",
                table: "guild_cfg",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)1,
                oldClrType: typeof(byte),
                oldType: "smallint",
                oldDefaultValue: (byte)0);
        }
    }
}
