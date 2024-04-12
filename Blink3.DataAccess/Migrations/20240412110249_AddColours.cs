using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blink3.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddColours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackgroundColour",
                table: "BlinkGuilds",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CorrectTileColour",
                table: "BlinkGuilds",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IncorrectTileColour",
                table: "BlinkGuilds",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MisplacedTileColour",
                table: "BlinkGuilds",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TextColour",
                table: "BlinkGuilds",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundColour",
                table: "BlinkGuilds");

            migrationBuilder.DropColumn(
                name: "CorrectTileColour",
                table: "BlinkGuilds");

            migrationBuilder.DropColumn(
                name: "IncorrectTileColour",
                table: "BlinkGuilds");

            migrationBuilder.DropColumn(
                name: "MisplacedTileColour",
                table: "BlinkGuilds");

            migrationBuilder.DropColumn(
                name: "TextColour",
                table: "BlinkGuilds");
        }
    }
}
