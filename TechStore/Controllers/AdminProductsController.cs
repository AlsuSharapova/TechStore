using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Data;
using TechStore.Models.Entities;
using TechStore.ViewModels.Admin;

namespace TechStore.Controllers {
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : Controller {
        private readonly AppDbContext _context;

        public AdminProductsController(AppDbContext context) {
            _context = context;
        }

        public async Task<IActionResult> Index() {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create() {
            ProductFormViewModel viewModel = new ProductFormViewModel();
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductFormViewModel model) {
            if (!ModelState.IsValid) {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View(model);
            }

            var product = new Product {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                CategoryId = model.CategoryId,
                IsBestSeller = model.IsBestSeller
            };

            // Загрузка картинки
            if (model.ImageFile != null && model.ImageFile.Length > 0) {
                var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                var savePath = Path.Combine("wwwroot/images/products", fileName);

                using (var stream = new FileStream(savePath, FileMode.Create)) {
                    await model.ImageFile.CopyToAsync(stream);
                }

                product.ImageUrl = "/images/products/" + fileName;
            }
            else {
                product.ImageUrl = "/images/products/no-image.png";
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Сохраняем характеристики
            for (int i = 0; i < model.SpecNames.Count; i++) {
                if (!string.IsNullOrWhiteSpace(model.SpecNames[i]) && !string.IsNullOrWhiteSpace(model.SpecValues[i])) {
                    _context.ProductSpecifications.Add(new ProductSpecification {
                        Name = model.SpecNames[i],
                        Value = model.SpecValues[i],
                        ProductId = product.Id
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}