using System.ComponentModel.DataAnnotations;

namespace TechStore.ViewModels.Account {
    public class ResetPasswordViewModel {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "NewPasswordRequired")]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "PasswordLength")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "ConfirmPasswordRequired")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "PasswordsDoNotMatch")]
        [Display(Name = "ConfirmPassword")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "PasswordLength")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}