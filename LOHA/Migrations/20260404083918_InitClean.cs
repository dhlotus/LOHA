using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LOHA.Migrations
{
    /// <inheritdoc />
    public partial class InitClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayCapNhatNgaySinh",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayCapNhatTen",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayCapNhatNgaySinh",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NgayCapNhatTen",
                table: "Users");
        }
    }
}
