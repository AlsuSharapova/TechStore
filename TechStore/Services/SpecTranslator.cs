using System.Globalization;
using TechStore.Data;
using TechStore.Models.Entities;

namespace TechStore.Services {
    public class SpecTranslator {
        private readonly Dictionary<string, SpecTranslation> _words;

        public SpecTranslator(AppDbContext context) {
            _words = context.SpecTranslations.ToDictionary(t => t.Term, StringComparer.OrdinalIgnoreCase);
        }

        public string Translate(string text) {
            if (!_words.TryGetValue(text, out var word)) {
                return text;
            }

            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var translation = lang switch { "en" => word.En, "az" => word.Az, _ => null };

            return string.IsNullOrWhiteSpace(translation) ? text : translation;
        }
    }
}