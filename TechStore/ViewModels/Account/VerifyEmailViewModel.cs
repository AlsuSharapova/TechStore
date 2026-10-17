using System.ComponentModel.DataAnnotations;

namespace TechStore.ViewModels.Account {
    public class VerifyEmailViewModel {
        [Required(ErrorMessage = "EmailRequired")]
        [EmailAddress(ErrorMessage = "EmailInvalid")]
        public string Email { get; set; } = string.Empty;
    }
}