using FurnitureStoreData.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FurnitureStoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")] // Cho phép cả Admin và Nhân viên xem thống kê
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Thống kê 4 ô vuông tổng quan trên cùng
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            // Tính tổng tiền của các đơn hàng có Status = 2 (Hoàn thành)
            ViewBag.TotalRevenue = await _context.Orders.Where(o => o.Status == 2).SumAsync(o => o.TotalAmount);
            ViewBag.TotalUsers = await _context.AppUsers.CountAsync();
            ViewBag.TotalProducts = await _context.Products.CountAsync();

            // 2. Lấy dữ liệu biểu đồ doanh thu theo 6 tháng gần nhất
            var currentDate = DateTime.Now;
            var sixMonthsAgo = currentDate.AddMonths(-5);

            // Rút dữ liệu từ SQL lên RAM trước để tránh lỗi dịch thuật của EF Core khi gom nhóm theo ngày tháng
            var recentOrders = await _context.Orders
                .Where(o => o.Status == 2 && o.OrderDate >= new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1))
                .ToListAsync();

            var revenueData = recentOrders
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Month = $"Tháng {g.Key.Month}/{g.Key.Year}",
                    Total = g.Sum(o => o.TotalAmount)
                })
                .ToList();

            // Truyền mảng dữ liệu sang View để Chart.js đọc
            ViewBag.ChartLabels = revenueData.Select(r => r.Month).ToArray();
            ViewBag.ChartData = revenueData.Select(r => r.Total).ToArray();

            return View();
        }
    }
}