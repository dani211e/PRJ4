using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTG_Emulator.Backend.Migrations
{
    /// <inheritdoc />
    public partial class LinkPlayersToGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_Games_GameId",
                table: "Players");

            migrationBuilder.RenameColumn(
                name: "GameId",
                table: "Players",
                newName: "CurrentGameId");

            migrationBuilder.RenameIndex(
                name: "IX_Players_GameId",
                table: "Players",
                newName: "IX_Players_CurrentGameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Games_CurrentGameId",
                table: "Players",
                column: "CurrentGameId",
                principalTable: "Games",
                principalColumn: "GameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_Games_CurrentGameId",
                table: "Players");

            migrationBuilder.RenameColumn(
                name: "CurrentGameId",
                table: "Players",
                newName: "GameId");

            migrationBuilder.RenameIndex(
                name: "IX_Players_CurrentGameId",
                table: "Players",
                newName: "IX_Players_GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Games_GameId",
                table: "Players",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "GameId");
        }
    }
}
