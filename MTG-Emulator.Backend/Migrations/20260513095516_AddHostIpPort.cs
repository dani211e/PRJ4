using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTG_Emulator.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddHostIpPort : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HostIp",
                table: "Games",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "HostPort",
                table: "Games",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HostIp",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "HostPort",
                table: "Games");
        }
    }
}
