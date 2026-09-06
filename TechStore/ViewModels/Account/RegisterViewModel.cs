using System.ComponentModel.DataAnnotations;

namespace TechStore.ViewModels.Account {
    public class RegisterViewModel {
        [Required(ErrorMessage = "Введите имя")]
        [Display(Name = "Имя")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "Пароль должен содержать от {2} до {1} символов.")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Подтвердите пароль")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [Display(Name = "Подтверждение пароля")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
