using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FurnitureStoreData.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategoriesData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Gồm Sofa, bàn trà, kệ tivi, tủ giày...", "Nội thất Phòng khách" },
                    { 2, "Gồm Giường ngủ, tủ quần áo, bàn trang điểm...", "Nội thất Phòng ngủ" },
                    { 3, "Gồm Bàn ăn, ghế phòng ăn, tủ kệ bếp...", "Nội thất Phòng bếp" },
                    { 4, "Gồm Tủ gương, kệ để đồ nhà vệ sinh...", "Nội thất Phòng tắm" },
                    { 5, "Gồm Bàn làm việc, ghế xoay, tủ tài liệu...", "Nội thất Văn phòng" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
