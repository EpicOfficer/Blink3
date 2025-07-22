using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blink3.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class StoreShuffleBlinkMix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShuffledSolution",
                table: "BlinkMixes",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShuffledSolution",
                table: "BlinkMixes");
        }
    }
}
