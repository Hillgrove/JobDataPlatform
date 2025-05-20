using Newtonsoft.Json.Linq;
using System.Net;

namespace Extract
{
    public static class SerpApi
    {
        private static string? _apiKey;
        private const string OutputDir = "data/raw";
        private static readonly HttpClient client = new();

        public static async Task Extract(string searchQuery)
        {
            var allJobs = await ScrapeJobs(searchQuery);
            await WriteJobsToJson(allJobs);
        }

        private static async Task<IEnumerable<object>> ScrapeJobs(string searchQuery)
        {
            Console.WriteLine("Scraper SerpApi:");

            var apiKey = await GetApiKeyFromEnvironmentVariable();
            var baseUrl = BuildBaseUrl(searchQuery, apiKey);

            var urlForYesterday = await GetUrlForYesterday(baseUrl);
            if (urlForYesterday is null)
            {
                Console.WriteLine(" Kunne ikke finde 'i går'-filtret via SerpApi.");
                return Enumerable.Empty<object>();
            }

            var allJobs = await FetchJobPages(urlForYesterday, apiKey);
            
            var scrapedAt = DateTime.UtcNow.ToString("yyyy-MM-dd");

            return allJobs.Select(job =>
            {
                if (job is JObject obj)
                {
                    if (obj["detected_extensions"] is JObject ext)
                    {
                        obj["detected_extensions_posted_at"] = ext["posted_at"];
                        obj["detected_extensions_schedule_type"] = ext["schedule_type"];
                        obj.Remove("detected_extensions");
                    }

                    obj.Property("thumbnail")?.Remove();
                }

                job["job_id"] = $"id_{job["job_id"]}";
                job["scrapedAt"] = scrapedAt;
                job["source"] = "serpapi.com";
                return (object)job;
            }).ToList();
        }

        private static async Task WriteJobsToJson(IEnumerable<object> allJobs)
        {
            if (allJobs is null || !allJobs.Any())
            {
                Console.WriteLine("Ingen jobopslag gemt.");
                return;
            }

            Directory.CreateDirectory(OutputDir);
            var filename = $"serpapi_results_{DateTime.UtcNow:yyyy-MM-dd}.ndjson";
            var path = Path.Combine(OutputDir, filename);

            await using var writer = File.CreateText(path);

            foreach (var job in allJobs)
            {
                var serializedJob = Newtonsoft.Json.JsonConvert.SerializeObject(job, Newtonsoft.Json.Formatting.None);
                await writer.WriteLineAsync(serializedJob);
            }

            Console.WriteLine($"Gemte {allJobs.Count()} jobopslag i {path}");
        }
        
        //private static async Task<string> GetApiKeyFromVault()
        //{
        //    if (_apiKey != null) return _apiKey;

        //    var keyVaultUrl = "https://hillvault.vault.azure.net";
        //    var secretName = "SerpApiKey";

        //    var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
        //    var secret = await client.GetSecretAsync(secretName);

        //    _apiKey = secret.Value.Value;
        //    return _apiKey;
        //}
        
        private static Task<string> GetApiKeyFromEnvironmentVariable()
        {
            if (_apiKey != null) return Task.FromResult(_apiKey);

            _apiKey = Environment.GetEnvironmentVariable("SERP_API_KEY");

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException("API-nøgle blev ikke fundet i miljøvariablerne.");
            }

            return Task.FromResult(_apiKey);
        }

        private static string BuildBaseUrl(string searchQuery, string apiKey)
        {
            var serpApiUrl = "https://serpapi.com/search.json";
            var queryParams = GetBaseQueryParams(searchQuery, apiKey);
            var queryString = BuildQueryString(queryParams);

            return $"{serpApiUrl}?{queryString}";
        }

        private static async Task<string?> GetUrlForYesterday(string url)
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

        private static async Task<JArray> FetchJobPages(string startUrl, string apiKey)
        {
            var allJobs = new JArray();
            var nextUrl = startUrl;
            int page = 1;

            while (!string.IsNullOrEmpty(nextUrl))
            {
                try
                {
                    Console.WriteLine($" [Side {page++}] Henter data..");

                    if (!nextUrl.Contains("api_key="))
                        nextUrl += $"&api_key={apiKey}";

                    var response = await client.GetStringAsync(nextUrl);
                    var jobs = ParseJobsFromResponse(response);

                    if (jobs != null)
                    {
                        allJobs.Merge(jobs);
                        Console.WriteLine($"  Fundet {jobs.Count} jobs");
                    }

                    var json = JObject.Parse(response);
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

            return allJobs;
        }
        
        private static JArray? ParseJobsFromResponse(string response)
        {
            try
            {
                var json = JObject.Parse(response);
                return json["jobs_results"] as JArray;
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                Console.WriteLine($" JSON Fejl: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Fejl: {ex.Message}");
                return null;
            }
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

        private static string GetApiKeyFromEnvironment()
        {
            if (_apiKey != null) return _apiKey;

            _apiKey = Environment.GetEnvironmentVariable("SERP_API_KEY");

            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("API key not found in environment variables.");
            }

            return _apiKey;
        }
    }
}
