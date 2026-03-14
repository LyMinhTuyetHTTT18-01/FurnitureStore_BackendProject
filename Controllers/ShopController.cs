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

            // Tìm sản phẩm theo ID, lấy kèm luôn thông tin Danh mục (Category)
            var product = _unitOfWork.Product.GetFirstOrDefault(m => m.Id == id, includeProperties: "Category");

            if (product == null) return NotFound();

            // Tìm thêm 4 sản phẩm cùng danh mục để làm phần "Sản phẩm liên quan"
            ViewBag.RelatedProducts = _unitOfWork.Product.GetAll(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .Take(4)
                .ToList();

            return View(product);
        }
    }
}