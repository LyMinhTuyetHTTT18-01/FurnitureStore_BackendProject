using FurnitureStoreData.Models;
using FurnitureStoreData.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureStoreWeb.Controllers
{
    public class ShopController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShopController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: /Shop/Index
        public IActionResult Index(int? categoryId, string? searchString)
        {
            var products = _unitOfWork.Product.GetAll(includeProperties: "Category");

            // Lọc theo danh mục nếu khách bấm từ menu Categories
            if (categoryId != null)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            // Lọc theo từ khóa nếu khách dùng ô Tìm kiếm (Kính lúp)
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            return View(products.ToList());
        }

        // GET: /Shop/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var product = _unitOfWork.Product.GetFirstOrDefault(m => m.Id == id, includeProperties: "Category");

            if (product == null) return NotFound();

            // Tìm thêm 4 sản phẩm cùng danh mục
            ViewBag.RelatedProducts = _unitOfWork.Product.GetAll(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .Take(4)
                .ToList();

            // Lấy danh sách đánh giá
            ViewBag.Reviews = _unitOfWork.ProductReview.GetAll(r => r.ProductId == id, includeProperties: "AppUser")
                .OrderByDescending(r => r.CreatedDate)
                .ToList();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostReview(int productId, int rating, string comment)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để đánh giá!" });
            }

            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return BadRequest();

            var review = new ProductReview
            {
                ProductId = productId,
                AppUserId = int.Parse(userIdStr),
                Rating = rating,
                Comment = comment,
                CreatedDate = DateTime.Now
            };

            _unitOfWork.ProductReview.Add(review);
            await _unitOfWork.SaveAsync();

            return Json(new { success = true, message = "Cảm ơn bạn đã đánh giá sản phẩm!" });
        }

        // GET: /Shop/GetQuickView/5
        // GET: /Shop/GetQuickView/5
        [HttpGet]
        public IActionResult GetQuickView(int id)
        {
            var product = _unitOfWork.Product.GetFirstOrDefault(m => m.Id == id, includeProperties: "Category");
            if (product == null) return NotFound();

            return Json(new
            {
                id = product.Id,
                name = product.Name,
                price = product.Price.ToString("N0") + " đ",
                imageUrl = product.ImageUrl,
                description = product.Description,
                categoryName = product.Category?.Name
            });
        }
    }
}