using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blink3.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class TempVcBanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal[]>(
                name: "BannedUsers",
                table: "TempVcs",
                type: "numeric(20,0)[]",
                nullable: false,
                defaultValue: new decimal[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannedUsers",
                table: "TempVcs");
        }
    }
}
