using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Extract
{
    public static class SerpApi
    {
        private static string? _apiKey;
        private static readonly HttpClient client = new();

        public static async Task Extract(string searchQuery)
        {
            var allJobs             = new JArray();
            
            var serpApiUrl          = "https://serpapi.com/search.json";
            var apiKey              = await GetApiKeyFromVault();
            var queryParams         = GetBaseQueryParams(searchQuery, apiKey);
            var QueryString         = BuildQueryString(queryParams);
            var baseUrl             = $"{serpApiUrl}?{QueryString}";

            var urlForYesterday     = await GetUrlForYesterday(client, baseUrl);
            if (urlForYesterday is null)
            {
                Console.WriteLine("Kunne ikke finde 'i går'-filret via SerpApi.");
                return;
            }
            var nextUrl             = urlForYesterday;

            int page = 1;
            while (!string.IsNullOrEmpty(nextUrl))
            {
                try
                {
                    Console.WriteLine($"[Side {page++}] Henter data..");

                    if (!nextUrl.Contains("api_key="))
                        nextUrl += $"&api_key={apiKey}";

                    var response = await client.GetStringAsync(nextUrl);
                    var json = JObject.Parse(response);

                    if (json["jobs_results"] is JArray jobs)
                    {
                        allJobs.Merge(jobs);
                        Console.WriteLine($"Fundet {jobs?.Count ?? 0} jobs");
                    }

                    nextUrl = json["serpapi_pagination"]?["next"]?.ToString();
                    await Task.Delay(1000); // Delay to avoid hitting the rate limit
                }
                
                catch (HttpRequestException httpEx)
                {
                    Console.WriteLine($"HTTP Fejl: {httpEx.Message}");
                    break;
                }
                
                catch (Exception ex)
                {
                    Console.WriteLine($"Fejl: {ex.Message}");
                    break;
                }
            }

            Directory.CreateDirectory("data/raw");
            var filename = $"data/raw/serpapi_results_{DateTime.Now:yyyy-MM-dd}.json";
            File.WriteAllText(filename, allJobs.ToString());
            Console.WriteLine($"Gemte {allJobs.Count} jobopslag i {filename}");
        }


        private static async Task<string> GetApiKeyFromVault()
        {
            if (_apiKey != null) return _apiKey;

            var keyVaultUrl = "https://hillvault.vault.azure.net";
            var secretName = "SerpApiKey";

            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            var secret = await client.GetSecretAsync(secretName);

            _apiKey = secret.Value.Value;
            return _apiKey;
        }

        private static async Task<string?> GetUrlForYesterday(HttpClient client, string url)
        {            
            var response = await client.GetStringAsync(url);
            var json = JObject.Parse(response);

            if (json["filters"] is not JArray filters) return null;

            var dateFilter = filters.FirstOrDefault(f => f["name"]?.ToString() == "Opslagsdato");
            if (dateFilter is null) return null;

            var yesterdayOption = dateFilter["options"]?
                .FirstOrDefault(o => o["name"]?.ToString()?.Contains("i går", StringComparison.OrdinalIgnoreCase) == true);

            var urlForYesterday = yesterdayOption?["serpapi_link"]?.ToString();
            return urlForYesterday;
        }

        private static Dictionary<string, string> GetBaseQueryParams(string searchQuery, string apiKey)
        {
            return new Dictionary<string, string>
            {
                { "engine", "google_jobs" },
                { "q", searchQuery },
                { "location", "Denmark" },
                { "google_domain", "google.dk" },
                { "gl", "dk" },
                { "hl", "da" },
                { "api_key", apiKey }
            };
        }

        private static string BuildQueryString(Dictionary<string, string> parameters)
        {
            return string.Join("&", parameters.Select(kvp =>
                $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));
        }
    }
}
