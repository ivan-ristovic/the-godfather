using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TheGodfather.Migrations
{
    public partial class Reactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "filters",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gid = table.Column<long>(nullable: false),
                    trigger = table.Column<string>(maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_filters", x => x.id);
                    table.ForeignKey(
                        name: "FK_filters_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reactions_emoji",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gid = table.Column<long>(nullable: false),
                    reaction = table.Column<string>(maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reactions_emoji", x => x.id);
                    table.ForeignKey(
                        name: "FK_reactions_emoji_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reactions_text",
                schema: "gf",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gid = table.Column<long>(nullable: false),
                    reaction = table.Column<string>(maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reactions_text", x => x.id);
                    table.ForeignKey(
                        name: "FK_reactions_text_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "gf",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reactions_emoji_triggers",
                schema: "gf",
                columns: table => new
                {
                    trigger = table.Column<string>(maxLength: 128, nullable: false),
                    id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reactions_emoji_triggers", x => new { x.id, x.trigger });
                    table.ForeignKey(
                        name: "FK_reactions_emoji_triggers_reactions_emoji_id",
                        column: x => x.id,
                        principalSchema: "gf",
                        principalTable: "reactions_emoji",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reactions_text_triggers",
                schema: "gf",
                columns: table => new
                {
                    trigger = table.Column<string>(maxLength: 128, nullable: false),
                    id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reactions_text_triggers", x => new { x.id, x.trigger });
                    table.ForeignKey(
                        name: "FK_reactions_text_triggers_reactions_text_id",
                        column: x => x.id,
                        principalSchema: "gf",
                        principalTable: "reactions_text",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_filters_gid",
                schema: "gf",
                table: "filters",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_reactions_emoji_gid",
                schema: "gf",
                table: "reactions_emoji",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_reactions_text_gid",
                schema: "gf",
                table: "reactions_text",
                column: "gid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "filters",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "reactions_emoji_triggers",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "reactions_text_triggers",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "reactions_emoji",
                schema: "gf");

            migrationBuilder.DropTable(
                name: "reactions_text",
                schema: "gf");
        }
    }
}
