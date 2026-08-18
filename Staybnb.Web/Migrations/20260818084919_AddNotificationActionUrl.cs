using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Staybnb.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationActionUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActionUrl",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredDocumentsJson",
                table: "CheckInProcesses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionUrl",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RequiredDocumentsJson",
                table: "CheckInProcesses");
        }
    }
}
