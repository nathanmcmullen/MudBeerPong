using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MudBeerPong.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PositionColumn",
                table: "Shots");

            migrationBuilder.DropColumn(
                name: "PositionRow",
                table: "Shots");

            migrationBuilder.AddColumn<string>(
                name: "CupPosition",
                table: "Shots",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CupPosition",
                table: "Shots");

            migrationBuilder.AddColumn<int>(
                name: "PositionColumn",
                table: "Shots",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionRow",
                table: "Shots",
                type: "nvarchar(1)",
                nullable: true);
        }
    }
}
