using TechStore.Models.Entities;

namespace TechStore.ViewModels {
    public class ProductsIndexViewModel {

        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        public int? SelectedCategoryId { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string ? Search { get; set; }
        public string? SortBy { get; set; }
        public List<string> SelectedSpecs { get; set; } = new();
        public List<SpecFilterGroup> AvailableSpecFilters { get; set; } = new();

    }
    public class SpecFilterGroup {
        public string Name { get; set; } = string.Empty;
        public List<string> Values { get; set; } = new();
    }
}
