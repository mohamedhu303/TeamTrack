using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamTrack.Migrations
{
    /// <inheritdoc />
    public partial class RenameCreatedDateToCreateDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "createDate",
                table: "userTask",
                newName: "createdDate");

            migrationBuilder.RenameColumn(
                name: "createdDate",
                table: "project",
                newName: "createDate");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "AspNetUsers",
                newName: "createdDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "createdDate",
                table: "userTask",
                newName: "createDate");

            migrationBuilder.RenameColumn(
                name: "createDate",
                table: "project",
                newName: "createdDate");

            migrationBuilder.RenameColumn(
                name: "createdDate",
                table: "AspNetUsers",
                newName: "CreatedDate");
        }
    }
}
