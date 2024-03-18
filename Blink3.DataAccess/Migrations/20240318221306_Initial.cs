using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blink3.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlinkGuilds",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlinkGuilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserTodos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 25, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Complete = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTodos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlinkGuilds");

            migrationBuilder.DropTable(
                name: "UserTodos");
        }
    }
}
