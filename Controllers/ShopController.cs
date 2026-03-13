using FurnitureStoreData.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureStoreWeb.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: /Shop/Index (Bấm vào nút Shop trên menu)
        // Nếu có truyền categoryId thì sẽ lọc sản phẩm theo danh mục đó
        public async Task<IActionResult> Index(int? categoryId, string? searchString)
        {
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            // Lọc theo danh mục nếu khách bấm từ menu Categories
            if (categoryId != null)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            // Lọc theo từ khóa nếu khách dùng ô Tìm kiếm (Kính lúp)
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString));
            }

            return View(await products.ToListAsync());
        }
        // GET: /Shop/Details/5 (Xem chi tiết 1 sản phẩm)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Tìm sản phẩm theo ID, lấy kèm luôn thông tin Danh mục (Category)
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            // Tìm thêm 4 sản phẩm cùng danh mục để làm phần "Sản phẩm liên quan"
            ViewBag.RelatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .Take(4)
                .ToListAsync();

            return View(product);
        }
    }
}