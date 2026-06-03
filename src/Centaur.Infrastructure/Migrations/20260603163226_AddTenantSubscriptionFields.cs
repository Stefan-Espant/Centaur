using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Centaur.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantSubscriptionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StripeCustomerId",
                schema: "public",
                table: "tenants",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeSubscriptionId",
                schema: "public",
                table: "tenants",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionStatus",
                schema: "public",
                table: "tenants",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "free");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_StripeCustomerId",
                schema: "public",
                table: "tenants",
                column: "StripeCustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tenants_StripeCustomerId",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "StripeCustomerId",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "StripeSubscriptionId",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "SubscriptionStatus",
                schema: "public",
                table: "tenants");
        }
    }
}
