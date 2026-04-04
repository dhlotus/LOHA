using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LOHA.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ngaysinh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gioitinh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailorSDT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Matkhau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngaytao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Baiviets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Noidung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Anh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngaydang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Baiviets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Baiviets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KetBans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiGuiId = table.Column<int>(type: "int", nullable: false),
                    NguoiNhanId = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayPhanHoi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KetBans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KetBans_Users_NguoiGuiId",
                        column: x => x.NguoiGuiId,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KetBans_Users_NguoiNhanId",
                        column: x => x.NguoiNhanId,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "TinNhan",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiGuiID = table.Column<int>(type: "int", nullable: false),
                    NguoiNhanID = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaXem = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinNhan", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TinNhan_Users_NguoiGuiID",
                        column: x => x.NguoiGuiID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TinNhan_Users_NguoiNhanID",
                        column: x => x.NguoiNhanID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Binhluans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Noidung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngaydang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BaivietId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Binhluans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Binhluans_Baiviets_BaivietId",
                        column: x => x.BaivietId,
                        principalTable: "Baiviets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Binhluans_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Thichs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BaivietId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Thichs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Thichs_Baiviets_BaivietId",
                        column: x => x.BaivietId,
                        principalTable: "Baiviets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Thichs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Baiviets_UserId",
                table: "Baiviets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Binhluans_BaivietId",
                table: "Binhluans",
                column: "BaivietId");

            migrationBuilder.CreateIndex(
                name: "IX_Binhluans_UserId",
                table: "Binhluans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_KetBans_NguoiGuiId",
                table: "KetBans",
                column: "NguoiGuiId");

            migrationBuilder.CreateIndex(
                name: "IX_KetBans_NguoiNhanId",
                table: "KetBans",
                column: "NguoiNhanId");

            migrationBuilder.CreateIndex(
                name: "IX_Thichs_BaivietId",
                table: "Thichs",
                column: "BaivietId");

            migrationBuilder.CreateIndex(
                name: "IX_Thichs_UserId",
                table: "Thichs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhan_NguoiGuiID",
                table: "TinNhan",
                column: "NguoiGuiID");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhan_NguoiNhanID",
                table: "TinNhan",
                column: "NguoiNhanID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Binhluans");

            migrationBuilder.DropTable(
                name: "KetBans");

            migrationBuilder.DropTable(
                name: "Thichs");

            migrationBuilder.DropTable(
                name: "TinNhan");

            migrationBuilder.DropTable(
                name: "Baiviets");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
