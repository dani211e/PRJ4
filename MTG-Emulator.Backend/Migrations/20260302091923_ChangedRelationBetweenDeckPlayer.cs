using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTG_Emulator.Backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangedRelationBetweenDeckPlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_Decks_DeckId",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Players_DeckId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "DeckId",
                table: "Players");

            migrationBuilder.AddColumn<int>(
                name: "PlayerId",
                table: "Decks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Decks_PlayerId",
                table: "Decks",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Decks_Players_PlayerId",
                table: "Decks",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "PlayerId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Decks_Players_PlayerId",
                table: "Decks");

            migrationBuilder.DropIndex(
                name: "IX_Decks_PlayerId",
                table: "Decks");

            migrationBuilder.DropColumn(
                name: "PlayerId",
                table: "Decks");

            migrationBuilder.AddColumn<int>(
                name: "DeckId",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Players_DeckId",
                table: "Players",
                column: "DeckId");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Decks_DeckId",
                table: "Players",
                column: "DeckId",
                principalTable: "Decks",
                principalColumn: "DeckId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
