namespace TechStore.Models.Entities {
    public class Order {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public string DeliveryAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.New;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}