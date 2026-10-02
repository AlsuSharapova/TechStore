using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Data;
using TechStore.Models.Entities;

namespace TechStore.Controllers {
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : Controller {
        private readonly AppDbContext _context;

        public AdminOrdersController(AppDbContext context) {
            _context = context;
        }

        public async Task<IActionResult> Index() {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int id) {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status) {
            var order = await _context.Orders.FindAsync(id);

            if (order == null) {
                return NotFound();
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}