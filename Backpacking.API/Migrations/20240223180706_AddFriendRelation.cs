using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backpacking.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFriendRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BPUserId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Friend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedById = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedToId = table.Column<Guid>(type: "uuid", nullable: false),
                    BecameFriendsTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RequestStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    BPUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friend", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Friend_AspNetUsers_BPUserId",
                        column: x => x.BPUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Friend_AspNetUsers_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Friend_AspNetUsers_RequestedToId",
                        column: x => x.RequestedToId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BPUserId",
                table: "AspNetUsers",
                column: "BPUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Friend_BPUserId",
                table: "Friend",
                column: "BPUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Friend_RequestedById",
                table: "Friend",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_Friend_RequestedToId",
                table: "Friend",
                column: "RequestedToId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_BPUserId",
                table: "AspNetUsers",
                column: "BPUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_BPUserId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Friend");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_BPUserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BPUserId",
                table: "AspNetUsers");
        }
    }
}
