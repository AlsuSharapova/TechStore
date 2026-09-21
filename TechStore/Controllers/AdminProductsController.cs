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

        [HttpGet]
        public async Task<IActionResult> Edit(int id) {
            var product = await _context.Products
                .Include(p => p.Specifications)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) {
                return NotFound();
            }

            var model = new ProductFormViewModel {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                IsBestSeller = product.IsBestSeller,
                SpecNames = product.Specifications.Select(s => s.Name).ToList(),
                SpecValues = product.Specifications.Select(s => s.Value).ToList()
            };

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.ProductId = product.Id;
            ViewBag.CurrentImageUrl = product.ImageUrl;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProductFormViewModel model) {
            var product = await _context.Products
                .Include(p => p.Specifications)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) {
                return NotFound();
            }

            if (!ModelState.IsValid) {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.ProductId = id;
                ViewBag.CurrentImageUrl = product.ImageUrl;
                return View(model);
            }

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.StockQuantity = model.StockQuantity;
            product.CategoryId = model.CategoryId;
            product.IsBestSeller = model.IsBestSeller;

            // Заменяем картинку, только если загрузили новую
            if (model.ImageFile != null && model.ImageFile.Length > 0) {
                var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                var savePath = Path.Combine("wwwroot/images/products", fileName);

                using (var stream = new FileStream(savePath, FileMode.Create)) {
                    await model.ImageFile.CopyToAsync(stream);
                }

                // Удаляем старую картинку, если это не заглушка
                if (product.ImageUrl != "/images/products/no-image.png") {
                    var oldPath = Path.Combine("wwwroot", product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) {
                        System.IO.File.Delete(oldPath);
                    }
                }

                product.ImageUrl = "/images/products/" + fileName;
            }

            // Пересобираем характеристики полностью
            _context.ProductSpecifications.RemoveRange(product.Specifications);

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

        [HttpPost]
        public async Task<IActionResult> Delete(int id) {
            var product = await _context.Products
                .Include(p => p.Specifications)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) {
                return NotFound();
            }

            // Удаляем картинку с диска, если это не заглушка
            if (product.ImageUrl != "/images/products/no-image.png") {
                var imagePath = Path.Combine("wwwroot", product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath)) {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.ProductSpecifications.RemoveRange(product.Specifications);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}