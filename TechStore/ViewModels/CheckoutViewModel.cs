using System.ComponentModel.DataAnnotations;
using TechStore.Models.Entities;

namespace TechStore.ViewModels {
    public class CheckoutViewModel {
        [Required(ErrorMessage = "AddressRequired")]
        [Display(Name = "DeliveryAddressField")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "PhoneRequired")]
        [Phone(ErrorMessage = "PhoneInvalid")]
        [Display(Name = "PhoneField")]
        public string PhoneNumber { get; set; } = string.Empty;

        public Cart? Cart { get; set; }
    }
}