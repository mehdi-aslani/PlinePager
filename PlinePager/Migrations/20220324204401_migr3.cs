using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlinePager.Migrations
{
    public partial class migr3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Schedules",
                table: "tblSchedules");

            migrationBuilder.AddColumn<int>(
                name: "IntervalDay",
                table: "tblSchedules",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IntervalEnable",
                table: "tblSchedules",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "IntervalHour",
                table: "tblSchedules",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IntervalMinute",
                table: "tblSchedules",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OfDate",
                table: "tblSchedules",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OfHour",
                table: "tblSchedules",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OfMinute",
                table: "tblSchedules",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ToDate",
                table: "tblSchedules",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ToDateEnable",
                table: "tblSchedules",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ToMinute",
                table: "tblSchedules",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ToOfHour",
                table: "tblSchedules",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntervalDay",
                table: "tblSchedules");

            migrationBuilder.DropColumn(
                name: "IntervalEnable",
                table: "tblSchedules");

            migrationBuilder.DropColumn(
                name: "IntervalHour",
                table: "tblSchedules");

            migrationBuilder.DropColumn(
                name: "IntervalMinute",
                table: "tblSchedules");

            migrationBuilder.DropColumn(
                name: "OfDate",
                table: "tblSchedules");

            migrationBuilder.DropColumn(
                name: "OfHour",
                table: "tblSchedules");

            migrationBuilder.DropColumn(
                name: "OfMinute",
                table: "tblSchedules");

            migrationBuilder.DropColumn(
                name: "ToDate",
                table: "tblSchedules");

            migrationBuilder.DropColumn(
                name: "ToDateEnable",
                table: "tblSchedules");

            migrationBuilder.DropColumn(
                name: "ToMinute",
                table: "tblSchedules");

            migrationBuilder.DropColumn(
                name: "ToOfHour",
                table: "tblSchedules");

            migrationBuilder.AddColumn<string>(
                name: "Schedules",
                table: "tblSchedules",
                type: "TEXT",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");
        }
    }
}
