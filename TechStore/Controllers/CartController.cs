using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Data;
using TechStore.Models.Entities;
using TechStore.ViewModels;
using TechStore.ViewModels.Account;
using Stripe.Checkout;
using Microsoft.Extensions.Localization;
using TechStore.Resources;

namespace TechStore.Controllers {
    [Authorize]
    public class CartController : Controller {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public CartController(AppDbContext context, UserManager<ApplicationUser> userManager, IStringLocalizer<SharedResource> localizer) {
            _context = context;
            _userManager = userManager;
            _localizer = localizer;
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
                    TempData["CartMessage"] = _localizer["CartNoMoreStock", existingItem.Product.Name].Value;
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
                        adjustedMessages.Add(_localizer["CartItemSoldOut", item.Product.Name].Value);
                        _context.CartItems.Remove(item);
                    }
                    else {
                        adjustedMessages.Add(_localizer["CartQuantityReduced", item.Product.Name, item.Product.StockQuantity].Value);
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
                        _localizer["CheckoutStockError", item.Product.Name, item.Product.StockQuantity].Value);
                }
            }

            if (!ModelState.IsValid) {
                model.Cart = cart;
                return View(model);
            }

            // Создаём "черновик" заказа — товар пока НЕ списывается, корзина НЕ очищается
            var order = new Order {
                UserId = userId!,
                DeliveryAddress = model.DeliveryAddress,
                PhoneNumber = model.PhoneNumber,
                TotalPrice = cart.Items.Sum(i => i.Product.Price * i.Quantity),
                Status = OrderStatus.AwaitingPayment
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
            }

            await _context.SaveChangesAsync();

            // Создаём сессию оплаты Stripe
            var lineItems = cart.Items.Select(item => new SessionLineItemOptions {
                PriceData = new SessionLineItemPriceDataOptions {
                    Currency = "azn",
                    UnitAmount = (long)(item.Product.Price * 100),
                    ProductData = new SessionLineItemPriceDataProductDataOptions {
                        Name = item.Product.Name
                    }
                },
                Quantity = item.Quantity
            }).ToList();

            var options = new SessionCreateOptions {
                AllowedPaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = Url.Action("PaymentSuccess", "Cart", new { orderId = order.Id }, Request.Scheme) + "&session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = Url.Action("PaymentCancel", "Cart", new { orderId = order.Id }, Request.Scheme)
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Redirect(session.Url);
        }
        [HttpGet]
        public async Task<IActionResult> PaymentSuccess(int orderId, string session_id) {
            var service = new SessionService();
            Session session = await service.GetAsync(session_id);

            if (session.PaymentStatus != "paid") {
                return RedirectToAction("PaymentCancel", new { orderId });
            }

            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) {
                return NotFound();
            }

            if (order.Status == OrderStatus.AwaitingPayment) {
                order.Status = OrderStatus.New;

                foreach (var item in order.Items) {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null) {
                        product.StockQuantity -= item.Quantity;
                    }
                }

                var userId = _userManager.GetUserId(User);
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart != null) {
                    _context.CartItems.RemoveRange(cart.Items);
                }

                await _context.SaveChangesAsync();
            }

            return View("InfoMessage", new InfoMessageViewModel {
                Icon = "✅",
                Title = _localizer["OrderPlaced"].Value,
                Message = _localizer["OrderPaidText", order.Id].Value,
                ButtonText = _localizer["MyOrders"].Value,
                ButtonController = "Orders",
                ButtonAction = "Index"
            });
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCancel(int orderId) {
            var order = await _context.Orders.FindAsync(orderId);

            if (order != null && order.Status == OrderStatus.AwaitingPayment) {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }

            return View("InfoMessage", new InfoMessageViewModel {
                Icon = "❌",
                Title = _localizer["PaymentCancelled"].Value,
                Message = _localizer["PaymentCancelledText"].Value,
                ButtonText = _localizer["BackToCart"].Value,
                ButtonController = "Cart",
                ButtonAction = "Index"
            });
        }
        
    }
}