using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Staybnb.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddHostApplicationToNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HostApplicationId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_HostApplicationId",
                table: "Notifications",
                column: "HostApplicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_HostApplications_HostApplicationId",
                table: "Notifications",
                column: "HostApplicationId",
                principalTable: "HostApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_HostApplications_HostApplicationId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_HostApplicationId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "HostApplicationId",
                table: "Notifications");
        }
    }
}
