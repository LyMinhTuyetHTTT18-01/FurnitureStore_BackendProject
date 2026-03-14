using FurnitureStoreData.Models;
using FurnitureStoreData.Repositories;
using FurnitureStoreWeb.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Security.Claims;

namespace FurnitureStoreWeb.Controllers
{
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private const string CART_KEY = "CartSession";

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // 1. Lấy danh sách giỏ hàng từ Session
        private List<CartItem> GetCartItems()
        {
            var sessionData = HttpContext.Session.GetString(CART_KEY);
            if (string.IsNullOrEmpty(sessionData))
            {
                return new List<CartItem>();
            }
            return JsonSerializer.Deserialize<List<CartItem>>(sessionData) ?? new List<CartItem>();
        }

        // 2. Lưu danh sách giỏ hàng vào Session
        private void SaveCartItems(List<CartItem> cart)
        {
            var sessionData = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CART_KEY, sessionData);
        }

        // GET: /Cart/Index
        public IActionResult Index()
        {
            var cart = GetCartItems();
            ViewBag.Total = cart.Sum(i => i.Total);
            return View(cart);
        }

        // GET: /Cart/Add/5
        public IActionResult Add(int id)
        {
            var product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            var cart = GetCartItems();
            var item = cart.FirstOrDefault(i => i.ProductId == id);

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = 1,
                    ImageUrl = product.ImageUrl ?? "no-image.png"
                });
            }
            else
            {
                item.Quantity++;
            }

            SaveCartItems(cart);
            TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng!";
            
            // Quay lại trang trước đó hoặc trang Shop
            var returnUrl = Request.Headers["Referer"].ToString();
            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction("Index", "Shop");
            return Redirect(returnUrl);
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (quantity > 0)
                    item.Quantity = quantity;
                else
                    cart.Remove(item);
            }
            SaveCartItems(cart);
            return RedirectToAction("Index");
        }

        // GET: /Cart/Remove/5
        public IActionResult Remove(int id)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(i => i.ProductId == id);
            if (item != null)
            {
                cart.Remove(item);
            }
            SaveCartItems(cart);
            return RedirectToAction("Index");
        }

        // GET: /Cart/Checkout
        public IActionResult Checkout()
        {
            var cart = GetCartItems();
            if (cart.Count == 0)
            {
                return RedirectToAction("Index", "Shop");
            }
            ViewBag.Total = cart.Sum(i => i.Total);
            return View();
        }

        // POST: /Cart/Checkout
        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = GetCartItems();
            if (cart.Count == 0) return RedirectToAction("Index", "Shop");

            if (ModelState.IsValid)
            {
                // Gán thông tin bổ sung cho đơn hàng
                order.OrderDate = DateTime.Now;
                order.TotalAmount = cart.Sum(i => i.Total);
                order.Status = 0; // Chờ xử lý

                // Nếu user đã đăng nhập, gán AppUserId
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim != null)
                    {
                        order.AppUserId = int.Parse(userIdClaim.Value);
                    }
                }

                // Lưu Order để lấy ID
                _unitOfWork.Order.Add(order);
                await _unitOfWork.SaveAsync();

                // Lưu OrderDetails
                foreach (var item in cart)
                {
                    var detail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    };
                    _unitOfWork.OrderDetail.Add(detail);
                }

                await _unitOfWork.SaveAsync();

                // Xóa giỏ hàng sau khi thanh toán
                HttpContext.Session.Remove(CART_KEY);

                TempData["Success"] = "Đặt hàng thành công! Chúng tôi sẽ liên hệ sớm nhất.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Total = cart.Sum(i => i.Total);
            return View(order);
        }
    }
}
