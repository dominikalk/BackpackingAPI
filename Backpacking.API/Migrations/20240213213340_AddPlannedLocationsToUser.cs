using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backpacking.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPlannedLocationsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LocationDateAccuracy",
                table: "Locations",
                newName: "LocationType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LocationType",
                table: "Locations",
                newName: "LocationDateAccuracy");
        }
    }
}
