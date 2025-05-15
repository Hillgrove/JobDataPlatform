using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Scrapers
{
    internal class RawDataExtractor_SerpAPI
    {
        private static string? _apiKey;

        public static async Task Run(string searchQuery)
        {
            var apiKey = await GetApiKeyFromVault();

            var encodedQuery = WebUtility.UrlEncode(searchQuery);
            var baseUrl = $"https://serpapi.com/search.json?engine=google_jobs&q={encodedQuery}&location=Denmark&google_domain=google.dk&gl=dk&hl=da&api_key={apiKey}";

            using var client = new HttpClient();
            var allJobs = new JArray();
            string? nextUrl = $"{baseUrl}&api_key={apiKey}";
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
    }
}
