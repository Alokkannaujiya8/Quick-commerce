using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompletePaymentSchemaAndWebhookIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                schema: "payment",
                table: "Wallets",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "payment",
                table: "Wallets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "payment",
                table: "Wallets",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "payment",
                table: "Wallets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "payment",
                table: "Wallets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "payment",
                table: "Wallets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                schema: "payment",
                table: "Payments",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                schema: "payment",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "payment",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "payment",
                table: "Payments",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                schema: "payment",
                table: "Payments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                schema: "payment",
                table: "Payments",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Method",
                schema: "payment",
                table: "Payments",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                schema: "payment",
                table: "Payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ProviderName",
                schema: "payment",
                table: "Payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProviderOrderId",
                schema: "payment",
                table: "Payments",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProviderPaymentId",
                schema: "payment",
                table: "Payments",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundedAmount",
                schema: "payment",
                table: "Payments",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                schema: "payment",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "payment",
                table: "Payments",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "payment",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "payment",
                table: "Payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "PaymentAuditLogs",
                schema: "payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    PreviousStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NewStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ProviderReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentAuditLogs_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalSchema: "payment",
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentWebhookEvents",
                schema: "payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderEventId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EventType = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PayloadHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentWebhookEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId",
                schema: "payment",
                table: "Wallets",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_IdempotencyKey",
                schema: "payment",
                table: "Payments",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                schema: "payment",
                table: "Payments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ProviderOrderId",
                schema: "payment",
                table: "Payments",
                column: "ProviderOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAuditLogs_PaymentId",
                schema: "payment",
                table: "PaymentAuditLogs",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentWebhookEvents_ProviderEventId",
                schema: "payment",
                table: "PaymentWebhookEvents",
                column: "ProviderEventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentAuditLogs",
                schema: "payment");

            migrationBuilder.DropTable(
                name: "PaymentWebhookEvents",
                schema: "payment");

            migrationBuilder.DropIndex(
                name: "IX_Wallets_UserId",
                schema: "payment",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Payments_IdempotencyKey",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_OrderId",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ProviderOrderId",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Balance",
                schema: "payment",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "payment",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "payment",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "payment",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "payment",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "payment",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "Amount",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Method",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "OrderId",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ProviderName",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ProviderOrderId",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ProviderPaymentId",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RefundedAmount",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "payment",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "payment",
                table: "Payments");
        }
    }
}
