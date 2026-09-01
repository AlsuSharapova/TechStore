using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Data;
using TechStore.ViewModels;

namespace TechStore.Controllers {
    public class ProductsController : Controller{

        public readonly AppDbContext _context;
        public ProductsController(AppDbContext context) {
            _context = context;
        }

        public async Task<IActionResult> Index(int? category, string? search, decimal? minPrice, decimal? maxPrice) {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (category.HasValue) {
                query = query.Where(p => p.CategoryId == category.Value);
            }

            if (!string.IsNullOrWhiteSpace(search)) {
                query = query.Where(p => p.Name.Contains(search));
            }

            if (minPrice.HasValue) {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue) {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            var viewModel = new ProductsIndexViewModel {
                Products = await query.ToListAsync(),
                Categories = await _context.Categories.ToListAsync(),
                SelectedCategoryId = category,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Search = search
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id) {

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Specifications)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) {
                return NotFound();
            }

            return View(product);
        }
    }
}
