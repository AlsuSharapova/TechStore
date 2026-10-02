using TechStore.Models.Entities;

namespace TechStore.ViewModels.Admin {
    public class DashboardViewModel {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
    }
}