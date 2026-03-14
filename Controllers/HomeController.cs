using FurnitureStoreData.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureStoreWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            // Lấy 8 sản phẩm mới nhất từ Database (Sắp xếp theo Id giảm dần)
            var latestProducts = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .OrderByDescending(p => p.Id)
                .Take(8);

            // Truyền list sản phẩm này ra View
            return View(latestProducts);
        }
    }
}