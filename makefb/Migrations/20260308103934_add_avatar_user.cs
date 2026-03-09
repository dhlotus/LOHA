using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace makefb.Migrations
{
    /// <inheritdoc />
    public partial class add_avatar_user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailorSDT",
                table: "Baiviets");

            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Baiviets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Baiviets_UserId",
                table: "Baiviets",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Baiviets_Users_UserId",
                table: "Baiviets",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Baiviets_Users_UserId",
                table: "Baiviets");

            migrationBuilder.DropIndex(
                name: "IX_Baiviets_UserId",
                table: "Baiviets");

            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Baiviets");

            migrationBuilder.AddColumn<string>(
                name: "EmailorSDT",
                table: "Baiviets",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
