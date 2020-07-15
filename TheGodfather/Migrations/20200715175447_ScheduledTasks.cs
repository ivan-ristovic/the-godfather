using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TheGodfather.Migrations
{
    public partial class ScheduledTasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reminders",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uid = table.Column<long>(nullable: false),
                    execution_time = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    cid = table.Column<long>(nullable: true),
                    message = table.Column<string>(maxLength: 256, nullable: false),
                    is_repeating = table.Column<bool>(nullable: false, defaultValue: false),
                    repeat_interval = table.Column<TimeSpan>(type: "interval", nullable: true, defaultValue: new TimeSpan(0, 0, 0, 0, -1))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scheduled_tasks",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uid = table.Column<long>(nullable: false),
                    execution_time = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    gid = table.Column<long>(nullable: false),
                    rid = table.Column<long>(nullable: true),
                    type = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduled_tasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_scheduled_tasks_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_tasks_gid",
                schema: "gf",
                table: "scheduled_tasks",
                column: "gid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reminders",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "scheduled_tasks",
                schema: "gf");
        }
    }
}
