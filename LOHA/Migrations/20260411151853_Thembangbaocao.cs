using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LOHA.Migrations
{
    /// <inheritdoc />
    public partial class Thembangbaocao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BaoCaoBaiViet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiBaoCaoId = table.Column<int>(type: "int", nullable: false),
                    BaiVietId = table.Column<int>(type: "int", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCaoBaiViet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaoCaoBaiViet_Baiviets_BaiVietId",
                        column: x => x.BaiVietId,
                        principalTable: "Baiviets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaoCaoBaiViet_Users_NguoiBaoCaoId",
                        column: x => x.NguoiBaoCaoId,
                        principalTable: "Users",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "BaoCaoNguoiDung",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiBaoCaoId = table.Column<int>(type: "int", nullable: false),
                    NguoiBiBaoCaoId = table.Column<int>(type: "int", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCaoNguoiDung", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaoCaoNguoiDung_Users_NguoiBaoCaoId",
                        column: x => x.NguoiBaoCaoId,
                        principalTable: "Users",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_BaoCaoNguoiDung_Users_NguoiBiBaoCaoId",
                        column: x => x.NguoiBiBaoCaoId,
                        principalTable: "Users",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoBaiViet_BaiVietId",
                table: "BaoCaoBaiViet",
                column: "BaiVietId");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoBaiViet_NguoiBaoCaoId",
                table: "BaoCaoBaiViet",
                column: "NguoiBaoCaoId");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoNguoiDung_NguoiBaoCaoId",
                table: "BaoCaoNguoiDung",
                column: "NguoiBaoCaoId");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoNguoiDung_NguoiBiBaoCaoId",
                table: "BaoCaoNguoiDung",
                column: "NguoiBiBaoCaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaoCaoBaiViet");

            migrationBuilder.DropTable(
                name: "BaoCaoNguoiDung");
        }
    }
}
