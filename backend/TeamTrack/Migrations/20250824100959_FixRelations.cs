using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamTrack.Migrations
{
    /// <inheritdoc />
    public partial class FixRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userTask_AspNetUsers_AssignedUserId",
                table: "userTask");

            migrationBuilder.AddForeignKey(
                name: "FK_userTask_AspNetUsers_AssignedUserId",
                table: "userTask",
                column: "AssignedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userTask_AspNetUsers_AssignedUserId",
                table: "userTask");

            migrationBuilder.AddForeignKey(
                name: "FK_userTask_AspNetUsers_AssignedUserId",
                table: "userTask",
                column: "AssignedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
