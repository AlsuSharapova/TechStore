using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Data;
using TechStore.Models.Entities;
using TechStore.ViewModels;
using TechStore.ViewModels.Account;

namespace TechStore.Controllers {
    [Authorize]
    public class CartController : Controller {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(AppDbContext context, UserManager<ApplicationUser> userManager) {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index() {
            var userId = _userManager.GetUserId(User);

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1) {
            var userId = _userManager.GetUserId(User);

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(c => c.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) {
                cart = new Cart { UserId = userId! };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null) {
                if (existingItem.Quantity + quantity > existingItem.Product.StockQuantity) {
                    TempData["CartMessage"] =  $"Больше товара {existingItem.Product.Name} нет в наличии.";
                    return RedirectToAction("Index");
                }
                existingItem.Quantity += quantity;
            }
            else {
                _context.CartItems.Add(new CartItem {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int itemId, int quantity) {
            var userId = _userManager.GetUserId(User);

            var item = await _context.CartItems
                .Include(i => i.Cart)
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.Cart.UserId == userId);

            if (item == null) {
                return NotFound();
            }

            if (quantity < 1) {
                quantity = 1;
            }

            if (quantity > item.Product.StockQuantity) {
                quantity = item.Product.StockQuantity;
            }

            item.Quantity = quantity;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int itemId) {
            var userId = _userManager.GetUserId(User);

            var item = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.Cart.UserId == userId);

            if (item == null) {
                return NotFound();
            }

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Checkout() {
            var userId = _userManager.GetUserId(User);

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any()) {
                return RedirectToAction("Index");
            }

            var adjustedMessages = new List<string>();

            foreach (var item in cart.Items.ToList()) {
                if (item.Quantity > item.Product.StockQuantity) {
                    if (item.Product.StockQuantity <= 0) {
                        adjustedMessages.Add($"«{item.Product.Name}» закончился и удалён из корзины.");
                        _context.CartItems.Remove(item);
                    }
                    else {
                        adjustedMessages.Add($"Количество «{item.Product.Name}» уменьшено до {item.Product.StockQuantity} шт. (столько осталось в наличии).");
                        item.Quantity = item.Product.StockQuantity;
                    }
                }
            }

            if (adjustedMessages.Any()) {
                await _context.SaveChangesAsync();
                TempData["CartMessage"] = string.Join(" ", adjustedMessages);
                return RedirectToAction("Index");
            }

            if (!cart.Items.Any()) {
                return RedirectToAction("Index");
            }

            var model = new CheckoutViewModel {
                Cart = cart
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model) {
            var userId = _userManager.GetUserId(User);

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any()) {
                return RedirectToAction("Index");
            }

            // Финальная проверка остатков перед оформлением
            foreach (var item in cart.Items) {
                if (item.Quantity > item.Product.StockQuantity) {
                    ModelState.AddModelError(string.Empty,
                        $"Товара «{item.Product.Name}» осталось только {item.Product.StockQuantity} шт. Пожалуйста, измените количество в корзине.");
                }
            }

            if (!ModelState.IsValid) {
                model.Cart = cart;
                return View(model);
            }

            var order = new Order {
                UserId = userId!,
                DeliveryAddress = model.DeliveryAddress,
                PhoneNumber = model.PhoneNumber,
                TotalPrice = cart.Items.Sum(i => i.Product.Price * i.Quantity),
                Status = OrderStatus.New
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cart.Items) {
                _context.OrderItems.Add(new OrderItem {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    ProductPrice = item.Product.Price,
                    Quantity = item.Quantity
                });

                // Уменьшаем остаток товара на складе
                item.Product.StockQuantity -= item.Quantity;
            }

            // Очищаем корзину
            _context.CartItems.RemoveRange(cart.Items);

            await _context.SaveChangesAsync();

            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }

        [HttpGet]
        public IActionResult OrderConfirmation(int orderId) {
            return View("InfoMessage", new InfoMessageViewModel {
                Icon = "✅",
                Title = "Заказ оформлен!",
                Message = $"Ваш заказ №{orderId} принят в обработку. Мы свяжемся с вами для подтверждения доставки.",
                ButtonText = "Мои заказы",
                ButtonController = "Orders",
                ButtonAction = "Index"
            });
        }
    }
}