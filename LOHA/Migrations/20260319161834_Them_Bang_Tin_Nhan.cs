//BÙI ĐỨC HÀ - LOTUS
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LOHA.Migrations
{
    /// <inheritdoc />
    public partial class Them_Bang_Tin_Nhan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "TinNhan");
        }
    }
}
