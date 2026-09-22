using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Data;
using TechStore.Models.Entities;
using TechStore.ViewModels.Admin;


namespace TechStore.Controllers {
    public class AdminCategoriesController : Controller {
        private readonly AppDbContext _context;
        public AdminCategoriesController(AppDbContext context) {
            _context = context;
        }

        public async Task<IActionResult> Index() {
            var categories = await _context.Categories
                .OrderByDescending(c => c.Name)
                .ToListAsync();

            return View(categories);
        }
        [HttpGet]
        public IActionResult Create() {
            return View(new CategoryFormViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryFormViewModel model) {
            if (!ModelState.IsValid) {
                return View(model);
            }

            var category = new Category {
                Name = model.Name,
                Icon = model.Icon,
                IsFeatured = model.IsFeatured
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id) {
            var category = await _context.Categories.FindAsync(id);

            if (category == null) {
                return NotFound();
            }

            var model = new CategoryFormViewModel {
                Name = category.Name,
                Icon = category.Icon,
                IsFeatured = category.IsFeatured
            };

            ViewBag.CategoryId = category.Id;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CategoryFormViewModel model) {
            var category = await _context.Categories.FindAsync(id);

            if (category == null) {
                return NotFound();
            }

            if (!ModelState.IsValid) {
                ViewBag.CategoryId = id;
                return View(model);
            }

            category.Name = model.Name;
            category.Icon = model.Icon;
            category.IsFeatured = model.IsFeatured;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id) {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) {
                return NotFound();
            }

            if (category.Products.Any()) {
                TempData["ErrorMessage"] = $"Нельзя удалить категорию «{category.Name}» — в ней есть товары ({category.Products.Count} шт.). Сначала удалите или перенесите их.";
                return RedirectToAction("Index");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
