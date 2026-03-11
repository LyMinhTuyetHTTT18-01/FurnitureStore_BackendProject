using FurnitureStoreData.Context;
using Microsoft.EntityFrameworkCore;

namespace FurnitureStoreWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Đăng ký DbContext (Bạn đã làm đúng)
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // 1. CẤU HÌNH SESSION (Dùng cho Giỏ hàng và Đăng nhập)
            builder.Services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian sống của Session
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            // 2. KÍCH HOẠT SESSION (Bắt buộc phải nằm giữa UseRouting và UseAuthorization)
            app.UseSession();

            app.UseAuthorization();

            app.MapStaticAssets();

            // 3. MAP ROUTE CHO AREA ADMIN (Phải đặt trước route default)
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}