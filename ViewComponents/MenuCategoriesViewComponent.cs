using FurnitureStoreData.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureStoreWeb.ViewComponents
{
    public class MenuCategoriesViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public MenuCategoriesViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy ra tất cả danh mục hiển thị lên Menu
            var categories = await _context.Categories.ToListAsync();
            return View(categories); // Trả về file Default.cshtml
        }
    }
}