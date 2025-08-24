using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTaskTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_task_AspNetUsers_AssignedUserId",
                table: "task");

            migrationBuilder.DropForeignKey(
                name: "FK_task_project_projectId",
                table: "task");

            migrationBuilder.DropPrimaryKey(
                name: "PK_task",
                table: "task");

            migrationBuilder.RenameTable(
                name: "task",
                newName: "userTask");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "AspNetUsers",
                newName: "userRole");

            migrationBuilder.RenameIndex(
                name: "IX_task_projectId",
                table: "userTask",
                newName: "IX_userTask_projectId");

            migrationBuilder.RenameIndex(
                name: "IX_task_AssignedUserId",
                table: "userTask",
                newName: "IX_userTask_AssignedUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_userTask",
                table: "userTask",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_userTask_AspNetUsers_AssignedUserId",
                table: "userTask",
                column: "AssignedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_userTask_project_projectId",
                table: "userTask",
                column: "projectId",
                principalTable: "project",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userTask_AspNetUsers_AssignedUserId",
                table: "userTask");

            migrationBuilder.DropForeignKey(
                name: "FK_userTask_project_projectId",
                table: "userTask");

            migrationBuilder.DropPrimaryKey(
                name: "PK_userTask",
                table: "userTask");

            migrationBuilder.RenameTable(
                name: "userTask",
                newName: "task");

            migrationBuilder.RenameColumn(
                name: "userRole",
                table: "AspNetUsers",
                newName: "Role");

            migrationBuilder.RenameIndex(
                name: "IX_userTask_projectId",
                table: "task",
                newName: "IX_task_projectId");

            migrationBuilder.RenameIndex(
                name: "IX_userTask_AssignedUserId",
                table: "task",
                newName: "IX_task_AssignedUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_task",
                table: "task",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_task_AspNetUsers_AssignedUserId",
                table: "task",
                column: "AssignedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_task_project_projectId",
                table: "task",
                column: "projectId",
                principalTable: "project",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
