using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LOHA.Migrations
{
    /// <inheritdoc />
    public partial class ThmNhomKetBan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NhomNguoiGui",
                table: "KetBans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NhomNguoiNhan",
                table: "KetBans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NhomNguoiGui",
                table: "KetBans");

            migrationBuilder.DropColumn(
                name: "NhomNguoiNhan",
                table: "KetBans");
        }
    }
}
