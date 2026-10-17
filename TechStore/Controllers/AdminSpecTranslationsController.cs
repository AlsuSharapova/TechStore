using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Data;
using Microsoft.Extensions.Localization;
using TechStore.Resources;

namespace TechStore.Controllers {
    [Authorize(Roles = "Admin")]
    public class AdminSpecTranslationsController : Controller {
        private readonly AppDbContext _context;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public AdminSpecTranslationsController(AppDbContext context, IStringLocalizer<SharedResource> localizer) {
            _context = context;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index() {
            var words = await _context.SpecTranslations
                .OrderBy(t => t.Term)
                .ToListAsync();

            return View(words);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id) {
            var word = await _context.SpecTranslations.FindAsync(id);

            if (word == null) {
                return NotFound();
            }

            return View(word);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string? term, string? en, string? az) {
            var word = await _context.SpecTranslations.FindAsync(id);

            if (word == null) {
                return NotFound();
            }

            term = (term ?? "").Trim();

            if (term == "" || string.IsNullOrWhiteSpace(en) || string.IsNullOrWhiteSpace(az)) {
                return View(word);
            }

            if (term != word.Term) {
                var exists = await _context.SpecTranslations.AnyAsync(t => t.Id != id && t.Term == term);

                if (exists) {
                    TempData["ErrorMessage"] = _localizer["TranslationExists", term].Value;
                    return RedirectToAction("Edit", new { id });
                }

                var oldTerm = word.Term;

                var specs = await _context.ProductSpecifications
                    .Where(s => s.Name == oldTerm || s.Value == oldTerm)
                    .ToListAsync();

                foreach (var s in specs) {
                    if (string.Equals(s.Name, oldTerm, StringComparison.OrdinalIgnoreCase)) s.Name = term;
                    if (string.Equals(s.Value, oldTerm, StringComparison.OrdinalIgnoreCase)) s.Value = term;
                }

                word.Term = term;
            }

            word.En = en;
            word.Az = az;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id) {
            var word = await _context.SpecTranslations.FindAsync(id);

            if (word == null) {
                return NotFound();
            }

            var productNames = await _context.ProductSpecifications
                .Where(s => s.Name == word.Term || s.Value == word.Term)
                .Select(s => s.Product.Name)
                .Distinct()
                .ToListAsync();

            if (productNames.Count > 0) {
                var list = string.Join(", ", productNames.Take(5));

                if (productNames.Count > 5) {
                    list += $" (+{productNames.Count - 5})";
                }

                TempData["ErrorMessage"] = _localizer["TranslationInUse", word.Term, list].Value;
                return RedirectToAction("Index");
            }

            _context.SpecTranslations.Remove(word);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}