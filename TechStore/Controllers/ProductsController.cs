using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Data;

namespace TechStore.Controllers {
    public class ProductsController : Controller{

        public readonly AppDbContext _context;
        public ProductsController(AppDbContext context) {
            _context = context;
        }
        public async Task<IActionResult> Index(int? category, string? search) {

            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (category.HasValue) {
                query = query.Where(p => p.CategoryId == category.Value);
            }

            if (!string.IsNullOrWhiteSpace(search)) {
                query = query.Where(p => p.Name.Contains(search));
            }

            var products = await query.ToListAsync();

            return View(products);
        }
    }
}
