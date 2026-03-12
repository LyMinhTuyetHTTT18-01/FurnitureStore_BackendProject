using FurnitureStoreData.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO; // Dùng để xử lý đường dẫn thư mục lưu ảnh
using Microsoft.AspNetCore.Http; // Dùng để nhận file ảnh (IFormFile) từ View

namespace FurnitureStoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Bảo mật URL
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Tiêm DbContext vào (Dependency Injection - Yêu cầu của bài)
        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================================
        // 1. DANH SÁCH & TÌM KIẾM, PHÂN TRANG
        // ==========================================================
        // GET: Admin/Products (Có tìm kiếm và phân trang bằng Skip/Take)
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 5; // Số lượng sản phẩm hiển thị trên 1 trang

            // Lấy danh sách sản phẩm, kèm theo thông tin Category (Eager Loading)
            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            // Logic Tìm kiếm (Nếu người dùng có gõ vào ô tìm kiếm)
            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchString));
            }

            // Logic Phân trang (Tính toán tổng số trang)
            int totalItems = await productsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Lấy dữ liệu của trang hiện tại (Dùng Skip và Take bất đồng bộ)
            var data = await productsQuery
                .OrderByDescending(p => p.Id) // Sản phẩm mới nhất lên đầu
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền dữ liệu sang View để hiển thị
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;

            return View(data);
        }

        // ==========================================================
        // 2. THÊM MỚI SẢN PHẨM VÀ UPLOAD ẢNH
        // ==========================================================

        // GET: Admin/Products/Create (Hiển thị Form nhập liệu)
        public IActionResult Create()
        {
            // Lấy danh sách Category (Danh mục) đẩy sang View để làm cái Dropdown (thẻ <select>)
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Admin/Products/Create (Xử lý khi bấm nút Lưu)
        [HttpPost]
        [ValidateAntiForgeryToken] // Chống hack form
        public async Task<IActionResult> Create(FurnitureStoreData.Models.Product product, IFormFile? imageFile)
        {
            // Dòng này rất quan trọng để bỏ qua việc validate bắt buộc nhập file ảnh nếu bạn muốn cho phép up ảnh sau
            ModelState.Remove("imageFile");

            if (ModelState.IsValid)
            {
                // XỬ LÝ UPLOAD HÌNH ẢNH
                if (imageFile != null && imageFile.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    string filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    product.ImageUrl = fileName;
                }
                else
                {
                    product.ImageUrl = "default.png";
                }

                // LƯU VÀO DATABASE
                _context.Add(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // ==========================================================
        // 3. SỬA SẢN PHẨM (EDIT)
        // ==========================================================

        // GET: Hiển thị form Sửa với dữ liệu cũ
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Xử lý khi bấm nút "Cập nhật"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FurnitureStoreData.Models.Product product, IFormFile? imageFile)
        {
            if (id != product.Id) return NotFound();

            ModelState.Remove("imageFile"); // Bỏ qua bắt buộc nhập ảnh mới

            if (ModelState.IsValid)
            {
                try
                {
                    // Nếu người dùng CÓ chọn ảnh mới để thay thế
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");

                        // Tiện tay xóa ảnh cũ đi cho nhẹ ổ cứng (nếu không phải ảnh default)
                        if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl != "default.png")
                        {
                            string oldFilePath = Path.Combine(uploadPath, product.ImageUrl);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Upload ảnh mới
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string filePath = Path.Combine(uploadPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        product.ImageUrl = fileName; // Cập nhật tên ảnh mới
                    }
                    // Ghi chú: Nếu không up ảnh mới, product.ImageUrl vẫn giữ tên cũ nhờ thẻ input hidden ngoài View

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == product.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // ==========================================================
        // 4. XÓA SẢN PHẨM (DELETE)
        // ==========================================================

        // GET: Hiển thị trang xác nhận Xóa
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Xử lý xóa thật dưới Database
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Xóa luôn cái file ảnh vật lý trong máy cho sạch rác
                if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl != "default.png")
                {
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                    string filePath = Path.Combine(uploadPath, product.ImageUrl);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}