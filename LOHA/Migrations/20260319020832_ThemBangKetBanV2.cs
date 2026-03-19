// Bùi Đức Hà - LOTUS
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LOHA.Migrations
{
    /// <inheritdoc />
    public partial class ThemBangKetBanV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) //áp dụng thay đổi 
        {
            migrationBuilder.CreateTable(
                name: "KetBans", //tên bảng
                columns: table => new //định nghĩa các cột
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"), // khoá chính tự động tăng
                    NguoiGuiId = table.Column<int>(type: "int", nullable: false), // ID người gửi lời mời kết bạn
                    NguoiNhanId = table.Column<int>(type: "int", nullable: false), // ID người nhận lời mời kết bạn
                    TrangThai = table.Column<int>(type: "int", nullable: false), // trạng thái kết bạn (0: Đang chờ, 1: Đã chấp nhận, 2: Đã từ chối, 3: Hủy kết bạn)
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: false), //
                    NgayPhanHoi = table.Column<DateTime>(type: "datetime2", nullable: true) // thgan phản hồi
                },
                constraints: table => // định nghĩa khoá chính và khoá ngoại
                {
                    table.PrimaryKey("PK_KetBans", x => x.Id); // định nghĩa khoá chính
                    table.ForeignKey(   // khoá ngoại 1
                        name: "FK_KetBans_Users_NguoiGuiId", // tên ràng buộc
                        column: x => x.NguoiGuiId, // cột khoá ngoại
                        principalTable: "Users", //bảng tham chiều
                        principalColumn: "ID", //cột tham chiếu
                        onDelete: ReferentialAction.Cascade); // xoá user thì xoá luôn bản ghi kb
                    table.ForeignKey( // Khoá ngoại 2
                        name: "FK_KetBans_Users_NguoiNhanId",
                        column: x => x.NguoiNhanId,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                });
            // giúp tăng hiệu suất truy vấn khi lọc theo người gửi hoặc người nhận
            migrationBuilder.CreateIndex(
                name: "IX_KetBans_NguoiGuiId",
                table: "KetBans",
                column: "NguoiGuiId");

            migrationBuilder.CreateIndex(
                name: "IX_KetBans_NguoiNhanId",
                table: "KetBans",
                column: "NguoiNhanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) // hoàn tác thay đổi
        {
            migrationBuilder.DropTable(
                name: "KetBans");
        }
    }
}
