using FurnitureStoreData.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FurnitureStoreWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Hiển thị form đăng nhập
        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì đá về trang chủ, không bắt đăng nhập lại
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // 2. Xử lý logic khi bấm nút "Đăng nhập"
        [HttpPost]
        // MỚI THÊM: Nhận giá trị rememberMe từ ô Checkbox ngoài giao diện
        public async Task<IActionResult> Login(string username, string password, bool rememberMe = false)
        {
            // Tìm user trong Database
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password && u.IsActive);

            if (user != null)
            {
                // Tạo các "thẻ chứng minh nhân dân" (Claims) cho user này
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role), // Quyết định xem có vào được Admin không
                    new Claim("FullName", user.FullName)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // MỚI THÊM: Cấu hình thuộc tính cho Cookie (Ghi nhớ đăng nhập)
                var authProperties = new AuthenticationProperties
                {
                    // Nếu rememberMe = true, Cookie sẽ tồn tại ngay cả khi tắt trình duyệt
                    IsPersistent = rememberMe,
                    // Cho phép sống 30 ngày nếu được tích chọn
                    ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null
                };

                // Lưu vào Cookie (Nhớ truyền tham số authProperties vào cuối)
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Phân luồng: Admin thì vào trang Quản trị, Khách thì ra Trang chủ
                if (user.Role == "Admin")
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

        // 4. Trang thông báo cấm truy cập (ĐÃ SỬA THÀNH VIEW)
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}