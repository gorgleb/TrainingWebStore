using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingWebStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class kol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SubtotalAmount",
                table: "orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "SubtotalAmount",
                table: "orders");
        }
    }
}
