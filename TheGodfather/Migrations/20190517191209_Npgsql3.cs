using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class Npgsql3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_chicken_bought_upgrades_guild_cfg_gid",
                schema: "gf",
                table: "chicken_bought_upgrades");

            migrationBuilder.DropForeignKey(
                name: "FK_chickens_guild_cfg_gid",
                schema: "gf",
                table: "chickens");

            migrationBuilder.DropForeignKey(
                name: "FK_cmd_rules_guild_cfg_gid",
                schema: "gf",
                table: "cmd_rules");

            migrationBuilder.AlterColumn<short>(
                name: "rank",
                schema: "gf",
                table: "guild_ranks",
                nullable: false,
                oldClrType: typeof(short))
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<byte>(
                name: "ratelimit_action",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: (byte)1,
                oldClrType: typeof(byte),
                oldDefaultValue: (byte)1)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<byte>(
                name: "antispam_action",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldDefaultValue: (byte)0)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<byte>(
                name: "antiflood_action",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: (byte)4,
                oldClrType: typeof(byte),
                oldDefaultValue: (byte)4)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<long>(
                name: "DbGuildConfigGuildIdDb",
                schema: "gf",
                table: "cmd_rules",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DbGuildConfigGuildIdDb",
                schema: "gf",
                table: "chickens",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DatabaseGuildConfigGuildIdDb",
                schema: "gf",
                table: "chicken_bought_upgrades",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_cmd_rules_DbGuildConfigGuildIdDb",
                schema: "gf",
                table: "cmd_rules",
                column: "DbGuildConfigGuildIdDb");

            migrationBuilder.CreateIndex(
                name: "IX_chickens_DbGuildConfigGuildIdDb",
                schema: "gf",
                table: "chickens",
                column: "DbGuildConfigGuildIdDb");

            migrationBuilder.CreateIndex(
                name: "IX_chicken_bought_upgrades_DatabaseGuildConfigGuildIdDb",
                schema: "gf",
                table: "chicken_bought_upgrades",
                column: "DatabaseGuildConfigGuildIdDb");

            migrationBuilder.AddForeignKey(
                name: "FK_chicken_bought_upgrades_guild_cfg_DatabaseGuildConfigGuildIdDb",
                schema: "gf",
                table: "chicken_bought_upgrades",
                column: "DatabaseGuildConfigGuildIdDb",
                principalSchema: "gf",
                principalTable: "guild_cfg",
                principalColumn: "gid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_chickens_guild_cfg_DbGuildConfigGuildIdDb",
                schema: "gf",
                table: "chickens",
                column: "DbGuildConfigGuildIdDb",
                principalSchema: "gf",
                principalTable: "guild_cfg",
                principalColumn: "gid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_cmd_rules_guild_cfg_DbGuildConfigGuildIdDb",
                schema: "gf",
                table: "cmd_rules",
                column: "DbGuildConfigGuildIdDb",
                principalSchema: "gf",
                principalTable: "guild_cfg",
                principalColumn: "gid",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_chicken_bought_upgrades_guild_cfg_DatabaseGuildConfigGuildIdDb",
                schema: "gf",
                table: "chicken_bought_upgrades");

            migrationBuilder.DropForeignKey(
                name: "FK_chickens_guild_cfg_DbGuildConfigGuildIdDb",
                schema: "gf",
                table: "chickens");

            migrationBuilder.DropForeignKey(
                name: "FK_cmd_rules_guild_cfg_DbGuildConfigGuildIdDb",
                schema: "gf",
                table: "cmd_rules");

            migrationBuilder.DropIndex(
                name: "IX_cmd_rules_DbGuildConfigGuildIdDb",
                schema: "gf",
                table: "cmd_rules");

            migrationBuilder.DropIndex(
                name: "IX_chickens_DbGuildConfigGuildIdDb",
                schema: "gf",
                table: "chickens");

            migrationBuilder.DropIndex(
                name: "IX_chicken_bought_upgrades_DatabaseGuildConfigGuildIdDb",
                schema: "gf",
                table: "chicken_bought_upgrades");

            migrationBuilder.DropColumn(
                name: "DbGuildConfigGuildIdDb",
                schema: "gf",
                table: "cmd_rules");

            migrationBuilder.DropColumn(
                name: "DbGuildConfigGuildIdDb",
                schema: "gf",
                table: "chickens");

            migrationBuilder.DropColumn(
                name: "DatabaseGuildConfigGuildIdDb",
                schema: "gf",
                table: "chicken_bought_upgrades");

            migrationBuilder.AlterColumn<short>(
                name: "rank",
                schema: "gf",
                table: "guild_ranks",
                nullable: false,
                oldClrType: typeof(short))
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<byte>(
                name: "ratelimit_action",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: (byte)1,
                oldClrType: typeof(byte),
                oldDefaultValue: (byte)1)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<byte>(
                name: "antispam_action",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldDefaultValue: (byte)0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<byte>(
                name: "antiflood_action",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: (byte)4,
                oldClrType: typeof(byte),
                oldDefaultValue: (byte)4)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddForeignKey(
                name: "FK_chicken_bought_upgrades_guild_cfg_gid",
                schema: "gf",
                table: "chicken_bought_upgrades",
                column: "gid",
                principalSchema: "gf",
                principalTable: "guild_cfg",
                principalColumn: "gid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_chickens_guild_cfg_gid",
                schema: "gf",
                table: "chickens",
                column: "gid",
                principalSchema: "gf",
                principalTable: "guild_cfg",
                principalColumn: "gid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_cmd_rules_guild_cfg_gid",
                schema: "gf",
                table: "cmd_rules",
                column: "gid",
                principalSchema: "gf",
                principalTable: "guild_cfg",
                principalColumn: "gid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
