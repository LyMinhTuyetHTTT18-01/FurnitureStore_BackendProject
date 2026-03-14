using System; // Mới thêm để dùng Console
using FurnitureStoreData.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies; // Thư viện dùng cho Cookie Auth

namespace FurnitureStoreWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Đăng ký DbContext 
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // 1. CẤU HÌNH SESSION 
            builder.Services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian sống của Session
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // 2. CẤU HÌNH COOKIE AUTHENTICATION
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login"; // Đường dẫn tự động văng về khi chưa đăng nhập
                    // ĐÂY CHÍNH LÀ ĐƯỜNG DẪN TỚI TRANG LỖI KHI CỐ TÌNH HACK (LỖI 403)
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(1); // Cookie sống trong 1 ngày
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // MỚI THÊM: Bắt mọi lỗi HTTP (như lỗi 404 khi gõ sai link) 
            // Điều hướng gọn gàng về trang thông báo lỗi của hệ thống
            app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // 3. KÍCH HOẠT SESSION 
            app.UseSession();

            // 4. BẬT XÁC THỰC (BẮT BUỘC nằm trên UseAuthorization)
            app.UseAuthentication();

            app.UseAuthorization();

            app.MapStaticAssets();

            // 5. MAP ROUTE CHO AREA ADMIN 
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