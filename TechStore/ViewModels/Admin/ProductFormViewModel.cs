using System.ComponentModel.DataAnnotations;

namespace TechStore.ViewModels.Admin {
    public class ProductFormViewModel {
        [Required(ErrorMessage = "Введите название")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите описание")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите цену")]
        [Range(0.01, 1000000, ErrorMessage = "Цена должна быть больше 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Введите количество")]
        [Range(0, int.MaxValue, ErrorMessage = "Количество не может быть отрицательным")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Выберите категорию")]
        public int CategoryId { get; set; }

        public bool IsBestSeller { get; set; }

        public IFormFile? ImageFile { get; set; }

        public List<string> SpecNames { get; set; } = new();
        public List<string> SpecValues { get; set; } = new();
    }
}