using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocLocker.API.Migrations
{
    /// <inheritdoc />
    public partial class AddChangeToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This removes any legacy IsSuperAdmin column from Projects if it exists.
            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsSuperAdmin' AND Object_ID = OBJECT_ID(N'[Projects]'))
BEGIN
    ALTER TABLE [Projects] DROP COLUMN [IsSuperAdmin]
END");

            // This adds the IsSuperAdmin column when it is missing.
            migrationBuilder.Sql(@"IF COL_LENGTH(N'Users', N'IsSuperAdmin') IS NULL
BEGIN
    ALTER TABLE [Users] ADD [IsSuperAdmin] bit NOT NULL DEFAULT CAST(0 AS bit)
END");

            // This marks existing admin users with management access as super admins.
            migrationBuilder.Sql("UPDATE Users SET IsSuperAdmin = 1 WHERE RoleId = 1 AND AllowUserManagement = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This removes the IsSuperAdmin column if it exists.
            migrationBuilder.Sql(@"IF COL_LENGTH(N'Users', N'IsSuperAdmin') IS NOT NULL
BEGIN
    ALTER TABLE [Users] DROP COLUMN [IsSuperAdmin]
END");

            migrationBuilder.AddColumn<bool>(
                name: "IsSuperAdmin",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
