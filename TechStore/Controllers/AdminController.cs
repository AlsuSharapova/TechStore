using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Data;
using TechStore.ViewModels.Admin;

namespace TechStore.Controllers {
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context) {
            _context = context;
        }

        public async Task<IActionResult> Index() {
            var model = new DashboardViewModel {
                TotalProducts = await _context.Products.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalRevenue = await _context.Orders
                    .Where(o => o.Status != Models.Entities.OrderStatus.Cancelled)
                    .SumAsync(o => o.TotalPrice),
                RecentOrders = await _context.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}