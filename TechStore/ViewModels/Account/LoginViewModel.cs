using System.ComponentModel.DataAnnotations;

namespace TechStore.ViewModels.Account {
    public class LoginViewModel {
        [Required(ErrorMessage = "EmailRequired")]
        [EmailAddress(ErrorMessage = "EmailInvalid")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "PasswordRequired")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "RememberMe")]
        public bool RememberMe { get; set; }
    }
}