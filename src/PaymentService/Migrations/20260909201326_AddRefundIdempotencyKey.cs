using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentService.Migrations
{
    /// <inheritdoc />
    public partial class AddRefundIdempotencyKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefundIdempotencyKey",
                table: "Payments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_RefundIdempotencyKey",
                table: "Payments",
                column: "RefundIdempotencyKey",
                unique: true,
                filter: "[RefundIdempotencyKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_RefundIdempotencyKey",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RefundIdempotencyKey",
                table: "Payments");
        }
    }
}
