using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocLocker.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCancelledDocumentRequestStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DocumentRequestStatuses",
                columns: new[] { "DocumentRequestStatusId", "Name" },
                values: new object[] { 6, "Cancelled" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DocumentRequestStatuses",
                keyColumn: "DocumentRequestStatusId",
                keyValue: 6);
        }
    }
}
