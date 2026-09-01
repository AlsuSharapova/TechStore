using TechStore.Models.Entities;

namespace TechStore.ViewModels {
    public class ProductsIndexViewModel {

        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        public int? SelectedCategoryId { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string ? Search { get; set; }
    }
}
