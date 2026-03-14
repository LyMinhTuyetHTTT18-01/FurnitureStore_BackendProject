using FurnitureStoreData.Models;
using FurnitureStoreData.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FurnitureStoreWeb.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public WishlistController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdStr);
            var wishlistItems = _unitOfWork.Wishlist.GetAll(w => w.AppUserId == userId, includeProperties: "Product");

            return View(wishlistItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để sử dụng tính năng này!" });
            }

            int userId = int.Parse(userIdStr);

            // Kiểm tra xem đã có trong wishlist chưa
            var existing = _unitOfWork.Wishlist.GetFirstOrDefault(w => w.AppUserId == userId && w.ProductId == productId);
            if (existing != null)
            {
                return Json(new { success = false, message = "Sản phẩm này đã có trong danh sách yêu thích!" });
            }

            var wishlistItem = new Wishlist
            {
                AppUserId = userId,
                ProductId = productId,
                AddedDate = DateTime.Now
            };

            _unitOfWork.Wishlist.Add(wishlistItem);
            await _unitOfWork.SaveAsync();

            return Json(new { success = true, message = "Đã thêm vào danh sách yêu thích!" });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int id)
        {
            var wishlistItem = _unitOfWork.Wishlist.GetFirstOrDefault(w => w.Id == id);
            if (wishlistItem == null) return NotFound();

            _unitOfWork.Wishlist.Remove(wishlistItem);
            await _unitOfWork.SaveAsync();

            return Json(new { success = true, message = "Đã xóa khỏi danh sách yêu thích!" });
        }
    }
}
