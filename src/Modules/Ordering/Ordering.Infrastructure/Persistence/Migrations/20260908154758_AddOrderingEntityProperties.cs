using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordering.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderingEntityProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ChangedAt",
                schema: "ordering",
                table: "OrderStatusHistories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "ordering",
                table: "OrderStatusHistories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                schema: "ordering",
                table: "OrderStatusHistories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "ordering",
                table: "OrderStatusHistories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                schema: "ordering",
                table: "Orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                schema: "ordering",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "ordering",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                schema: "ordering",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeliveryAddressId",
                schema: "ordering",
                table: "Orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryFee",
                schema: "ordering",
                table: "Orders",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                schema: "ordering",
                table: "Orders",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                schema: "ordering",
                table: "Orders",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PlacedAt",
                schema: "ordering",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "ordering",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Placed");

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                schema: "ordering",
                table: "Orders",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                schema: "ordering",
                table: "Orders",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "ordering",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "ordering",
                table: "Orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "LineTotal",
                schema: "ordering",
                table: "OrderItems",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                schema: "ordering",
                table: "OrderItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                schema: "ordering",
                table: "OrderItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ProductNameSnapshot",
                schema: "ordering",
                table: "OrderItems",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductSkuSnapshot",
                schema: "ordering",
                table: "OrderItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                schema: "ordering",
                table: "OrderItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                schema: "ordering",
                table: "OrderItems",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_OrderId",
                schema: "ordering",
                table: "OrderStatusHistories",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                schema: "ordering",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                schema: "ordering",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                schema: "ordering",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                schema: "ordering",
                table: "OrderItems",
                column: "OrderId",
                principalSchema: "ordering",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderStatusHistories_Orders_OrderId",
                schema: "ordering",
                table: "OrderStatusHistories",
                column: "OrderId",
                principalSchema: "ordering",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                schema: "ordering",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderStatusHistories_Orders_OrderId",
                schema: "ordering",
                table: "OrderStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_OrderStatusHistories_OrderId",
                schema: "ordering",
                table: "OrderStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderNumber",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OrderId",
                schema: "ordering",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ChangedAt",
                schema: "ordering",
                table: "OrderStatusHistories");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "ordering",
                table: "OrderStatusHistories");

            migrationBuilder.DropColumn(
                name: "OrderId",
                schema: "ordering",
                table: "OrderStatusHistories");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "ordering",
                table: "OrderStatusHistories");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddressId",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryFee",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlacedAt",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LineTotal",
                schema: "ordering",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "OrderId",
                schema: "ordering",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductId",
                schema: "ordering",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductNameSnapshot",
                schema: "ordering",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductSkuSnapshot",
                schema: "ordering",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "Quantity",
                schema: "ordering",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                schema: "ordering",
                table: "OrderItems");
        }
    }
}
