using FurnitureStoreData.Context;
using FurnitureStoreData.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureStoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")] // Khóa bảo mật URL
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET: Admin/Categories (Danh sách có Tìm kiếm & Phân trang)
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 5;
            var query = _context.Categories.AsQueryable();

            // Logic tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c => c.Name.Contains(searchString));
            }

            // Logic phân trang
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var data = await query
                .OrderByDescending(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;

            return View(data);
        }

        // 2. GET: Admin/Categories/Create (Hiển thị Form thêm mới)
        public IActionResult Create()
        {
            return View();
        }

        // 3. POST: Admin/Categories/Create (Xử lý lưu vào Database)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync(); // Lưu xuống SQL
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // ==========================================================
        // 4. SỬA DANH MỤC (EDIT)
        // ==========================================================

        // GET: Hiển thị form Sửa
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Xử lý lưu cập nhật
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(e => e.Id == category.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // ==========================================================
        // 5. XÓA DANH MỤC (DELETE)
        // ==========================================================

        // GET: Hiển thị trang xác nhận Xóa
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Xử lý xóa thật dưới DB
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}