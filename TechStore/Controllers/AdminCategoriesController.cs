using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Data;
using TechStore.Models.Entities;
using TechStore.ViewModels.Admin;
using Microsoft.Extensions.Localization;
using TechStore.Resources;

namespace TechStore.Controllers {
    [Authorize(Roles = "Admin")]
    public class AdminCategoriesController : Controller {
        private readonly AppDbContext _context;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public AdminCategoriesController(AppDbContext context, IStringLocalizer<SharedResource> localizer) {
            _context = context;
            _localizer = localizer;
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
            if (model.IsFeatured) {
                var featuredCount = await _context.Categories.CountAsync(c => c.IsFeatured);

                if (featuredCount >= 4) {
                    ModelState.AddModelError(string.Empty, _localizer["MaxFeaturedCategories"].Value);
                }
            }

            if (!ModelState.IsValid) {
                return View(model);
            }

            var category = new Category {
                Name = model.Name,
                NameEn = model.NameEn,
                NameAz = model.NameAz,
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
                NameEn = category.NameEn,
                NameAz = category.NameAz,
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

            if (model.IsFeatured) {
                var otherFeaturedCount = await _context.Categories
                    .CountAsync(c => c.IsFeatured && c.Id != id);

                if (otherFeaturedCount >= 4) {
                    ModelState.AddModelError(string.Empty, _localizer["MaxFeaturedCategories"].Value);
                }
            }

            if (!ModelState.IsValid) {
                ViewBag.CategoryId = id;
                return View(model);
            }

            category.Name = model.Name;
            category.NameEn = model.NameEn;
            category.NameAz = model.NameAz;
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
