using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Blink3.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class WordleInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wordles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WordToGuess = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wordles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WordleGuesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuessedById = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    WordleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordleGuesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WordleGuesses_Wordles_WordleId",
                        column: x => x.WordleId,
                        principalTable: "Wordles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WordleLetter",
                columns: table => new
                {
                    WordleGuessId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Letter = table.Column<char>(type: "character(1)", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordleLetter", x => new { x.WordleGuessId, x.Id });
                    table.ForeignKey(
                        name: "FK_WordleLetter_WordleGuesses_WordleGuessId",
                        column: x => x.WordleGuessId,
                        principalTable: "WordleGuesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WordleGuesses_WordleId",
                table: "WordleGuesses",
                column: "WordleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WordleLetter");

            migrationBuilder.DropTable(
                name: "WordleGuesses");

            migrationBuilder.DropTable(
                name: "Wordles");
        }
    }
}
