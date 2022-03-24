using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlinePager.Migrations
{
    public partial class migr5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ToOfHour",
                table: "tblSchedules",
                newName: "ToHour");

            migrationBuilder.AlterColumn<string>(
                name: "ToDate",
                table: "tblSchedules",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 10);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ToHour",
                table: "tblSchedules",
                newName: "ToOfHour");

            migrationBuilder.AlterColumn<string>(
                name: "ToDate",
                table: "tblSchedules",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
