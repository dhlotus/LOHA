using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LOHA.Migrations
{
    /// <inheritdoc />
    public partial class ThemBangXacThucEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "XacThucEmail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaOTP = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Matkhau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ngaysinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gioitinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGianTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianHetHan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaSuDung = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XacThucEmail", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "XacThucEmail");
        }
    }
}
