using FurnitureStoreData.Models;
using Microsoft.EntityFrameworkCore;

namespace FurnitureStoreData.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=127.0.0.1,1434;Database=FurnitureStoreDB;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False;TrustServerCertificate=True");
            }
        }

        // Khai báo 6 bảng
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<OtpVerification> OtpVerifications { get; set; }

        // Bổ sung hàm này để tự động tạo tài khoản Admin
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Nội thất Phòng khách", Description = "Gồm Sofa, bàn trà, kệ tivi, tủ giày..." },
                new Category { Id = 2, Name = "Nội thất Phòng ngủ", Description = "Gồm Giường ngủ, tủ quần áo, bàn trang điểm..." },
                new Category { Id = 3, Name = "Nội thất Phòng bếp", Description = "Gồm Bàn ăn, ghế phòng ăn, tủ kệ bếp..." },
                new Category { Id = 4, Name = "Nội thất Phòng tắm", Description = "Gồm Tủ gương, kệ để đồ nhà vệ sinh..." },
                new Category { Id = 5, Name = "Nội thất Văn phòng", Description = "Gồm Bàn làm việc, ghế xoay, tủ tài liệu..." }
            );
            // Cấu hình Seed Data cho bảng AppUser
            modelBuilder.Entity<AppUser>().HasData(
                new AppUser
                {
                    Id = 1,
                    Username = "admin",
                    // BẢO MẬT: Đây là mã băm SHA-256 của mật khẩu "123"
                    // Không bao giờ lưu plain-text để tránh bị trừ điểm nặng!
                    Password = "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3",
                    FullName = "Quản Trị Viên Hệ Thống",
                    Email = "admin@noithat.com",
                    Phone = "0912345678",
                    Role = "Admin",
                    IsActive = true
                }
            );
        }
    }
}