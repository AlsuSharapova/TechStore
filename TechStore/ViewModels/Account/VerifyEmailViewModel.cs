using System.ComponentModel.DataAnnotations;

namespace TechStore.ViewModels.Account {
    public class VerifyEmailViewModel {
        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        public string Email { get; set; } = string.Empty;
    }
}
