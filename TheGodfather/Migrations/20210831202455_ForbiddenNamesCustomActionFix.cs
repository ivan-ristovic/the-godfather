using Microsoft.EntityFrameworkCore.Migrations;

namespace TheGodfather.Migrations
{
    public partial class ForbiddenNamesCustomActionFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "action",
                schema: "gf",
                table: "forbidden_names",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "smallint",
                oldMaxLength: 128);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "action",
                schema: "gf",
                table: "forbidden_names",
                type: "smallint",
                maxLength: 128,
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "smallint",
                oldNullable: true);
        }
    }
}
