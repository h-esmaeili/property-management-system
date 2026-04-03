using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WebhookSubscriptionsOwnedByUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_webhook_subscriptions_tenants_TenantId",
                table: "webhook_subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_webhook_subscriptions_TenantId_EventType_Url",
                table: "webhook_subscriptions");

            // Tenant IDs are not valid user IDs; clear rows before re-pointing FK to users.
            migrationBuilder.Sql("DELETE FROM webhook_subscriptions;");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "webhook_subscriptions",
                newName: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_webhook_subscriptions_UserId_EventType_Url",
                table: "webhook_subscriptions",
                columns: new[] { "UserId", "EventType", "Url" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_webhook_subscriptions_users_UserId",
                table: "webhook_subscriptions",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_webhook_subscriptions_users_UserId",
                table: "webhook_subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_webhook_subscriptions_UserId_EventType_Url",
                table: "webhook_subscriptions");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "webhook_subscriptions",
                newName: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_webhook_subscriptions_TenantId_EventType_Url",
                table: "webhook_subscriptions",
                columns: new[] { "TenantId", "EventType", "Url" });

            migrationBuilder.AddForeignKey(
                name: "FK_webhook_subscriptions_tenants_TenantId",
                table: "webhook_subscriptions",
                column: "TenantId",
                principalTable: "tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
