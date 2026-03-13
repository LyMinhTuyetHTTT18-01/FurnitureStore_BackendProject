using FurnitureStoreData.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography; // Thêm thư viện để băm mật khẩu
using System.Text; // Thêm thư viện để xử lý chuỗi
using System.Security.Claims; // MỚI THÊM: Để lấy ID người đang đăng nhập chặn tự khóa mình

namespace FurnitureStoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================================
        // HÀM HỖ TRỢ: Băm mật khẩu ra mã SHA-256
        // ==========================================================
        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // GET: Admin/Accounts (Hiển thị danh sách tài khoản)
        public async Task<IActionResult> Index()
        {
            // Lấy toàn bộ danh sách tài khoản đẩy ra View
            var users = await _context.AppUsers.OrderByDescending(u => u.Id).ToListAsync();
            return View(users);
        }

        // ==========================================================
        // THÊM TÀI KHOẢN MỚI
        // ==========================================================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FurnitureStoreData.Models.AppUser user)
        {
            if (ModelState.IsValid)
            {
                bool isExist = await _context.AppUsers.AnyAsync(u => u.Username == user.Username);
                if (isExist)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại, vui lòng chọn tên khác!");
                    return View(user);
                }

                user.Password = ComputeSha256Hash(user.Password);
                user.IsActive = true;

                _context.Add(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm tài khoản thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // ==========================================================
        // SỬA TÀI KHOẢN (EDIT)
        // ==========================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.AppUsers.FindAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FurnitureStoreData.Models.AppUser user, string? newPassword)
        {
            if (id != user.Id) return NotFound();

            // Bỏ qua validate bắt buộc nhập Password ngoài View (vì có thể họ không muốn đổi pass)
            ModelState.Remove("Password");
            ModelState.Remove("newPassword");

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy user cũ từ DB lên để đối chiếu
                    var existingUser = await _context.AppUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
                    if (existingUser == null) return NotFound();

                    // Kiểm tra đổi Tên đăng nhập có bị trùng không
                    if (existingUser.Username != user.Username)
                    {
                        bool isExist = await _context.AppUsers.AnyAsync(u => u.Username == user.Username);
                        if (isExist)
                        {
                            ModelState.AddModelError("Username", "Tên đăng nhập này đã có người sử dụng!");
                            user.Password = existingUser.Password;
                            return View(user);
                        }
                    }

                    // Xử lý đổi mật khẩu
                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        user.Password = ComputeSha256Hash(newPassword); // Có gõ pass mới -> băm pass mới
                    }
                    else
                    {
                        user.Password = existingUser.Password; // Để trống -> giữ nguyên pass cũ
                    }

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật tài khoản thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.AppUsers.Any(e => e.Id == user.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // ==========================================================
        // KHÓA / MỞ KHÓA TÀI KHOẢN (Thay thế cho Xóa cứng)
        // ==========================================================
        public async Task<IActionResult> ToggleLock(int? id)
        {
            if (id == null) return NotFound();

            // Chốt chặn an toàn: Không cho phép Admin tự khóa chính mình
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == id.ToString())
            {
                TempData["Success"] = "Lỗi: Bạn không thể tự khóa tài khoản của chính mình!";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.AppUsers.FindAsync(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive; // Lật trạng thái
                await _context.SaveChangesAsync();

                TempData["Success"] = user.IsActive ? "Đã MỞ KHÓA tài khoản thành công!" : "Đã KHÓA tài khoản thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}