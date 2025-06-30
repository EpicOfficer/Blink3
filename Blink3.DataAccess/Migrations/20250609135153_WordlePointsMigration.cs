using Blink3.Core.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blink3.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class WordlePointsMigration : Migration
    {
        private const int WordleGameTypeValue = (int)GameType.BlinkWord;
        
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Points",
                table: "GameStatistics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Migrate existing Points from BlinkUsers to GameStatistics (for GameType.Wordle)
            migrationBuilder.Sql($@"
                -- Define Wordle game type constant
                DO $$ 
                BEGIN
                    -- Add Points to existing Wordle records
                    UPDATE ""GameStatistics""
                    SET ""Points"" = ""Points"" + COALESCE(
                        (SELECT ""Points"" FROM ""BlinkUsers"" 
                         WHERE ""GameStatistics"".""BlinkUserId"" = ""BlinkUsers"".""Id""), 0
                    )
                    WHERE ""Type"" = {WordleGameTypeValue};

                    -- Insert new GameStatistics for users without Wordle records
                    INSERT INTO ""GameStatistics"" (""BlinkUserId"", ""Points"", ""Type"", ""GamesPlayed"", ""GamesWon"", ""CurrentStreak"", ""MaxStreak"", ""LastActivity"")
                    SELECT ""Id"", ""Points"", {WordleGameTypeValue}, 0, 0, 0, 0, NULL
                    FROM ""BlinkUsers""
                    WHERE ""Points"" > 0 AND ""Id"" NOT IN (
                        SELECT ""BlinkUserId"" FROM ""GameStatistics"" WHERE ""Type"" = {WordleGameTypeValue}
                    );
                END $$;
            ");
            
            migrationBuilder.DropColumn(
                name: "Points",
                table: "BlinkUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Points",
                table: "GameStatistics");

            migrationBuilder.AddColumn<int>(
                name: "Points",
                table: "BlinkUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            
            // Restore Points back to BlinkUsers from GameStatistics
            migrationBuilder.Sql($@"
                UPDATE ""BlinkUsers""
                SET ""Points"" = COALESCE(
                    (SELECT ""Points"" FROM ""GameStatistics"" 
                     WHERE ""GameStatistics"".""BlinkUserId"" = ""BlinkUsers"".""Id"" AND ""GameStatistics"".""Type"" = {WordleGameTypeValue}), 0
                );
            ");
        }
    }
}
