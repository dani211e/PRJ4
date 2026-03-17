using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTG_Emulator.Backend.Migrations
{
    /// <inheritdoc />
    public partial class RenameOraceTextToOracleText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Component",
                table: "RelatedCards");

            migrationBuilder.DropColumn(
                name: "TypeLine",
                table: "RelatedCards");

            migrationBuilder.RenameColumn(
                name: "OraceText",
                table: "Cards",
                newName: "OracleText");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OracleText",
                table: "Cards",
                newName: "OraceText");

            migrationBuilder.AddColumn<string>(
                name: "Component",
                table: "RelatedCards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TypeLine",
                table: "RelatedCards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
