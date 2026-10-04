namespace TechStore.Models.Entities {
    public enum OrderStatus {
        AwaitingPayment,
        New,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }
}