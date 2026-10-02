namespace TechStore.ViewModels.Admin {
    public class AdminUserListItem {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CurrentRole { get; set; } = string.Empty;
        public bool IsLockedOut { get; set; }
    }
}