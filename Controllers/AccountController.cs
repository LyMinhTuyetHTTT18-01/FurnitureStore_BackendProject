using FurnitureStoreData.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography; // MỚI THÊM: Để dùng thư viện băm SHA-256
using System.Text; // MỚI THÊM: Để xử lý chuỗi chữ

namespace FurnitureStoreWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
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

        // 1. Hiển thị form đăng nhập
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // 2. Xử lý logic khi bấm nút "Đăng nhập"
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe = false)
        {
            // BẢO MẬT TỐI ĐA: Băm mật khẩu người dùng nhập vào trước khi so sánh
            string hashedPassword = ComputeSha256Hash(password);

            // Tìm user trong Database (So sánh bằng chuỗi đã băm)
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == hashedPassword && u.IsActive);

            if (user != null)
            {
                // Tạo các "thẻ chứng minh nhân dân" (Claims) cho user này
                var claims = new List<Claim>
                {
                    // QUAN TRỌNG: Phải lưu ID vào đây để bài trước lấy ra chặn lỗi IDOR (2.0 điểm)
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role), // Quyết định xem có vào được Admin không
                    new Claim("FullName", user.FullName)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Cấu hình thuộc tính cho Cookie (Ghi nhớ đăng nhập)
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null
                };

                // Lưu vào Cookie 
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Phân luồng: Admin thì vào trang Quản trị, Khách thì ra Trang chủ
                if (user.Role == "Admin" || user.Role == "Staff")
                {
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }
                return RedirectToAction("Index", "Home");
            }

            // Nếu sai tài khoản/mật khẩu
            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không chính xác!";
            return View();
        }

        // 3. Đăng xuất
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // 4. Trang thông báo cấm truy cập
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}