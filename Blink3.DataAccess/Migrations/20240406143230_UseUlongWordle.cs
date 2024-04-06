using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Blink3.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UseUlongWordle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WordleGuesses_Wordles_WordleId",
                table: "WordleGuesses");

            migrationBuilder.DropIndex(
                name: "IX_WordleGuesses_WordleId",
                table: "WordleGuesses");

            migrationBuilder.AlterColumn<decimal>(
                name: "Id",
                table: "Wordles",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<decimal>(
                name: "WordleId1",
                table: "WordleGuesses",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_WordleGuesses_WordleId1",
                table: "WordleGuesses",
                column: "WordleId1");

            migrationBuilder.AddForeignKey(
                name: "FK_WordleGuesses_Wordles_WordleId1",
                table: "WordleGuesses",
                column: "WordleId1",
                principalTable: "Wordles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WordleGuesses_Wordles_WordleId1",
                table: "WordleGuesses");

            migrationBuilder.DropIndex(
                name: "IX_WordleGuesses_WordleId1",
                table: "WordleGuesses");

            migrationBuilder.DropColumn(
                name: "WordleId1",
                table: "WordleGuesses");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Wordles",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "IX_WordleGuesses_WordleId",
                table: "WordleGuesses",
                column: "WordleId");

            migrationBuilder.AddForeignKey(
                name: "FK_WordleGuesses_Wordles_WordleId",
                table: "WordleGuesses",
                column: "WordleId",
                principalTable: "Wordles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
