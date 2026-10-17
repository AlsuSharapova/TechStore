using System.ComponentModel.DataAnnotations;

namespace TechStore.ViewModels.Account {
    public class RegisterViewModel {
        [Required(ErrorMessage = "NameRequired")]
        [Display(Name = "FullName")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "EmailRequired")]
        [EmailAddress(ErrorMessage = "EmailInvalid")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "PasswordRequired")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "PasswordLength")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "ConfirmPasswordRequired")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "PasswordsDoNotMatch")]
        [Display(Name = "ConfirmPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}