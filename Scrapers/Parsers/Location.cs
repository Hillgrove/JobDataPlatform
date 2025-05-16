using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Scrapers.Parser
{
    internal static class Location
    {
        private static Dictionary<string, List<string>> _cityToPostalList = new(StringComparer.OrdinalIgnoreCase);
        private static HashSet<string> _postalCodes = new();

        public static void Load(string jsonPath)
        {
            var json = File.ReadAllText(jsonPath);
            var items = JsonSerializer.Deserialize<List<PostalCodeRecord>>(json)!;

            foreach (var item in items)
            {
                var city = item.Name.Trim();

                if (!_cityToPostalList.ContainsKey(city))
                {
                    _cityToPostalList[city] = new List<string>();
                }

                _cityToPostalList[city].Add(item.Code);

                // Ekstra: Tilføj kommune-navn som alternativ nøgle
                if (item.Municipalities?.Count > 0)
                {
                    var kommuneNavn = item.Municipalities[0].Name.Trim();
                    if (!_cityToPostalList.ContainsKey(kommuneNavn))
                        _cityToPostalList[kommuneNavn] = new List<string>();

                    _cityToPostalList[kommuneNavn].Add(item.Code);
                }

                _postalCodes.Add(item.Code);
            }
        }

        public static string? Extract(string text)
        {
            foreach (var kvp in _cityToPostalList)
            {
                if (text.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value.FirstOrDefault();
                }
            }

            var match = Regex.Match(text, @"\b\d{4}\b");
            if (match.Success && _postalCodes.Contains(match.Value))
            {
                return match.Value;
            }

            return null;
        }
    }

    
    internal class PostalCodeRecord
    {
        [JsonPropertyName("nr")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("navn")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("kommuner")]
        public List<MunicipalityRecord>? Municipalities { get; set; }
    }

    internal class MunicipalityRecord
    {
        [JsonPropertyName("navn")]
        public string Name { get; set; } = string.Empty;
    }
}
