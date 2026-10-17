namespace TechStore.Models.Entities {
    public class SpecTranslation {
        public int Id { get; set; }
        public string Term { get; set; } = string.Empty;
        public string? En { get; set; }
        public string? Az { get; set; }
    }
}