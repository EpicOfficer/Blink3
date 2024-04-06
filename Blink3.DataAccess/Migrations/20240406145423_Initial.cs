using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlinkGuilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserTodos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Label = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    Description = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Complete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTodos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Wordles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    WordToGuess = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wordles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Words",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Text = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    IsSolution = table.Column<bool>(type: "boolean", nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Length = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Words", x => x.Id);
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

            migrationBuilder.CreateIndex(
                name: "IX_Words_Language_IsSolution_Length",
                table: "Words",
                columns: new[] { "Language", "IsSolution", "Length" });

            migrationBuilder.CreateIndex(
                name: "IX_Words_Text_Language",
                table: "Words",
                columns: new[] { "Text", "Language" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlinkGuilds");

            migrationBuilder.DropTable(
                name: "UserTodos");

            migrationBuilder.DropTable(
                name: "WordleLetter");

            migrationBuilder.DropTable(
                name: "Words");

            migrationBuilder.DropTable(
                name: "WordleGuesses");

            migrationBuilder.DropTable(
                name: "Wordles");
        }
    }
}
