using FurnitureStoreData.Models;
using FurnitureStoreData.Repositories;
using FurnitureStoreWeb.Models.ViewModels;
using FurnitureStoreWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography; // MỚI THÊM: Để dùng thư viện băm SHA-256
using System.Text; 

namespace FurnitureStoreWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public AccountController(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
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
            var user = _unitOfWork.AppUser
                .GetFirstOrDefault(u => u.Username == username && u.Password == hashedPassword && u.IsActive);

            if (user != null)
            {
                await SignInUser(user, rememberMe);

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

        private async Task SignInUser(AppUser user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role ?? "Customer"),
                new Claim("FullName", user.FullName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
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

        // ==========================================================
        // 5. CHỨC NĂNG ĐĂNG KÝ
        // ==========================================================
        
        [HttpGet]
        public IActionResult Register()
        {
             if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _unitOfWork.AppUser.GetFirstOrDefault(u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã được sử dụng!");
                    return View(model);
                }

                // Kiểm tra Email đã tồn tại chưa
                var existingEmail = _unitOfWork.AppUser.GetFirstOrDefault(u => u.Email == model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng!");
                    return View(model);
                }

                var newUser = new FurnitureStoreData.Models.AppUser
                {
                    Username = model.Username,
                    Password = ComputeSha256Hash(model.Password),
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Role = "Customer",
                    IsActive = true
                };

                _unitOfWork.AppUser.Add(newUser);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Đăng ký tài khoản thành công! Mời bạn đăng nhập.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // ==========================================================
        // 6. ĐĂNG NHẬP BẰNG EMAIL (OTP)
        // ==========================================================
        [HttpGet]
        public IActionResult LoginWithEmail()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Vui lòng nhập Email!";
                return View("LoginWithEmail");
            }

            var user = _unitOfWork.AppUser.GetFirstOrDefault(u => u.Email == email && u.IsActive);
            if (user == null)
            {
                ViewBag.Error = "Email chưa được gắn với tài khoản nào hoặc tài khoản bị khóa!";
                return View("LoginWithEmail");
            }

            // Sinh mã OTP 6 số
            string otpCode = new Random().Next(100000, 999999).ToString();
            
            // Lưu vào DB
            var otpEntity = new OtpVerification
            {
                Email = email,
                OtpCode = otpCode,
                CreatedAt = DateTime.Now,
                ExpirationTime = DateTime.Now.AddMinutes(5), // Hết hạn sau 5 phút
                IsUsed = false
            };

            _unitOfWork.OtpVerification.Add(otpEntity);
            await _unitOfWork.SaveAsync();

            try
            {
                // Gửi qua Email (SMTP)
                string subject = "Mã xác thực đăng nhập - FurnitureStore";
                string htmlMessage = $"<h2>Mã xác thực đăng nhập của bạn là: <strong>{otpCode}</strong></h2>" +
                                     $"<p>Mã này sẽ hết hạn sau 5 phút. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>";

                await _emailService.SendEmailAsync(email, subject, htmlMessage);
                
                TempData["OTPEmail"] = email;
                return RedirectToAction("VerifyOtp");
            }
            catch (Exception ex)
            {
                // Xử lý lỗi cẩn thận kẻo crash app
                ViewBag.Error = "Có lỗi xảy ra khi gửi Email. Vui lòng kiểm tra lại cấu hình SMTP.";
                return View("LoginWithEmail");
            }
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            if (TempData["OTPEmail"] == null)
            {
                return RedirectToAction("LoginWithEmail");
            }
            
            ViewBag.Email = TempData["OTPEmail"].ToString();
            TempData.Keep("OTPEmail"); // Giữ lại giá trị để dùng khi Post
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string email, string otpCode)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otpCode))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                ViewBag.Email = email;
                return View();
            }

            // Tìm OTP chưa sử dụng, đúng mã, đúng email và chưa hết hạn
            var otpRecord = _unitOfWork.OtpVerification.GetFirstOrDefault(
                o => o.Email == email 
                  && o.OtpCode == otpCode 
                  && o.IsUsed == false
                  && o.ExpirationTime > DateTime.Now);

            if (otpRecord != null)
            {
                // Đánh dấu đã dùng
                otpRecord.IsUsed = true;
                _unitOfWork.OtpVerification.Update(otpRecord);
                await _unitOfWork.SaveAsync();

                // SignIn
                var user = _unitOfWork.AppUser.GetFirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    await SignInUser(user, rememberMe: true);
                    TempData["Success"] = "Đăng nhập thành công bằng mã OTP!";
                    
                    if (user.Role == "Admin" || user.Role == "Staff")
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Mã xác nhận không đúng hoặc đã hết hạn!";
            ViewBag.Email = email;
            TempData.Keep("OTPEmail");
            return View();
        }
    }
}