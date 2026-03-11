using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace makefb.Migrations
{
    /// <inheritdoc />
    public partial class ThemLuotThich : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Luotthich",
                table: "Baiviets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Luotthich",
                table: "Baiviets");
        }
    }
}
