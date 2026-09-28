using System.ComponentModel.DataAnnotations;
using TechStore.Models.Entities;

namespace TechStore.ViewModels {
    public class CheckoutViewModel {
        [Required(ErrorMessage = "Введите адрес доставки")]
        [Display(Name = "Адрес доставки")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите номер телефона")]
        [Phone(ErrorMessage = "Некорректный номер телефона")]
        [Display(Name = "Телефон")]
        public string PhoneNumber { get; set; } = string.Empty;

        public Cart? Cart { get; set; }
    }
}