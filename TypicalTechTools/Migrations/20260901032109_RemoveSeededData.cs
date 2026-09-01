using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TypicalTechTools.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeededData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "ReviewId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "ProductDescription", "ProductName", "ProductPrice", "UpdatedDate" },
                values: new object[] { 1, "Example Product Description", "Example Product", 50.5m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Reviews",
                columns: new[] { "ReviewId", "CreatedDate", "ProductId", "ReviewText" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Review Text" });
        }
    }
}
