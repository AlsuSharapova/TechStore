using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TechStore.Data;
using TechStore.Models;
using TechStore.Models.Entities;
using TechStore.ViewModels;

namespace TechStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly IOptions<StoreSettings> _storeSettings;
        public HomeController(ILogger<HomeController> logger, AppDbContext context, IOptions<StoreSettings> storeSettings)
        {
            _logger = logger;
            _context = context;
            _storeSettings = storeSettings;
            
        }

        public async Task<IActionResult> Index() {
            var viewModel = new HomeViewModel {
                Categories = await _context.Categories
                .Where(c => c.IsFeatured)
                .Take(4)
                .ToListAsync(),
                BestSellers = await _context.Products
                    .Where(p => p.IsBestSeller)
                    .Include(p => p.Category)
                    .Include(p => p.Specifications)
                    .Take(4)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult About() {
            var categories = _context.Categories
                .Where(c => c.IsFeatured)
                .Take(4)
                .ToList();

            return View(categories);
        }

        public IActionResult Contacts() {
            return View(_storeSettings.Value);
        }
    }
}
