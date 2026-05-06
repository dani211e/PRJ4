using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTG_Emulator.Backend.Migrations
{
    /// <inheritdoc />
    public partial class RenameCardFaceName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CardFaceName",
                table: "AltFaces",
                newName: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AltFaces",
                newName: "CardFaceName");
        }
    }
}
