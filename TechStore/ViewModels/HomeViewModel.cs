using TechStore.Models.Entities;

namespace TechStore.ViewModels {
    public class HomeViewModel {
        public List<Category> Categories { get; set; } = null!;
        public List<Product> BestSellers { get; set; } = null!;
    }
}
