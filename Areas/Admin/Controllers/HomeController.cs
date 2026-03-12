using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureStoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Khóa bảo mật
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}