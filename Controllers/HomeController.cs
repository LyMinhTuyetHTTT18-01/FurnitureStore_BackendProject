using FurnitureStoreData.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureStoreWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Tiêm Database vào
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy 8 sản phẩm mới nhất từ Database (Sắp xếp theo Id giảm dần)
            var latestProducts = await _context.Products
                .Include(p => p.Category) // Lấy luôn thông tin Danh mục nếu cần
                .OrderByDescending(p => p.Id)
                .Take(8)
                .ToListAsync();

            // Truyền list sản phẩm này ra View
            return View(latestProducts);
        }
    }
}