using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace makefb.Migrations
{
    /// <inheritdoc />
    public partial class TaoBangBinhLuan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_Binhluans_BaivietId",
                table: "Binhluans",
                column: "BaivietId");

            migrationBuilder.CreateIndex(
                name: "IX_Binhluans_UserId",
                table: "Binhluans",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Binhluans");
        }
    }
}
