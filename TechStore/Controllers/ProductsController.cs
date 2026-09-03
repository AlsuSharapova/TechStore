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

        public async Task<IActionResult> Index(int? category, string? search, decimal? minPrice, decimal? maxPrice, string? sortBy, List<string>? specs) {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Specifications)
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

            // Фильтр по характеристикам
            if (specs != null && specs.Any()) {
                var groupedSpecs = specs
                    .Select(s => s.Split(':'))
                    .GroupBy(parts => parts[0], parts => parts[1]);

                foreach (var group in groupedSpecs) {
                    var specName = group.Key;
                    var specValues = group.ToList();

                    query = query.Where(p => p.Specifications.Any(s => s.Name == specName && specValues.Contains(s.Value)));
                }
            }

            query = sortBy switch {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                "popular" => query.OrderByDescending(p => p.IsBestSeller),
                _ => query
            };

            var products = await query.ToListAsync();

            // Собираем доступные фильтры по характеристикам ТОЛЬКО для выбранной категории
            var availableSpecFilters = new List<SpecFilterGroup>();

            if (category.HasValue) {
                var categorySpecs = await _context.ProductSpecifications
                    .Where(s => s.Product.CategoryId == category.Value)
                    .Select(s => new { s.Name, s.Value })
                    .Distinct()
                    .ToListAsync();

                availableSpecFilters = categorySpecs
                    .GroupBy(s => s.Name)
                    .Select(g => new SpecFilterGroup {
                        Name = g.Key,
                        Values = g.Select(x => x.Value).Distinct().ToList()
                    })
                    .ToList();
            }

            var viewModel = new ProductsIndexViewModel {
                Products = products,
                Categories = await _context.Categories.ToListAsync(),
                SelectedCategoryId = category,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Search = search,
                SortBy = sortBy,
                SelectedSpecs = specs ?? new List<string>(),
                AvailableSpecFilters = availableSpecFilters
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
