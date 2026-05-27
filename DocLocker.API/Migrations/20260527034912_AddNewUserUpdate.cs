using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocLocker.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNewUserUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowUserManagement",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE Users SET AllowUserManagement = 1 WHERE RoleId = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowUserManagement",
                table: "Users");
        }
    }
}
