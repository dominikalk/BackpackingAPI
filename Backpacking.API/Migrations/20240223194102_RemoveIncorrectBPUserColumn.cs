using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backpacking.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIncorrectBPUserColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_BPUserId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Friend_AspNetUsers_BPUserId",
                table: "Friend");

            migrationBuilder.DropIndex(
                name: "IX_Friend_BPUserId",
                table: "Friend");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_BPUserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BPUserId",
                table: "Friend");

            migrationBuilder.DropColumn(
                name: "BPUserId",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BPUserId",
                table: "Friend",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BPUserId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friend_BPUserId",
                table: "Friend",
                column: "BPUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BPUserId",
                table: "AspNetUsers",
                column: "BPUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_BPUserId",
                table: "AspNetUsers",
                column: "BPUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Friend_AspNetUsers_BPUserId",
                table: "Friend",
                column: "BPUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
