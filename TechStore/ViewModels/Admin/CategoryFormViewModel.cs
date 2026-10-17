using System.ComponentModel.DataAnnotations;

namespace TechStore.ViewModels.Admin {
    public class CategoryFormViewModel {
        [Display(Name = "ProductName")]
        [Required(ErrorMessage = "TitleRequired")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "TitleEnRequired")]
        public string? NameEn { get; set; }

        [Required(ErrorMessage = "TitleAzRequired")]
        public string? NameAz { get; set; }

        [Display(Name = "Icon")]
        [Required(ErrorMessage = "IconRequired")]
        public string Icon { get; set; } = string.Empty;

        [Display(Name = "OnHomepage")]
        public bool IsFeatured { get; set; }
    }
}