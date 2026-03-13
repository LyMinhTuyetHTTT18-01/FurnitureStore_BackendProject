using FurnitureStoreData.Context;
using FurnitureStoreData.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Để lấy ID của Admin đang đăng nhập
using System.IO;

namespace FurnitureStoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================================
        // 1. DANH SÁCH BÀI VIẾT
        // ==========================================================
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10;
            // Dùng Include để lấy tên Admin đã đăng bài
            var query = _context.Posts.Include(p => p.AppUser).AsQueryable();

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(posts);
        }

        // ==========================================================
        // 2. THÊM BÀI VIẾT
        // ==========================================================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post post, IFormFile? imageFile)
        {
            ModelState.Remove("imageFile"); // Bỏ qua validate bắt buộc file

            if (ModelState.IsValid)
            {
                // 1. Xử lý Upload Ảnh bìa bài viết
                if (imageFile != null && imageFile.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "posts");

                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                    using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    post.ImageUrl = fileName;
                }
                else
                {
                    post.ImageUrl = "default-post.png";
                }

                // 2. Tự động lấy ID của Admin đang đăng nhập gán cho bài viết
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdString))
                {
                    post.AppUserId = int.Parse(userIdString);
                }

                post.CreatedAt = DateTime.Now; // Chốt thời gian đăng

                _context.Add(post);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đăng bài viết mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // ==========================================================
        // 3. SỬA BÀI VIẾT
        // ==========================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Post post, IFormFile? imageFile)
        {
            if (id != post.Id) return NotFound();
            ModelState.Remove("imageFile");

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "posts");

                    // Xóa ảnh cũ
                    if (!string.IsNullOrEmpty(post.ImageUrl) && post.ImageUrl != "default-post.png")
                    {
                        var oldPath = Path.Combine(uploadPath, post.ImageUrl);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    // Lưu ảnh mới
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    post.ImageUrl = fileName;
                }

                _context.Update(post);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật bài viết thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // ==========================================================
        // 4. XÓA BÀI VIẾT
        // ==========================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var post = await _context.Posts.FirstOrDefaultAsync(m => m.Id == id);
            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                if (!string.IsNullOrEmpty(post.ImageUrl) && post.ImageUrl != "default-post.png")
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "posts", post.ImageUrl);
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa bài viết thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}