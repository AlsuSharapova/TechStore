using System.Globalization;
using TechStore.Models.Entities;

namespace TechStore.Helpers {
    public static class ContentLocalizationExtensions {
        private static string Pick(string ru, string? en, string? az) {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var result = lang switch { "en" => en, "az" => az, _ => ru };
            return string.IsNullOrWhiteSpace(result) ? ru : result;
        }

        public static string GetName(this Category c) => Pick(c.Name, c.NameEn, c.NameAz);
        public static string GetDescription(this Product p) => Pick(p.Description, p.DescriptionEn, p.DescriptionAz);
    }
}