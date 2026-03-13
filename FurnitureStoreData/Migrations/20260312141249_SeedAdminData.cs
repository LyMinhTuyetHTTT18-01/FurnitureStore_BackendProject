using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FurnitureStoreData.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AppUsers",
                columns: new[] { "Id", "Email", "FullName", "IsActive", "Password", "Phone", "Role", "Username" },
                values: new object[] { 1, "admin@noithat.com", "Quản Trị Viên Hệ Thống", true, "123", "0912345678", "Admin", "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
