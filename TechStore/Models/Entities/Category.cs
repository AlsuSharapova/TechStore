namespace TechStore.Models.Entities {
    public class Category {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool IsFeatured { get; set; } = false;
        public ICollection<Product> Products { get; set; } = null!;
    }
}
