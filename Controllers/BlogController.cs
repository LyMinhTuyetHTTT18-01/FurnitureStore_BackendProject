using FurnitureStoreData.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureStoreWeb.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Blog/Index (Danh sách bài viết)
        public async Task<IActionResult> Index()
        {
            // Chỉ lấy các bài viết đã được Admin check chọn "Xuất bản" (IsPublished == true)
            // Lấy kèm tên Admin đã viết bài (Include AppUser) và sắp xếp mới nhất lên đầu
            var posts = await _context.Posts
                .Include(p => p.AppUser)
                .Where(p => p.IsPublished == true)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        // GET: /Blog/Details/5 (Xem chi tiết 1 bài viết)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.Posts
                .Include(p => p.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsPublished == true);

            if (post == null) return NotFound();

            // Lấy 3 bài viết mới nhất (trừ bài hiện tại) để làm mục "Bài viết liên quan" bên góc phải
            ViewBag.RecentPosts = await _context.Posts
                .Where(p => p.IsPublished == true && p.Id != post.Id)
                .OrderByDescending(p => p.CreatedAt)
                .Take(3)
                .ToListAsync();

            return View(post);
        }
    }
}