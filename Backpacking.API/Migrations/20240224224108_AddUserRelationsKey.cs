using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backpacking.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRelationsKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRelations",
                table: "UserRelations");

            migrationBuilder.DropIndex(
                name: "IX_UserRelations_SentById",
                table: "UserRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRelations",
                table: "UserRelations",
                columns: new[] { "SentById", "SentToId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRelations",
                table: "UserRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRelations",
                table: "UserRelations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserRelations_SentById",
                table: "UserRelations",
                column: "SentById");
        }
    }
}
