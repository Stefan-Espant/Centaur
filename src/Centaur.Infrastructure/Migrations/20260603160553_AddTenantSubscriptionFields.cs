using System;
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

            migrationBuilder.CreateTable(
                name: "block_types",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Fields = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_block_types", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_block_types_Slug",
                schema: "public",
                table: "block_types",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "block_types",
                schema: "public");

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
