using FurnitureStoreData.Context;
using FurnitureStoreData.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureStoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class ProductReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var reviews = await _context.ProductReviews
                .Include(r => r.Product)
                .Include(r => r.AppUser)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
            return View(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.ProductReviews.FindAsync(id);
            if (review != null)
            {
                _context.ProductReviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa đánh giá thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
