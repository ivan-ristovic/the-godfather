using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class GuildConfig2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "welcome_msg",
                schema: "gf",
                table: "guild_cfg",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "welcome_cid",
                schema: "gf",
                table: "guild_cfg",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "suggestions_enabled",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "silent_response_enabled",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<short>(
                name: "ratelimit_sensitivity",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: (short)5,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<bool>(
                name: "ratelimit_enabled",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<byte>(
                name: "ratelimit_action",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: (byte)1,
                oldClrType: typeof(byte),
                oldType: "smallint");

            migrationBuilder.AlterColumn<string>(
                name: "prefix",
                schema: "gf",
                table: "guild_cfg",
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(8)",
                oldMaxLength: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "mute_rid",
                schema: "gf",
                table: "guild_cfg",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "log_cid",
                schema: "gf",
                table: "guild_cfg",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "linkfilter_shorteners",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "linkfilter_loggers",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "linkfilter_enabled",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "linkfilter_disturbing",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "linkfilter_invites",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "linkfilter_booters",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "leave_msg",
                schema: "gf",
                table: "guild_cfg",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "leave_cid",
                schema: "gf",
                table: "guild_cfg",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                schema: "gf",
                table: "guild_cfg",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "antispam_sensitivity",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: (short)5,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<bool>(
                name: "antispam_enabled",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<byte>(
                name: "antispam_action",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "smallint");

            migrationBuilder.AlterColumn<short>(
                name: "antiflood_sensitivity",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: (short)5,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<bool>(
                name: "antiflood_enabled",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<short>(
                name: "antiflood_cooldown",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: (short)10,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<byte>(
                name: "antiflood_action",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: (byte)4,
                oldClrType: typeof(byte),
                oldType: "smallint");

            migrationBuilder.AlterColumn<bool>(
                name: "antiinstantleave_enabled",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<short>(
                name: "antiinstantleave_cooldown",
                schema: "gf",
                table: "guild_cfg",
                nullable: false,
                defaultValue: (short)3,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<string>(
                name: "reason",
                schema: "gf",
                table: "blocked_users",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "reason",
                schema: "gf",
                table: "blocked_channels",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "welcome_msg",
                schema: "gf",
                table: "guild_cfg",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "welcome_cid",
                schema: "gf",
                table: "guild_cfg",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "suggestions_enabled",
                schema: "gf",
                table: "guild_cfg",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "silent_response_enabled",
                schema: "gf",
                table: "guild_cfg",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<short>(
                name: "ratelimit_sensitivity",
                schema: "gf",
                table: "guild_cfg",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(short),
                oldDefaultValue: (short)5);

            migrationBuilder.AlterColumn<bool>(
                name: "ratelimit_enabled",
                schema: "gf",
                table: "guild_cfg",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<byte>(
                name: "ratelimit_action",
                schema: "gf",
                table: "guild_cfg",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(byte),
                oldDefaultValue: (byte)1);

            migrationBuilder.AlterColumn<string>(
                name: "prefix",
                schema: "gf",
                table: "guild_cfg",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "mute_rid",
                schema: "gf",
                table: "guild_cfg",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "log_cid",
                schema: "gf",
                table: "guild_cfg",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "linkfilter_shorteners",
                schema: "gf",
                table: "guild_cfg",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "linkfilter_loggers",
                schema: "gf",
                table: "guild_cfg",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "linkfilter_enabled",
                schema: "gf",
                table: "guild_cfg",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "linkfilter_disturbing",
                schema: "gf",
                table: "guild_cfg",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "linkfilter_invites",
                schema: "gf",
                table: "guild_cfg",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "linkfilter_booters",
                schema: "gf",
                table: "guild_cfg",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "leave_msg",
                schema: "gf",
                table: "guild_cfg",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "leave_cid",
                schema: "gf",
                table: "guild_cfg",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                schema: "gf",
                table: "guild_cfg",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "antispam_sensitivity",
                schema: "gf",
                table: "guild_cfg",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(short),
                oldDefaultValue: (short)5);

            migrationBuilder.AlterColumn<bool>(
                name: "antispam_enabled",
                schema: "gf",
                table: "guild_cfg",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<byte>(
                name: "antispam_action",
                schema: "gf",
                table: "guild_cfg",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(byte),
                oldDefaultValue: (byte)0);

            migrationBuilder.AlterColumn<short>(
                name: "antiflood_sensitivity",
                schema: "gf",
                table: "guild_cfg",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(short),
                oldDefaultValue: (short)5);

            migrationBuilder.AlterColumn<bool>(
                name: "antiflood_enabled",
                schema: "gf",
                table: "guild_cfg",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<short>(
                name: "antiflood_cooldown",
                schema: "gf",
                table: "guild_cfg",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(short),
                oldDefaultValue: (short)10);

            migrationBuilder.AlterColumn<byte>(
                name: "antiflood_action",
                schema: "gf",
                table: "guild_cfg",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(byte),
                oldDefaultValue: (byte)4);

            migrationBuilder.AlterColumn<bool>(
                name: "antiinstantleave_enabled",
                schema: "gf",
                table: "guild_cfg",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<short>(
                name: "antiinstantleave_cooldown",
                schema: "gf",
                table: "guild_cfg",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(short),
                oldDefaultValue: (short)3);

            migrationBuilder.AlterColumn<string>(
                name: "reason",
                schema: "gf",
                table: "blocked_users",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "reason",
                schema: "gf",
                table: "blocked_channels",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 64,
                oldNullable: true);
        }
    }
}
