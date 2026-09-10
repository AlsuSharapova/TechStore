using System.ComponentModel.DataAnnotations;

namespace TechStore.ViewModels.Account {
    public class ResetPasswordViewModel {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите новый пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Подтвердите пароль")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [Display(Name = "Подтверждение пароля")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}