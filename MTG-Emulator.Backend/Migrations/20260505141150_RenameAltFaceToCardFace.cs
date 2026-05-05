using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTG_Emulator.Backend.Migrations
{
    /// <inheritdoc />
    public partial class RenameAltFaceToCardFace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "URI",
                table: "RelatedCards",
                newName: "ImageUri");

            migrationBuilder.RenameColumn(
                name: "ImageURI",
                table: "AltFaces",
                newName: "ImageUri");

            migrationBuilder.RenameColumn(
                name: "AltFaceId",
                table: "AltFaces",
                newName: "CardFaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUri",
                table: "RelatedCards",
                newName: "URI");

            migrationBuilder.RenameColumn(
                name: "ImageUri",
                table: "AltFaces",
                newName: "ImageURI");

            migrationBuilder.RenameColumn(
                name: "CardFaceId",
                table: "AltFaces",
                newName: "AltFaceId");
        }
    }
}
