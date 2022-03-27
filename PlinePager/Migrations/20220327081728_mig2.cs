using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlinePager.Migrations
{
    public partial class mig2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ended",
                table: "tblSchedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NextDate",
                table: "tblSchedules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NextHour",
                table: "tblSchedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NextMinute",
                table: "tblSchedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ended",
                table: "tblSchedules");

            migrationBuilder.DropColumn(
                name: "NextDate",
                table: "tblSchedules");

            migrationBuilder.DropColumn(
                name: "NextHour",
                table: "tblSchedules");

            migrationBuilder.DropColumn(
                name: "NextMinute",
                table: "tblSchedules");
        }
    }
}
