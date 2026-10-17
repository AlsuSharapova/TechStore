using System.ComponentModel.DataAnnotations;

namespace TechStore.ViewModels.Admin {
    public class ProductFormViewModel {
        [Required(ErrorMessage = "TitleRequired")]
        [Display(Name = "ProductName")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "DescriptionRequired")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "DescriptionEnRequired")]
        public string? DescriptionEn { get; set; }

        [Required(ErrorMessage = "DescriptionAzRequired")]
        public string? DescriptionAz { get; set; }

        [Required(ErrorMessage = "PriceRequired")]
        [Display(Name = "PriceHeader")]
        [Range(0.01, 1000000, ErrorMessage = "PriceRange")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "QuantityRequired")]
        [Range(0, int.MaxValue, ErrorMessage = "QuantityNegative")]
        [Display(Name = "Stock")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "SelectCategory")]
        public int CategoryId { get; set; }

        public bool IsBestSeller { get; set; }

        public IFormFile? ImageFile { get; set; }

        public List<string> SpecNames { get; set; } = new();
        public List<string> SpecValues { get; set; } = new();
    }
}