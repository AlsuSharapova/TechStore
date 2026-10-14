using System.ComponentModel.DataAnnotations;

namespace TechStore.ViewModels.Admin {
    public class CategoryFormViewModel {
        [Display(Name = "Название категории")]
        [Required(ErrorMessage = "Введите название")]     
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Введите название")]
        public string? NameEn { get; set; }
        [Required(ErrorMessage = "Введите название")]
        public string? NameAz { get; set; }

        [Display(Name = "Иконка")]
        [Required(ErrorMessage = "Введите иконку (эмодзи)")]
        public string Icon { get; set; } = string.Empty;

        [Display(Name = "Популярная?")]
        public bool IsFeatured { get; set; }
    }
}