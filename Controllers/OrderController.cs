using FurnitureStoreData.Models;
using FurnitureStoreData.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FurnitureStoreWeb.Controllers
{
    [Authorize] // Yêu cầu đăng nhập để xem lịch sử đơn hàng
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: /Order/Index
        // Hiển thị danh sách các đơn hàng của user đang đăng nhập
        public IActionResult Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim.Value);

            var orders = _unitOfWork.Order.GetAll(o => o.AppUserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // GET: /Order/Details/5
        // Hiển thị chi tiết của một đơn hàng
        public IActionResult Details(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim.Value);

            // Fetch order and include details & products
            // using string based include for multiple levels
            var order = _unitOfWork.Order.GetFirstOrDefault(o => o.Id == id && o.AppUserId == userId, includeProperties: "OrderDetails.Product");

            if (order == null)
            {
                return NotFound("Không tìm thấy đơn hàng hoặc bạn không có quyền xem đơn hàng này.");
            }

            return View(order);
        }
    }
}
