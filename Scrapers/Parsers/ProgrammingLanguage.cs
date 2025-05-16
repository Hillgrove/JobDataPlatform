using System.Text.Json;
using System.Text.RegularExpressions;

namespace Scrapers.Parser
{
    internal class ProgrammingLanguage
    {
        private static readonly List<string> _languages = new();

        public static void Load(string jsonPath)
        {
            var json = File.ReadAllText(jsonPath);
            var languages = JsonSerializer.Deserialize<List<string>>(json)!;
            _languages.AddRange(languages);
        }

        public static List<string> Extract(string text)
        {
            var foundLanguages = new List<string>();
            foreach (var language in _languages)
            {
                bool requiresExact = language.Contains('+') || language.Contains('#');

                string pattern = requiresExact
                    ? $@"(?<!\w){Regex.Escape(language)}(?!\w)"
                    : $@"\b{Regex.Escape(language)}\b";

                if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
                {
                    foundLanguages.Add(language);
                }
            }
            return foundLanguages;
        }
    }
}
