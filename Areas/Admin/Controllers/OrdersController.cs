using FurnitureStoreData.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureStoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET: Admin/Orders
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10;
            // Đã xóa .Include(o => o.AppUser) vì bạn không nối bảng
            var query = _context.Orders.AsQueryable();

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(orders);
        }

        // 2. GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.OrderDetails) // Lấy chi tiết đơn
                    .ThenInclude(od => od.Product) // Lấy luôn thông tin sản phẩm
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // 3. POST: Admin/Orders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, int status) // Sửa thành int status
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = status; // 0, 1, 2, 3
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công!";
            }
            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}