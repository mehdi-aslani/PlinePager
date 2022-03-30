using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PlinePager.Migrations
{
    public partial class mig2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblAzans",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<string>(type: "text", nullable: false),
                    EnableA = table.Column<bool>(type: "boolean", nullable: false),
                    HourA = table.Column<int>(type: "integer", nullable: false),
                    MinuteA = table.Column<int>(type: "integer", nullable: false),
                    SecondA = table.Column<int>(type: "integer", nullable: false),
                    SoundsBeforeA = table.Column<string>(type: "text", nullable: true),
                    SoundsA = table.Column<string>(type: "text", nullable: true),
                    SoundsAfterA = table.Column<string>(type: "text", nullable: true),
                    AreasA = table.Column<string>(type: "text", nullable: true),
                    EnableB = table.Column<bool>(type: "boolean", nullable: false),
                    HourB = table.Column<int>(type: "integer", nullable: false),
                    MinuteB = table.Column<int>(type: "integer", nullable: false),
                    SecondB = table.Column<int>(type: "integer", nullable: false),
                    SoundsBeforeB = table.Column<string>(type: "text", nullable: true),
                    SoundsB = table.Column<string>(type: "text", nullable: true),
                    SoundsAfterB = table.Column<string>(type: "text", nullable: true),
                    AreasB = table.Column<string>(type: "text", nullable: true),
                    EnableC = table.Column<bool>(type: "boolean", nullable: false),
                    HourC = table.Column<int>(type: "integer", nullable: false),
                    MinuteC = table.Column<int>(type: "integer", nullable: false),
                    SecondC = table.Column<int>(type: "integer", nullable: false),
                    SoundsBeforeC = table.Column<string>(type: "text", nullable: true),
                    SoundsC = table.Column<string>(type: "text", nullable: true),
                    SoundsAfterC = table.Column<string>(type: "text", nullable: true),
                    AreasC = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblAzans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblAzans_Date",
                table: "tblAzans",
                column: "Date",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblAzans");
        }
    }
}
