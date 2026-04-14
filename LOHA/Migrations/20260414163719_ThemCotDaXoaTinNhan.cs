using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LOHA.Migrations
{
    /// <inheritdoc />
    public partial class ThemCotDaXoaTinNhan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DaXoaBoiNguoiGui",
                table: "TinNhan",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaXoaBoiNguoiNhan",
                table: "TinNhan",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaXoaBoiNguoiGui",
                table: "TinNhan");

            migrationBuilder.DropColumn(
                name: "DaXoaBoiNguoiNhan",
                table: "TinNhan");
        }
    }
}
