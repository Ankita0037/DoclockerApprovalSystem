using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocLocker.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdateToDocumentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Documents",
                newName: "FileName");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Documents",
                newName: "DocumentId");

            migrationBuilder.AddColumn<int>(
                name: "AssignedManagerId",
                table: "Documents",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedManagerId",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Documents",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "DocumentId",
                table: "Documents",
                newName: "Id");
        }
    }
}
